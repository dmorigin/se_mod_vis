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
            public DataCollectorReactor(Configuration.Options options)
                : base("reactor", "", options)
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
                foreach (IMyReactor reactor in Blocks)
                {
                    currentOutput_ += reactor.CurrentOutput;
                    maxAvailableOutput_ += reactor.MaxOutput;

                    IMyInventory inventory = reactor.GetInventory();
                    fuelCurrent_ += (double)inventory.CurrentVolume;
                    fuelMax_ += (double)inventory.MaxVolume;

                    inventory.GetItemAmount(new MyItemType("MyObjectBuilder_Ingot/", "Uranium"));
                }

                powerAvailableUsing_ = currentOutput_ / maxAvailableOutput_;
                fuelRatio_ = (float)(fuelCurrent_ / fuelMax_);
            }

            double fuelMax_ = 0.0;
            double fuelCurrent_ = 0.0;
            float fuelRatio_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxfuel%", new ValueType(fuelMax_, unit: Unit.l).pack().ToString())
                    .Replace("%currentfuel%", new ValueType(fuelCurrent_, unit: Unit.l).pack().ToString())
                    .Replace("%fuelratio%", new ValueType(fuelRatio_, unit: Unit.Percent).pack().ToString());
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
                public override ValueType min() => new ValueType(0, unit: Unit.l);
                public override ValueType max() => new ValueType(dc_.fuelMax_, unit: Unit.l);
                public override ValueType value() => new ValueType(dc_.fuelCurrent_, unit: Unit.l);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(IMyReactor entry in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        IMyInventory inventory = entry.GetInventory();
                        item.name = entry.CustomName;
                        item.indicator = (double)inventory.CurrentVolume / (double)inventory.MaxVolume;
                        item.min = new ValueType(0.0, unit: Unit.l);
                        item.max = new ValueType((double)inventory.MaxVolume, unit: Unit.l);
                        item.value = new ValueType((double)inventory.CurrentVolume, unit: Unit.l);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
