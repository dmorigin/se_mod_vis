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
        public class DataCollectorGasTank : DataCollector<IMyGasTank>
        {
            public DataCollectorGasTank(string name, string typeId, Configuration.Options options)
                : base(name, "MyObjectBuilder_OxygenTank", options)
            {
                subType_ = typeId;
            }

            static string typePattern = @"^Type: (?<type>Oxygen Tank|Hydrogen Tank)$";
            string subType_ = "";
            public override bool construct()
            {
                var regex = new System.Text.RegularExpressions.Regex(typePattern, System.Text.RegularExpressions.RegexOptions.Multiline);
                AcceptBlock = (block) =>
                {
                    var match = regex.Match(block.DetailedInfo);
                    if (match.Success && match.Groups["type"].Value == subType_)
                    {
                        capacity_ += block.Capacity;
                        Blocks.Add(block);
                    }
                };

                return base.construct();
            }

            protected override void update()
            {
                double fillRation = 0.0;

                foreach (var tank in Blocks)
                {
                    fillRation += tank.FilledRatio;
                    blocksOn_ += isOn(tank) ? 1 : 0;
                }

                fillRatio_ = Blocks.Count == 0 ? 0f : (float)(fillRation / Blocks.Count);
                UpdateFinished = true;
            }

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%capacity%", new VISUnitType(capacity_, unit: Unit.Liter).pack())
                    .Replace("%fillratio%", new VISUnitType(fillRatio_, unit: Unit.Percent).pack())
                    .Replace("%fillvalue", new VISUnitType(fillRatio_ * capacity_, unit: Unit.Liter).pack());
            }

            float fillRatio_ = 0f;
            float capacity_ = 0f;

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "capacity":
                    case "":
                        return new Capacity(this);
                }

                return base.getDataAccessor(name);
            }

            public class Capacity : DataAccessor
            {
                DataCollectorGasTank dc_ = null;
                public Capacity(DataCollectorGasTank collector)
                {
                    dc_ = collector;
                }

                public override double indicator() => dc_.fillRatio_;
                public override VISUnitType min() => new VISUnitType(0, unit: Unit.Liter);
                public override VISUnitType max() => new VISUnitType(dc_.capacity_, unit: Unit.Liter);
                public override VISUnitType value() => new VISUnitType(dc_.capacity_ * dc_.fillRatio_, unit: Unit.Liter);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var tank in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = tank.CustomName;
                        item.indicator = (float)tank.FilledRatio;
                        item.min = new VISUnitType(0, unit:Unit.Liter);
                        item.max = new VISUnitType(tank.Capacity, unit: Unit.Liter);
                        item.value = new VISUnitType(tank.FilledRatio * tank.Capacity, unit: Unit.Liter);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
