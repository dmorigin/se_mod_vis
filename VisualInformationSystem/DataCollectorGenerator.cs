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
        public class DataCollectorGenerator : DataCollectorPowerProducer<IMyPowerProducer>
        {
            public DataCollectorGenerator(Configuration.Options options, string connector)
                : base("generator", "MyObjectBuilder_HydrogenEngine", options, connector)
            {
            }

            public override void prepareUpdate()
            {
                fuelCurrent_ = 0.0;
                fuelMax_ = 0.0;

                base.prepareUpdate();
            }

            protected override void update()
            {
                foreach (IMyPowerProducer pp in Blocks)
                {
                    blocksOn_ += isOn(pp) ? 1 : 0;
                    blocksFunctional_ += pp.IsFunctional ? 1 : 0;

                    currentOutput_ += pp.CurrentOutput;
                    maxAvailableOutput_ += pp.MaxOutput;

                    double current, max;
                    parseDetailedInfo(pp.DetailedInfo, out current, out max);
                    fuelCurrent_ += current;
                    fuelMax_ += max;
                }

                powerAvailableUsing_ = currentOutput_ / maxAvailableOutput_;
                UpdateFinished = true;
            }

            double fuelMax_ = 0.0;
            double fuelCurrent_ = 0.0;
            float fuelRatio_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxfuel%", new VISUnitType(fuelMax_, unit: Unit.Liter).pack())
                    .Replace("%currentfuel%", new VISUnitType(fuelCurrent_, unit: Unit.Liter).pack())
                    .Replace("%fuelratio%", new VISUnitType(fuelRatio_, unit: Unit.Percent));
            }

            static string pattern = @"^Filled: [0-9\.]+% \((?<cur>[0-9]+)L/(?<max>[0-9]+)L\)$";
            bool parseDetailedInfo(string info, out double current, out double max)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Multiline);
                System.Text.RegularExpressions.Match match = regex.Match(info);
                if (match.Success)
                {
                    double.TryParse(match.Groups["cur"].Value, out current);
                    double.TryParse(match.Groups["max"].Value, out max);
                    return true;
                }

                current = 0.0;
                max = 0.0;
                return false;
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                if (name.ToLower() == "fuel")
                    return new Fuel(this);
                return base.getDataAccessor(name);
            }

            class Fuel : DataAccessor
            {
                DataCollectorGenerator dc_ = null;
                public Fuel(DataCollectorGenerator dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.fuelRatio_;
                public override VISUnitType min() => new VISUnitType(0, unit: Unit.Liter);
                public override VISUnitType max() => new VISUnitType(dc_.fuelMax_, unit: Unit.Liter);
                public override VISUnitType value() => new VISUnitType(dc_.fuelCurrent_, unit: Unit.Liter);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyPowerProducer entry in dc_.Blocks)
                    {
                        double current, max;
                        dc_.parseDetailedInfo(entry.DetailedInfo, out current, out max);

                        ListContainer item = new ListContainer();
                        item.name = entry.CustomName;
                        item.indicator = max != 0 ? current / max : 0.0;
                        item.min = new VISUnitType(0.0, unit: Unit.Liter);
                        item.max = new VISUnitType(max, unit: Unit.Liter);
                        item.value = new VISUnitType(current, unit: Unit.Liter);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
