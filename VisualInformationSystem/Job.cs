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
                JobManager = App.Manager.JobManager;
                JobId = JobManager.NextJobID;
                JobFinished = true;
                LastExecute = new TimeSpan();
            }


            public virtual void prepareJob()
            {
            }


            public virtual void finalizeJob()
            {
            }

            public virtual bool handleException() => false;

            public JobManager JobManager
            {
                get;
                private set;
            }

            public int JobId
            {
                get;
                private set;
            }

            public bool JobFinished
            {
                get;
                protected set;
            }

            public TimeSpan LastExecute
            {
                get;
                set;
            }
        }
    }
}
