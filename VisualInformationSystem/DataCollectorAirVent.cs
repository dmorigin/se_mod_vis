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
        public class DataCollectorAirVent : DataCollector<IMyAirVent>
        {
            public DataCollectorAirVent(Configuration.Options options)
                : base("airvent", "", options)
            {
            }

            protected override void update()
            {
                oxygenLevel_ = 0f;
                foreach (var airvent in Blocks)
                {
                    oxygenLevel_ += airvent.GetOxygenLevel();
                    pressurizeAble_ += airvent.CanPressurize && !airvent.Depressurize ? 1 : 0;
                    blocksOn_ += isOn(airvent) ? 1 : 0;
                }

                oxygenLevel_ /= Blocks.Count;
                UpdateFinished = true;
            }

            float oxygenLevel_ = 0f;
            int pressurizeAble_ = 0;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%pressurizeable%", pressurizeAble_.ToString())
                    .Replace("%oxygenlevel%", new VISUnitType(oxygenLevel_, unit: Unit.Percent));
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "oxygenlevel":
                        return new OxygenLevel(this);
                    case "pressurizeable":
                        return new PressurizeAble(this);
                }

                return base.getDataAccessor(name);
            }

            class PressurizeAble : DataAccessor
            {
                DataCollectorAirVent dc_ = null;
                public PressurizeAble(DataCollectorAirVent obj)
                {
                    dc_ = obj;
                }

                public override double indicator() => (double)dc_.pressurizeAble_ / (double)dc_.Blocks.Count;
                public override VISUnitType min() => new VISUnitType(0.0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType();

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var airvent in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.min = new VISUnitType(0.0);
                        item.max = new VISUnitType(1.0);
                        item.value = new VISUnitType(airvent.CanPressurize && !airvent.Depressurize ? 1.0 : 0.0);
                        item.indicator = item.value.Value;
                        item.name = airvent.CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class OxygenLevel : DataAccessor
            {
                DataCollectorAirVent dc_ = null;
                public OxygenLevel(DataCollectorAirVent obj)
                {
                    dc_ = obj;
                }

                public override double indicator() => 0.0;
                public override VISUnitType min() => new VISUnitType(0.0, unit: Unit.Percent);
                public override VISUnitType max() => new VISUnitType(1.0, unit: Unit.Percent);
                public override VISUnitType value() => new VISUnitType(dc_.oxygenLevel_, unit: Unit.Percent);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var airvent in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.min = new VISUnitType(0.0, unit: Unit.Percent);
                        item.max = new VISUnitType(1.0, unit: Unit.Percent);
                        item.value = new VISUnitType(airvent.GetOxygenLevel(), unit: Unit.Percent);
                        item.indicator = airvent.GetOxygenLevel();
                        item.name = airvent.CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
