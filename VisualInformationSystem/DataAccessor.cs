﻿using Sandbox.Game.EntityComponents;
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
        public abstract class DataAccessor
        {
            public struct ListContainer
            {
                public string name;
                public VISUnitType min;
                public VISUnitType max;
                public double indicator;
                public VISUnitType value;
                public MyItemType type;
            }

            public abstract double indicator();
            public abstract VISUnitType value();
            public abstract VISUnitType min();
            public abstract VISUnitType max();
            public abstract void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null);
        }
    }
}
