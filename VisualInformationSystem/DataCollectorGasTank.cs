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

                update();
                Constructed = true;
                return true;
            }

            protected override void update()
            {
                if (Blocks.Count == 0)
                    return;

                double fillRation = 0.0;
                capacity_ = 0f;

                foreach (var tank in Blocks)
                    fillRation += tank.FilledRatio;

                fillRation_ = (float)(fillRation / Blocks.Count);
            }

            public override string CollectorTypeName
            {
                get { return "gastank"; }
            }

            public override DataRetriever getDataRetriever(string name)
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
                return data.Replace("%capacity%", capacity_.ToString())
                    .Replace("%fillration%", fillRation_.ToString())
                    .Replace("%fillvalue", (fillRation_ * capacity_).ToString());
            }

            float fillRation_ = 0f;
            float capacity_ = 0f;

            public class Capacity : DataRetriever
            {
                public Capacity(DataCollectorGasTank collector)
                {
                    collector_ = collector;
                }

                DataCollectorGasTank collector_ = null;

                public override double getMin()
                {
                    return 0;
                }

                public override double getMax()
                {
                    return collector_.capacity_;
                }

                public override double getValue()
                {
                    return collector_.capacity_ * collector_.fillRation_;
                }

                public override double getIndicator()
                {
                    return collector_.fillRation_;
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();
                    foreach (var tank in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = tank.CustomName;
                        item.indicator = (float)tank.FilledRatio;
                        item.min = 0;
                        item.max = tank.Capacity;
                        item.value = (float)(tank.FilledRatio * tank.Capacity);
                        container.Add(item);
                    }
                }
            }
        }
    }
}
