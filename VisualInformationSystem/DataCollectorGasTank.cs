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
                : base(typeId, options)
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
                    fillRation += tank.FilledRatio;

                fillRatio_ = (float)(fillRation / Blocks.Count);
            }

            public override string CollectorTypeName
            {
                get { return "gastank"; }
            }

            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "capacity":
                    case "":
                        return new Capacity(this);
                }

                log(Console.LogType.Error, $"Invalid data retriever {name}");
                return null;
            }

            public override string getText(string data)
            {
                return data.Replace("%capacity%", new ValueType(capacity_, unit: Unit.l).pack().ToString())
                    .Replace("%fillratio%", new ValueType(fillRatio_, unit: Unit.Percent).pack().ToString())
                    .Replace("%fillvalue", new ValueType(fillRatio_ * capacity_, unit: Unit.l).pack().ToString());
            }

            float fillRatio_ = 0f;
            float capacity_ = 0f;

            public class Capacity : DataAccessor
            {
                public Capacity(DataCollectorGasTank collector)
                {
                    collector_ = collector;
                }

                DataCollectorGasTank collector_ = null;

                public override ValueType min()
                {
                    return new ValueType(0, unit: Unit.l);
                }

                public override ValueType max()
                {
                    return new ValueType(collector_.capacity_, unit: Unit.l);
                }

                public override ValueType value()
                {
                    return new ValueType(collector_.capacity_ * collector_.fillRatio_, unit: Unit.l);
                }

                public override double indicator()
                {
                    return collector_.fillRatio_;
                }

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var tank in collector_.Blocks)
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
        }
    }
}
