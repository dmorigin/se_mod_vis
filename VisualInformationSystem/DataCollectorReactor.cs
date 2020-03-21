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
        public class DataCollectorReactor : DataCollectorPowerProducer<IMyReactor>
        {
            public DataCollectorReactor(Configuration.Options options, string connector)
                : base("reactor", "", options, connector)
            {
            }

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                fuelCurrent_ = 0.0;
                fuelMax_ = 0.0;
            }

            protected override void update()
            {
                foreach (IMyReactor reactor in Blocks)
                {
                    currentOutput_ += reactor.CurrentOutput;
                    maxAvailableOutput_ += reactor.MaxOutput;

                    IMyInventory inventory = reactor.GetInventory();
                    fuelCurrent_ += (double)inventory.CurrentVolume;
                    fuelMax_ += (double)inventory.MaxVolume;

                    blocksOn_ += isOn(reactor) ? 1 : 0;
                    blocksFunctional_ += reactor.IsFunctional ? 1 : 0;
                }

                powerAvailableUsing_ = maxAvailableOutput_ != 0f ? currentOutput_ / maxAvailableOutput_ : 0f;
                fuelRatio_ = (float)(fuelCurrent_ / fuelMax_);
                UpdateFinished = true;
            }

            double fuelMax_ = 0.0;
            double fuelCurrent_ = 0.0;
            float fuelRatio_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxfuel%", new VISUnitType(fuelMax_, Multiplier.K, Unit.Liter).pack())
                    .Replace("%currentfuel%", new VISUnitType(fuelCurrent_, Multiplier.K, Unit.Liter).pack())
                    .Replace("%fuelratio%", new VISUnitType(fuelRatio_, unit: Unit.Percent));
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
                DataCollectorReactor dc_ = null;
                public Fuel(DataCollectorReactor dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.fuelRatio_;
                public override VISUnitType min() => new VISUnitType(0, unit: Unit.Liter);
                public override VISUnitType max() => new VISUnitType(dc_.fuelMax_, Multiplier.K, Unit.Liter);
                public override VISUnitType value() => new VISUnitType(dc_.fuelCurrent_, Multiplier.K, Unit.Liter);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(IMyReactor entry in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        IMyInventory inventory = entry.GetInventory();
                        item.name = entry.CustomName;
                        item.indicator = (double)inventory.CurrentVolume / (double)inventory.MaxVolume;
                        item.min = new VISUnitType(0.0, unit: Unit.Liter);
                        item.max = new VISUnitType((double)inventory.MaxVolume, Multiplier.K, Unit.Liter);
                        item.value = new VISUnitType((double)inventory.CurrentVolume, Multiplier.K, Unit.Liter);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
