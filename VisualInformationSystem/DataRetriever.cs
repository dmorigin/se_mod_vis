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
        public abstract class DataRetriever
        {
            public struct ListContainer
            {
                public string name;
                public double min;
                public double max;
                public double indicator;
                public double value;
                public MyItemType type;
            }

            public abstract double getIndicator();
            public abstract double getValue();
            public abstract void getList(out List<ListContainer> container);
            public abstract double getMin();
            public abstract double getMax();
        }
    }
}
