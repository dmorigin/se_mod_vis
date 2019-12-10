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
        public class DataCollectorProperty<T> : DataCollector<T> where T: class
        {
            public DataCollectorProperty(string typeId, Configuration.Options options)
                : base(typeId, options)
            {
            }

            public override bool construct()
            {
                if (!base.construct())
                    return false;

                if (Options.Count == 3)
                {
                    // get property
                    property_ = Options[2];
                    Constructed = true;
                    return true;
                }

                Constructed = false;
                return false;
            }

            string property_ = "";
            public string Property
            {
                get { return property_; }
            }

            public override DataRetriever getDataRetriever(string name)
            {
                return null;
            }
        }
    }
}
