using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class JobManager : RuntimeObject
        {
            public JobManager()
                : base("JobManager")
            {
            }

            static int nextJobId_ = 1;
            public int NextJobID
            {
                get { return ++nextJobId_; }
            }

            //public struct JobstatisticInfo
            //{
            //    public int id;
            //    public string name;
            //    public int count;
            //}

            int jobCountLastUpdate_ = 0;
            int queuedJobCountFinished_ = 0;
            int queuedJobCountExecutes_ = 0;
            //List<JobstatisticInfo> jobsExecuted_ = new List<JobstatisticInfo>();
            public void getStatistic(ref int jobCountLastUpdate, ref int queuedJobCountFinished, ref int queuedJobCountExecutes)
                //ref List<JobstatisticInfo> jobInfo)
            {
                jobCountLastUpdate = jobCountLastUpdate_;
                queuedJobCountFinished = queuedJobCountFinished_;
                queuedJobCountExecutes = queuedJobCountExecutes_;
                //jobInfo = jobsExecuted_.GetRange(0, jobsExecuted_.Count);

                jobCountLastUpdate_ = 0;
                queuedJobCountFinished_ = 0;
                queuedJobCountExecutes_ = 0;
                //jobsExecuted_.Clear();
            }

            #region Timed Jobs
            List<JobTimed> timedJobs_ = new List<JobTimed>();

            public bool registerTimedJob(JobTimed job)
            {
                if (job != null)
                {
                    if (getTimedJob(job.JobId) == null)
                    {
                        timedJobs_.Add(job);
                        return true;
                    }
                    else
                        log(Console.LogType.Error, $"Job '{job.JobId}' already registered");
                }

                return false;
            }

            public bool unregisterTimedJob(JobTimed job)
            {
                if (job == null)
                    return false;

                return timedJobs_.Remove(job);
            }

            public bool unregisterTimedJob(int id)
            {
                return unregisterTimedJob(getTimedJob(id));
            }

            public JobTimed getTimedJob(int id)
            {
                foreach(var job in timedJobs_)
                {
                    if (job.JobId == id)
                        return job;
                }

                return null;
            }

            public JobTimed getTimedJob(string name)
            {
                foreach(var job in timedJobs_)
                {
                    if (job.Name == name)
                    {
                        log(Console.LogType.Debug, $"Job {name} found");
                        return job;
                    }
                }

                return null;
            }

            JobTimed getNextTimedJob()
            {
                JobTimed nextJob = null;
                TimeSpan awaiting = new TimeSpan(0);

                foreach (var job in timedJobs_)
                {
                    // first execute
                    if (job.NextExecute == job.LastExecute)
                    {
                        job.LastExecute = Manager.Timer.Ticks;
                        return job;
                    }

                    TimeSpan wait = Manager.Timer.Ticks - job.NextExecute;
                    if (wait > awaiting)
                    {
                        nextJob = job;
                        awaiting = wait;
                    }
                }

                if (awaiting <= Manager.Timer.Ticks)
                    return nextJob;
                return null;
            }
            #endregion // Timed Jobs

            #region Queued Jobs
            //Queue<Job> queuedJobs_ = new Queue<Job>();
            LinkedList<Job> queuedJobs_ = new LinkedList<Job>();
            Job curQueuedJob_ = null;
            TimeSpan curQueuedJobLastExecute_ = new TimeSpan(0);

            public void queueJob(Job job, bool front = false)
            {
                if (job != null)
                {
                    if (!front)
                        queuedJobs_.AddLast(job);
                    else
                        queuedJobs_.AddFirst(job);
                }
            }
            #endregion // Queued Jobs

            bool toggleRun_ = false;
            //JobstatisticInfo jobInfo_ = new JobstatisticInfo();
            public override void tick(TimeSpan delta)
            {
                if ((toggleRun_ = !toggleRun_) == true)
                {
                    try
                    {
                        // process queued jobs
                        if (curQueuedJob_ != null)
                        {
                            curQueuedJob_.tick(Manager.Timer.Ticks - curQueuedJobLastExecute_);
                            curQueuedJobLastExecute_ = Manager.Timer.Ticks;
                            queuedJobCountExecutes_++;
                            //jobInfo_.count++;

                            if (curQueuedJob_.JobFinished)
                            {
                                curQueuedJob_.finalizeJob();
                                curQueuedJob_.LastExecute = Manager.Timer.Ticks;
                                queuedJobCountFinished_++;
                                curQueuedJob_ = null;

                                //statistic
                                //jobsExecuted_.Add(jobInfo_);
                            }
                        }
                        else if (queuedJobs_.Count > 0)
                        {
                            curQueuedJob_ = queuedJobs_.First.Value;
                            queuedJobs_.RemoveFirst();
                            curQueuedJob_.prepareJob();
                            curQueuedJobLastExecute_ = Manager.Timer.Ticks;

                            // statistic
                            //jobInfo_.id = curQueuedJob_.JobId;
                            //jobInfo_.name = curQueuedJob_.Name;
                            //jobInfo_.count = 0;
                        }
                    }
                    catch(Exception exp)
                    {
                        if (!curQueuedJob_.handleException())
                            throw exp;
                        curQueuedJob_ = null;
                    }
                }
                else
                {
                    // we have a timed job...
                    JobTimed timedJob = getNextTimedJob();
                    if (timedJob != null)
                    {
                        timedJob.tick(Manager.Timer.Ticks - timedJob.LastExecute);
                        timedJob.LastExecute = Manager.Timer.Ticks;
                        timedJob.NextExecute = timedJob.LastExecute + timedJob.Interval;
                        jobCountLastUpdate_++;
                    }
                }
            }
        }
    }
}
