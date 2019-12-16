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
        public class Job : RuntimeObject
        {
            public Job()
            {
                jobManager_ = App.Manager.JobManager;
                jobId_ = jobManager_.NextJobID;
            }


            public virtual void prepareJob()
            {
            }


            public virtual void finalizeJob()
            {
            }

            bool finished_ = true;
            public bool JobFinished
            {
                get { return finished_; }
                protected set { finished_ = value; }
            }

            JobManager jobManager_ = null;
            public JobManager JobManager
            {
                get { return jobManager_; }
            }

            TimeSpan lastExecute = new TimeSpan();
            public TimeSpan LastExecute
            {
                get { return lastExecute; }
                set { lastExecute = value; }
            }

            int jobId_ = 0;
            public int JobID
            {
                get { return jobId_; }
            }
        }
    }
}
