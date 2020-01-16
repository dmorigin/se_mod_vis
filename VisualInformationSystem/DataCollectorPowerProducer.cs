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
        public class DataCollectorPowerProducer<T> : DataCollector<T> where T: class
        {
            public DataCollectorPowerProducer(string collectorTypeName, string typeId, Configuration.Options options)
                : base(collectorTypeName, typeId, options)
            {
            }

            public override void prepareUpdate()
            {
                currentOutput_ = 0f;
                maxAvailableOutput_ = 0f;

                base.prepareUpdate();
            }

            protected override void update()
            {
                foreach (IMyPowerProducer pp in Blocks)
                {
                    currentOutput_ += pp.CurrentOutput;
                    maxAvailableOutput_ += pp.MaxOutput;
                }

                powerAvailableUsing_ = currentOutput_ / maxAvailableOutput_;
            }

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%usage%", new ValueType(powerAvailableUsing_, unit:Unit.Percent).pack().ToString())
                    .Replace("%maxoutput%", new ValueType(maxAvailableOutput_, Multiplier.M, Unit.W).pack().ToString())
                    .Replace("%currentoutput%", new ValueType(currentOutput_, Multiplier.M, Unit.W).pack().ToString());
            }


            protected float maxAvailableOutput_ = 0f; // MW
            protected float currentOutput_ = 0f; // MW
            protected float powerAvailableUsing_ = 0f;


            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "":
                    case "usage":
                        return new Usage(this);
                }

                log(Console.LogType.Error, $"Invalid data retriever {name}");
                return null;
            }

            class Usage : DataAccessor
            {
                DataCollectorPowerProducer<T> collector_ = null;
                public Usage(DataCollectorPowerProducer<T> collector)
                {
                    collector_ = collector;
                }

                public override double indicator() => collector_.powerAvailableUsing_;
                public override ValueType value() => new ValueType(collector_.currentOutput_, Multiplier.M, Unit.W);
                public override ValueType min() => new ValueType(0, Multiplier.M, Unit.W);
                public override ValueType max() => new ValueType(collector_.maxAvailableOutput_, Multiplier.M, Unit.W);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyPowerProducer pp in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = pp.CustomName;
                        item.indicator = pp.MaxOutput != 0.0 ? pp.CurrentOutput / pp.MaxOutput : 0.0;
                        item.value = new ValueType(pp.CurrentOutput, Multiplier.M, Unit.W);
                        item.min = new ValueType(0, Multiplier.M, Unit.W);
                        item.max = new ValueType(pp.MaxOutput, Multiplier.M, Unit.W);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            } // Using
            #endregion // Data Accessor
        }
    }
}
