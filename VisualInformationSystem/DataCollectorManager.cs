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
        public class DataCollectorManager : VISObject
        {
            public DataCollectorManager()
            {
            }


            List<IDataCollector> dataCollectors_ = new List<IDataCollector>();


            public IDataCollector getDataCollector(string name, Configuration.Options options)
            {
                foreach (IDataCollector collector in dataCollectors_)
                {
                    if (collector.isSameCollector(name, options) == true)
                        return collector;
                }

                return createDataCollector(name, options);
            }


            private IDataCollector createDataCollector(string name, Configuration.Options options)
            {
                IDataCollector dataCollector = null;
                bool constructed = false;

                switch (name)
                {
                    case "hydrogen":
                        DataCollectorGasTank hydrogenTanks = new DataCollectorGasTank("MyObjectBuilder_HydrogenTank", options);
                        constructed = hydrogenTanks.construct();
                        dataCollector = hydrogenTanks;
                        break;
                    case "oxygen":
                        DataCollectorGasTank oxygenTanks = new DataCollectorGasTank("MyObjectBuilder_OxygenTank", options);
                        constructed = oxygenTanks.construct();
                        dataCollector = oxygenTanks;
                        break;
                    case "inventory":
                        DataCollectorInventory inventory = new DataCollectorInventory(options);
                        constructed = inventory.construct();
                        dataCollector = inventory;
                        break;
                    case "battery":
                        DataCollectorBattery battery = new DataCollectorBattery(options);
                        constructed = battery.construct();
                        dataCollector = battery;
                        break;
                    case "solar":
                        DataCollectorEnergyProducer<IMySolarPanel> solar = new DataCollectorEnergyProducer<IMySolarPanel>(options);
                        constructed = solar.construct();
                        dataCollector = solar;
                        break;
                    case "reactor":
                        DataCollectorEnergyProducer<IMyReactor> reactor = new DataCollectorEnergyProducer<IMyReactor>(options);
                        constructed = reactor.construct();
                        dataCollector = reactor;
                        break;
                    case "generator":
                        DataCollectorEnergyProducer<IMyGasGenerator> generator = new DataCollectorEnergyProducer<IMyGasGenerator>(options);
                        constructed = generator.construct();
                        dataCollector = generator;
                        break;
                    case "energyproducer":
                        DataCollectorEnergyProducer<IMyPowerProducer> energyproducer = new DataCollectorEnergyProducer<IMyPowerProducer>(options);
                        constructed = energyproducer.construct();
                        dataCollector = energyproducer;
                        break;
                }

                if (dataCollector != null)
                {
                    if (constructed)
                        return dataCollector;
                    else
                        log(Console.LogType.Error, $"Cannot construct data collector {name}");
                }
                else
                    log(Console.LogType.Error, $"Invalid data collector name {name}");

                return null;
            }
        }
    }
}
