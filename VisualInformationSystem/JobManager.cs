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

            #region Timed Jobs
            List<JobTimed> timedJobs_ = new List<JobTimed>();

            public bool registerTimedJob(JobTimed job)
            {
                if (job != null)
                {
                    if (getTimedJob(job.JobID) == null)
                    {
                        timedJobs_.Add(job);
                        return true;
                    }
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
                    if (job.JobID == id)
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

            public bool queueJob(Job job)
            {
                queuedJobs_.Enqueue(job);
                return true;
            }
            #endregion // Queued Jobs

            public override void tick(TimeSpan delta)
            {
                // process queued jobs
                if (queuedJobs_.Count > 0)
                {
                    Job queuedJob = queuedJobs_.Dequeue();
                    queuedJob.tick(Manager.Timer.Ticks - queuedJob.LastExecute);
                    queuedJob.LastExecute = Manager.Timer.Ticks;
                }

                // we have a timed job...
                JobTimed timedJob = getNextTimedJob();
                if (timedJob != null)
                {
                    timedJob.tick(Manager.Timer.Ticks - timedJob.LastExecute);
                    timedJob.LastExecute = Manager.Timer.Ticks;
                    timedJob.NextExecute = timedJob.LastExecute + timedJob.Interval;
                    log(Console.LogType.Debug, $"Execute timed job({timedJob.JobID}) at {Manager.Timer.Ticks}");
                }
            }
        }
    }
}
