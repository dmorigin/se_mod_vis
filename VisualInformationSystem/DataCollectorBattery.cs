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
        public class DataCollectorBattery : DataCollectorEnergyProducer<IMyBatteryBlock>
        {
            public DataCollectorBattery(Configuration.Options options)
                : base(options)
            {
            }

            public override bool construct()
            {
                if (!base.construct())
                    return false;

                maxOutput_ = 0f;
                maxInput_ = 0f;
                maxStored_ = 0f;
                foreach(var battery in Blocks)
                {
                    maxOutput_ += battery.MaxOutput;
                    maxInput_ += battery.MaxInput;
                    maxStored_ += battery.MaxStoredPower;
                }

                update();
                Constructed = true;
                return true;
            }


            protected override void update()
            {
                float currentInput = 0f;
                float currentOutput = 0f;
                float currentStored = 0f;

                foreach (var battery in Blocks)
                {
                    currentInput += battery.CurrentInput;
                    currentOutput += battery.CurrentOutput;
                    currentStored += battery.CurrentStoredPower;
                }

                currentInput_ = currentInput;
                currentOutput_ = currentOutput;
                currentStored_ = currentStored;

                powerUsing_ = currentOutput_ / maxOutput_;
                powerStoring_ = currentInput_ / maxInput_;
                powerLeft_ = currentStored_ / maxStored_;
            }


            public override string CollectorTypeName
            {
                get { return "battery"; }
            }


            float maxInput_ = 0f; // MW
            float maxStored_ = 0f; // MWh
            float currentInput_ = 0f; // MW
            float currentStored_ = 0f; //MWh

            float powerLeft_ = 0f;
            float powerStoring_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%powerleft%", powerLeft_.ToString(Program.Default.StringFormat))
                    .Replace("%powerstoring%", powerStoring_.ToString(Program.Default.StringFormat))
                    .Replace("%maxinput%", (new ValueType(maxInput_, Multiplier.M, Unit.W)).pack().ToString())
                    .Replace("%maxstored%", (new ValueType(maxStored_, Multiplier.M, Unit.Wh)).pack().ToString())
                    .Replace("%currentinput%", (new ValueType(currentInput_, Multiplier.M, Unit.W)).pack().ToString())
                    .Replace("%currentstored%", (new ValueType(currentStored_, Multiplier.M, Unit.Wh)).pack().ToString());
            }

            public override DataRetriever getDataRetriever(string name)
            {
                switch(name.ToLower())
                {
                    case "capacity":
                        return new Capacity(this);
                    case "inout":
                        return new InOut(this);
                }

                return base.getDataRetriever(name);
            }


            class Capacity : DataRetriever
            {
                public Capacity(DataCollectorBattery collector)
                {
                    collector_ = collector;
                }

                DataCollectorBattery collector_ = null;


                public override double getIndicator()
                {
                    return collector_.powerLeft_;
                }

                public override ValueType getValue()
                {
                    return new ValueType(collector_.currentStored_, Multiplier.M, Unit.Wh);
                }

                public override ValueType getMin()
                {
                    return new ValueType(0, Multiplier.M, Unit.Wh);
                }

                public override ValueType getMax()
                {
                    return new ValueType(collector_.maxStored_, Multiplier.M, Unit.Wh);
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();
                    foreach (var ep in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.onoff = DataCollector<IMyBatteryBlock>.isOn(ep);
                        item.name = ep.CustomName;
                        item.indicator = ep.CurrentStoredPower / ep.MaxStoredPower;
                        item.value = new ValueType(ep.CurrentStoredPower, Multiplier.M, Unit.Wh);
                        item.min = new ValueType(0, Multiplier.M, Unit.Wh);
                        item.max = new ValueType(ep.MaxStoredPower, Multiplier.M, Unit.Wh);
                        container.Add(item);
                    }
                }
            }

            class InOut : DataRetriever
            {
                public InOut(DataCollectorBattery collector)
                {
                    collector_ = collector;
                }

                DataCollectorBattery collector_ = null;


                public override double getIndicator()
                {
                    return collector_.powerStoring_ - collector_.powerUsing_;
                }

                public override ValueType getValue()
                {
                    return new ValueType(collector_.currentInput_ - collector_.currentOutput_, Multiplier.M, Unit.W);
                }

                public override ValueType getMin()
                {
                    return new ValueType(-collector_.maxOutput_, Multiplier.M, Unit.W);
                }

                public override ValueType getMax()
                {
                    return new ValueType(collector_.maxInput_, Multiplier.M, Unit.W);
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();
                    foreach (var battery in collector_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.onoff = DataCollector<IMyBatteryBlock>.isOn(battery);
                        item.name = battery.CustomName;
                        item.indicator = (battery.CurrentInput / battery.MaxInput) - (battery.CurrentOutput / battery.MaxOutput);
                        item.value = new ValueType(battery.CurrentInput - battery.CurrentOutput, Multiplier.M, Unit.W);
                        item.min = new ValueType(battery.MaxOutput, Multiplier.M, Unit.W);
                        item.max = new ValueType(battery.MaxInput, Multiplier.M, Unit.W);
                        container.Add(item);
                    }
                }
            }
        }
    }
}
