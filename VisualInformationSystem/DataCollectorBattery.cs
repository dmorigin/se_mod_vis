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
        public class DataCollectorBattery : DataCollectorPowerProducer<IMyBatteryBlock>
        {
            public DataCollectorBattery(Configuration.Options options)
                : base("battery", "", options)
            {
            }

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                currentInput_ = 0f;
                currentOutput_ = 0f;
                currentStored_ = 0f;

                maxAvailableOutput_ = 0f;
                maxAvailableInput_ = 0f;
                maxAvailableStored_ = 0f;
            }

            protected override void update()
            {
                foreach (var battery in Blocks)
                {
                    currentInput_ += battery.CurrentInput;
                    currentOutput_ += battery.CurrentOutput;
                    currentStored_ += battery.CurrentStoredPower;

                    maxAvailableOutput_ += battery.MaxOutput;
                    maxAvailableInput_ += battery.MaxInput;
                    maxAvailableStored_ += battery.MaxStoredPower;
                }

                powerAvailableUsing_ = currentOutput_ / maxAvailableOutput_;
                powerAvailableStoring_ = currentInput_ / maxAvailableInput_;
                powerAvailableLeft_ = currentStored_ / maxAvailableStored_;
            }

            public List<IMyBatteryBlock> Batteries
            {
                get { return Blocks; }
            }

            float maxAvailableInput_ = 0f; // MW
            float maxAvailableStored_ = 0f; // MWh
            float currentInput_ = 0f; // MW
            float currentStored_ = 0f; //MWh

            float powerAvailableLeft_ = 0f;
            float powerAvailableStoring_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%powerleft%", new ValueType(powerAvailableLeft_, unit: Unit.Percent).pack().ToString())
                    .Replace("%powerstoring%", new ValueType(powerAvailableStoring_, unit: Unit.Percent).pack().ToString())
                    .Replace("%maxinput%", new ValueType(maxAvailableInput_, Multiplier.M, Unit.W).pack().ToString())
                    .Replace("%maxcapacity%", new ValueType(maxAvailableStored_, Multiplier.M, Unit.Wh).pack().ToString())
                    .Replace("%currentinput%", new ValueType(currentInput_, Multiplier.M, Unit.W).pack().ToString())
                    .Replace("%currentcapacity%", new ValueType(currentStored_, Multiplier.M, Unit.Wh).pack().ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "":
                    case "capacity":
                        return new Capacity(this);
                    case "inout":
                        return new InOut(this);
                }

                return base.getDataAccessor(name);
            }

            class Capacity : DataAccessor
            {
                DataCollectorBattery dc_ = null;
                public Capacity(DataCollectorBattery collector)
                {
                    dc_ = collector;
                }

                public override double indicator() => dc_.powerAvailableLeft_;
                public override ValueType value() => new ValueType(dc_.currentStored_, Multiplier.M, Unit.Wh);
                public override ValueType min() => new ValueType(0, Multiplier.M, Unit.Wh);
                public override ValueType max() => new ValueType(dc_.maxAvailableStored_, Multiplier.M, Unit.Wh);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var ep in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = ep.CustomName;
                        item.indicator = ep.CurrentStoredPower / ep.MaxStoredPower;
                        item.value = new ValueType(ep.CurrentStoredPower, Multiplier.M, Unit.Wh);
                        item.min = new ValueType(0, Multiplier.M, Unit.Wh);
                        item.max = new ValueType(ep.MaxStoredPower, Multiplier.M, Unit.Wh);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class InOut : DataAccessor
            {
                DataCollectorBattery dc_ = null;
                public InOut(DataCollectorBattery collector)
                {
                    dc_ = collector;
                }

                public override double indicator() => dc_.powerAvailableStoring_ - dc_.powerAvailableUsing_;
                public override ValueType value() => new ValueType(dc_.currentInput_ - dc_.currentOutput_, Multiplier.M, Unit.W);
                public override ValueType min() => new ValueType(-dc_.maxAvailableOutput_, Multiplier.M, Unit.W);
                public override ValueType max() => new ValueType(dc_.maxAvailableInput_, Multiplier.M, Unit.W);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var battery in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = battery.CustomName;
                        item.indicator = (battery.CurrentInput / battery.MaxInput) - (battery.CurrentOutput / battery.MaxOutput);
                        item.value = new ValueType(battery.CurrentInput - battery.CurrentOutput, Multiplier.M, Unit.W);
                        item.min = new ValueType(battery.MaxOutput, Multiplier.M, Unit.W);
                        item.max = new ValueType(battery.MaxInput, Multiplier.M, Unit.W);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
