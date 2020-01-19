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
        public class DataCollectorJumpDrive: DataCollector<IMyJumpDrive>
        {
            public DataCollectorJumpDrive(Configuration.Options options)
                : base("jumpdrive", "", options)
            {
            }

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                maxStoredPower_ = 0f;
                currentStoredPower_ = 0f;
            }

            protected override void update()
            {
                foreach (IMyJumpDrive jd in Blocks)
                {
                    currentStoredPower_ += jd.CurrentStoredPower;
                    maxStoredPower_ += jd.MaxStoredPower;
                    amountRecharging_ += jd.Status == MyJumpDriveStatus.Charging ? 1 : 0;
                    amountJumping_ += jd.Status == MyJumpDriveStatus.Jumping ? 1 : 0;
                    amountReady_ += jd.Status == MyJumpDriveStatus.Ready ? 1 : 0;

                    blocksOn_ += isOn(jd) ? 1 : 0;
                }

                ratioStoredPower_ = maxStoredPower_ != 0f ? currentStoredPower_ / maxStoredPower_ : 0f;
                UpdateFinished = true;
            }

            float maxStoredPower_ = 0f;
            float currentStoredPower_ = 0f;
            float ratioStoredPower_ = 0f;
            int amountRecharging_ = 0;
            int amountJumping_ = 0;
            int amountReady_ = 0;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxcapacity%", new ValueType(maxStoredPower_, Multiplier.M, Unit.Wh).pack().ToString())
                    .Replace("%currentcapacity%", new ValueType(currentStoredPower_, Multiplier.M, Unit.Wh).pack().ToString())
                    .Replace("%capacityratio%", new ValueType(ratioStoredPower_, unit: Unit.Percent).pack().ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                return base.getDataAccessor(name);
            }

            class Capacity : DataAccessor
            {
                DataCollectorJumpDrive dc_;
                public Capacity(DataCollectorJumpDrive dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.ratioStoredPower_;
                public override ValueType min() => new ValueType(0, Multiplier.M, Unit.Wh);
                public override ValueType max() => new ValueType(dc_.maxStoredPower_, Multiplier.M, Unit.Wh);
                public override ValueType value() => new ValueType(dc_.currentStoredPower_, Multiplier.M, Unit.Wh);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var jd in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = jd.CustomName;
                        item.indicator = jd.CurrentStoredPower / jd.MaxStoredPower;
                        item.min = new ValueType(0, Multiplier.M, Unit.Wh);
                        item.max = new ValueType(jd.MaxStoredPower, Multiplier.M, Unit.Wh);
                        item.value = new ValueType(jd.CurrentStoredPower, Multiplier.M, Unit.Wh);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Ready : DataAccessor
            {
                DataCollectorJumpDrive dc_;
                public Ready(DataCollectorJumpDrive dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => (double)dc_.amountReady_ / dc_.Blocks.Count;
                public override ValueType min() => new ValueType(0);
                public override ValueType max() => new ValueType(dc_.Blocks.Count);
                public override ValueType value() => new ValueType(dc_.amountReady_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var jd in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = jd.CustomName;
                        item.indicator = jd.CurrentStoredPower / jd.MaxStoredPower;
                        item.min = new ValueType(0, Multiplier.M, Unit.Wh);
                        item.max = new ValueType(jd.MaxStoredPower, Multiplier.M, Unit.Wh);
                        item.value = new ValueType(jd.CurrentStoredPower, Multiplier.M, Unit.Wh);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
