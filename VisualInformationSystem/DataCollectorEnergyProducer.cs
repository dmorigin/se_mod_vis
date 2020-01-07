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

                maxAvailableOutput_ = 0f;
                foreach (IMyPowerProducer pp in Blocks)
                    maxAvailableOutput_ += pp.MaxOutput;

                Constructed = true;
                return true;
            }


            protected override void update()
            {
                currentOutput_ = 0f;

                foreach (IMyPowerProducer pp in Blocks)
                    currentOutput_ += pp.CurrentOutput;

                powerAvailableUsing_ = currentOutput_ / maxAvailableOutput_;
            }


            public override string CollectorTypeName
            {
                get { return "energyproducer"; }
            }


            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%using%", powerAvailableUsing_.ToString(Program.Default.StringFormat))
                    .Replace("%maxoutput%", (new ValueType(maxAvailableOutput_, Multiplier.M, Unit.W)).pack().ToString())
                    .Replace("%currentoutput%", (new ValueType(currentOutput_, Multiplier.M, Unit.W)).pack().ToString());
            }


            protected float maxAvailableOutput_ = 0f; // MW
            protected float currentOutput_ = 0f; // MW
            protected float powerAvailableUsing_ = 0f;


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


                public override double indicator()
                {
                    return collector_.powerAvailableUsing_;
                }

                public override ValueType value()
                {
                    return new ValueType(collector_.currentOutput_, Multiplier.M, Unit.W);
                }

                public override ValueType min()
                {
                    return new ValueType(0, Multiplier.M, Unit.W);
                }

                public override ValueType max()
                {
                    return new ValueType(collector_.maxAvailableOutput_, Multiplier.M, Unit.W);
                }

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyPowerProducer pp in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = pp.CustomName;
                        item.indicator = pp.CurrentOutput / pp.MaxOutput;
                        item.value = new ValueType(pp.CurrentOutput, Multiplier.M, Unit.W);
                        item.min = new ValueType(0, Multiplier.M, Unit.W);
                        item.max = new ValueType(pp.MaxOutput, Multiplier.M, Unit.W);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            } // Using
        }
    }
}
