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
        public class DataCollectorLandingGear : DataCollector<IMyLandingGear>
        {
            public DataCollectorLandingGear(Configuration.Options options)
                : base("landinggear", "", options)
            {
            }

            protected override void update()
            {
                locked_ = 0;

                foreach(var gear in Blocks)
                {
                    blocksOn_ += isOn(gear) ? 1 : 0;
                    locked_ += gear.IsLocked ? 1 : 0;
                }

                UpdateFinished = true;
            }

            int locked_ = 0;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%locked%", locked_.ToString())
                    .Replace("%ratio%", new ValueType(locked_ / (double)Blocks.Count, unit: Unit.Percent).pack().ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                if (name.ToLower() == "status")
                    return new Status(this);
                return base.getDataAccessor(name);
            }

            class Status : DataAccessor
            {
                DataCollectorLandingGear dc_;
                public Status(DataCollectorLandingGear dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.locked_ / (double)dc_.Blocks.Count;
                public override ValueType min() => new ValueType(0);
                public override ValueType max() => new ValueType(dc_.Blocks.Count);
                public override ValueType value() => new ValueType(dc_.locked_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var gear in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = gear.CustomName;
                        item.indicator = gear.IsLocked ? 1 : 0;
                        item.min = new ValueType(0);
                        item.max = new ValueType(1);
                        item.value = new ValueType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
