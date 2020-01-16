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

            int jobCountLastUpdate_ = 0;
            int queuedJobCountFinished_ = 0;
            int queuedJobCountExecutes_ = 0;
            public void getStatistic(ref int jobCountLastUpdate, ref int queuedJobCountFinished, ref int queuedJobCountExecutes)
            {
                jobCountLastUpdate = jobCountLastUpdate_;
                queuedJobCountFinished = queuedJobCountFinished_;
                queuedJobCountExecutes = queuedJobCountExecutes_;

                jobCountLastUpdate_ = 0;
                queuedJobCountFinished_ = 0;
                queuedJobCountExecutes_ = 0;
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
                        log(Console.LogType.Error, $"Job with id {job.JobId} already registered");
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
            Queue<Job> queuedJobs_ = new Queue<Job>();
            Job curQueuedJob_ = null;
            TimeSpan curQueuedJobLastExecute_ = new TimeSpan(0);

            public bool queueJob(Job job)
            {
                if (job != null)
                    queuedJobs_.Enqueue(job);
                return true;
            }
            #endregion // Queued Jobs

            bool toggleRun_ = false;
            public override void tick(TimeSpan delta)
            {
                if ((toggleRun_ = !toggleRun_) == true)
                {
                    // process queued jobs
                    if (curQueuedJob_ != null)
                    {
                        curQueuedJob_.tick(Manager.Timer.Ticks - curQueuedJobLastExecute_);
                        curQueuedJobLastExecute_ = Manager.Timer.Ticks;
                        queuedJobCountExecutes_++;

                        if (curQueuedJob_.JobFinished)
                        {
                            curQueuedJob_.finalizeJob();
                            curQueuedJob_.LastExecute = Manager.Timer.Ticks;
                            queuedJobCountFinished_++;
                            curQueuedJob_ = null;
                        }
                    }
                    else if (queuedJobs_.Count > 0)
                    {
                        curQueuedJob_ = queuedJobs_.Dequeue();
                        curQueuedJob_.prepareJob();
                        curQueuedJobLastExecute_ = Manager.Timer.Ticks;
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
