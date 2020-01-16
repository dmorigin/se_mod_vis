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
            public DataCollectorGasTank(string typeId, Configuration.Options options)
                : base("gastank", typeId, options)
            {
            }

            public override bool construct()
            {
                if (!base.construct())
                    return false;

                foreach (var tank in Blocks)
                    capacity_ += tank.Capacity;

                Constructed = true;
                return true;
            }

            protected override void update()
            {
                if (Blocks.Count == 0)
                    return;

                double fillRation = 0.0;

                foreach (var tank in Blocks)
                {
                    fillRation += tank.FilledRatio;
                    blocksOn_ += isOn(tank) ? 1 : 0;
                }

                fillRatio_ = (float)(fillRation / Blocks.Count);
                UpdateFinished = true;
            }

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%capacity%", new ValueType(capacity_, unit: Unit.l).pack().ToString())
                    .Replace("%fillratio%", new ValueType(fillRatio_, unit: Unit.Percent).pack().ToString())
                    .Replace("%fillvalue", new ValueType(fillRatio_ * capacity_, unit: Unit.l).pack().ToString());
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
                public override ValueType min() => new ValueType(0, unit: Unit.l);
                public override ValueType max() => new ValueType(dc_.capacity_, unit: Unit.l);
                public override ValueType value() => new ValueType(dc_.capacity_ * dc_.fillRatio_, unit: Unit.l);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var tank in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = tank.CustomName;
                        item.indicator = (float)tank.FilledRatio;
                        item.min = new ValueType(0, unit:Unit.l);
                        item.max = new ValueType(tank.Capacity, unit: Unit.l);
                        item.value = new ValueType(tank.FilledRatio * tank.Capacity, unit: Unit.l);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
