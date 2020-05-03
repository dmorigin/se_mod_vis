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
        public class JobTimed : Job
        {
            public JobTimed(string name)
                : base(name)
            {
                Interval = Default.Refresh;
                NextExecute = new TimeSpan(0);
            }

            //TimeSpan interval_ = Default.Refresh;
            public virtual TimeSpan Interval
            {
                get;// { return interval_; }
                set;// { interval_ = value; }
            }


            //TimeSpan nextExecute_ = new TimeSpan(0);
            public TimeSpan NextExecute
            {
                get;// { return nextExecute_; }
                set;// { nextExecute_ = value; }
            }
        }
    }
}
