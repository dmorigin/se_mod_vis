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
        public class DataCollectorEnergyProducer<T> : DataCollector<T> where T: class
        {
            public DataCollectorEnergyProducer(Configuration.Options options)
                : base("", options)
            {
            }

            public override bool construct()
            {
                if (!base.construct())
                    return false;

                maxOutput_ = 0f;
                foreach (IMyPowerProducer producer in Blocks)
                    maxOutput_ += producer.MaxOutput;

                update();
                Constructed = true;
                return true;
            }


            protected override void update()
            {
                float currentOutput = 0f;

                foreach (IMyPowerProducer pp in Blocks)
                    currentOutput += pp.CurrentOutput;

                currentOutput_ = currentOutput;
                powerUsing_ = currentOutput_ / maxOutput_;
            }


            public override string CollectorTypeName
            {
                get { return "energyproducer"; }
            }


            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%powerusing%", powerUsing_.ToString(Program.Default.StringFormat))
                    .Replace("%maxoutput%", (new ValueType(maxOutput_, Multiplier.M, Unit.W)).pack().ToString())
                    .Replace("%currentoutput%", (new ValueType(currentOutput_, Multiplier.M, Unit.W)).pack().ToString());
            }


            protected float maxOutput_ = 0f; // MW
            protected float currentOutput_ = 0f; // MW
            protected float powerUsing_ = 0f;


            public override DataRetriever getDataRetriever(string name)
            {
                switch (name.ToLower())
                {
                    case "":
                    case "using":
                        return new Using(this);
                }

                log(Console.LogType.Error, $"Invalid data retriever {name}");
                return null;
            }

            class Using : DataRetriever
            {
                public Using(DataCollectorEnergyProducer<T> collector)
                {
                    collector_ = collector;
                }

                DataCollectorEnergyProducer<T> collector_ = null;


                public override double getIndicator()
                {
                    return collector_.powerUsing_;
                }

                public override ValueType getValue()
                {
                    return new ValueType(collector_.currentOutput_, Multiplier.M, Unit.W);
                }

                public override ValueType getMin()
                {
                    return new ValueType(0, Multiplier.M, Unit.W);
                }

                public override ValueType getMax()
                {
                    return new ValueType(collector_.maxOutput_, Multiplier.M, Unit.W);
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();
                    foreach (IMyPowerProducer pp in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.onoff = DataCollector<T>.isOn(pp);
                        item.name = pp.CustomName;
                        item.indicator = pp.CurrentOutput / pp.MaxOutput;
                        item.value = new ValueType(pp.CurrentOutput, Multiplier.M, Unit.W);
                        item.min = new ValueType(0, Multiplier.M, Unit.W);
                        item.max = new ValueType(pp.MaxOutput, Multiplier.M, Unit.W);
                        container.Add(item);
                    }
                }
            }
        }
    }
}
