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
            public DataCollectorPowerProducer(string collectorTypeName, string typeId, Configuration.Options options, string connector)
                : base(collectorTypeName, typeId, options, connector)
            {
            }

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                currentOutput_ = 0f;
                maxAvailableOutput_ = 0f;
            }

            protected override void update()
            {
                foreach (IMyPowerProducer pp in Blocks)
                {
                    currentOutput_ += pp.CurrentOutput;
                    maxAvailableOutput_ += pp.MaxOutput;
                    blocksOn_ += isOn(pp) ? 1 : 0;
                    blocksFunctional_ += pp.IsFunctional ? 1 : 0;
                }

                powerAvailableUsing_ = maxAvailableOutput_ != 0f ? currentOutput_ / maxAvailableOutput_ : 0f;
                UpdateFinished = true;
            }

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%usage%", new VISUnitType(powerAvailableUsing_, unit:Unit.Percent))
                    .Replace("%maxoutput%", new VISUnitType(maxAvailableOutput_, Multiplier.M, Unit.Watt).pack())
                    .Replace("%currentoutput%", new VISUnitType(currentOutput_, Multiplier.M, Unit.Watt).pack());
            }

            protected float maxAvailableOutput_ = 0f; // MW
            protected float currentOutput_ = 0f; // MW
            protected float powerAvailableUsing_ = 0f;

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "usage":
                        return new Usage(this);
                }

                return base.getDataAccessor(name);
            }

            class Usage : DataAccessor
            {
                DataCollectorPowerProducer<T> collector_ = null;
                public Usage(DataCollectorPowerProducer<T> collector)
                {
                    collector_ = collector;
                }

                public override double indicator() => collector_.powerAvailableUsing_;
                public override VISUnitType value() => new VISUnitType(collector_.currentOutput_, Multiplier.M, Unit.Watt);
                public override VISUnitType min() => new VISUnitType(0, Multiplier.M, Unit.Watt);
                public override VISUnitType max() => new VISUnitType(collector_.maxAvailableOutput_, Multiplier.M, Unit.Watt);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyPowerProducer pp in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = pp.CustomName;
                        item.indicator = pp.MaxOutput != 0.0 ? pp.CurrentOutput / pp.MaxOutput : 0.0;
                        item.value = new VISUnitType(pp.CurrentOutput, Multiplier.M, Unit.Watt);
                        item.min = new VISUnitType(0, Multiplier.M, Unit.Watt);
                        item.max = new VISUnitType(pp.MaxOutput, Multiplier.M, Unit.Watt);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            } // Using
            #endregion // Data Accessor
        }
    }
}
