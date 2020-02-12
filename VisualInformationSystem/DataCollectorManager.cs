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
                : base("DataCollectorManager")
            {
            }


            List<IDataCollector> dataCollectors_ = new List<IDataCollector>();

            public int Requested
            {
                get;
                protected set;
            }

            public int Created
            {
                get;
                protected set;
            }

            public IDataCollector getDataCollector(string name, Configuration.Options options, string connector = "")
            {
                Requested++;
                IDataCollector collector = dataCollectors_.Find(x => x.isSameCollector(name, options, connector));
                if (collector != null)
                    return collector;

                return createDataCollector(name, options, connector);
            }


            IDataCollector createDataCollector(string name, Configuration.Options options, string connector)
            {
                IDataCollector dataCollector = null;
                switch (name)
                {
                    case "hydrogen":
                        dataCollector = new DataCollectorGasTank("hydrogentank", "Hydrogen Tank", options, connector);
                        break;
                    case "oxygen":
                        dataCollector = new DataCollectorGasTank("oxygentank", "Oxygen Tank", options, connector);
                        break;
                    case "inventory":
                        dataCollector = new DataCollectorInventory(options, connector);
                        break;
                    case "battery":
                        dataCollector = new DataCollectorBattery(options, connector);
                        break;
                    case "solar":
                        dataCollector = new DataCollectorPowerProducer<IMySolarPanel>("solar", "", options, connector);
                        break;
                    case "windturbine":
                        dataCollector = new DataCollectorPowerProducer<IMyPowerProducer>("windturbine", "MyObjectBuilder_WindTurbine", options, connector);
                        break;
                    case "reactor":
                        dataCollector = new DataCollectorReactor(options, connector);
                        break;
                    case "generator":
                        dataCollector = new DataCollectorGenerator(options, connector);
                        break;
                    case "powerproducer":
                        dataCollector = new DataCollectorPowerProducer<IMyPowerProducer>("powerproducer", "", options, connector);
                        break;
                    case "airvent":
                        dataCollector = new DataCollectorAirVent(options, connector);
                        break;
                    case "jumpdrive":
                        dataCollector = new DataCollectorJumpDrive(options, connector);
                        break;
                    case "landinggear":
                        dataCollector = new DataCollectorLandingGear(options, connector);
                        break;
                    case "connector":
                        dataCollector = new DataCollectorConnector(options, connector);
                        break;
                    case "shipcontroller":
                        dataCollector = new DataCollectorShipController(options, connector);
                        break;
                    case "production":
                        dataCollector = new DataCollectorProduction<IMyProductionBlock>("production", "", Unit.None, options, connector);
                        break;
                    case "refinery":
                        dataCollector = new DataCollectorProduction<IMyRefinery>("refinery", "", Unit.Gram, options, connector);
                        break;
                    case "assembler":
                        dataCollector = new DataCollectorProduction<IMyAssembler>("assembler", "", Unit.None, options, connector);
                        break;
                    case "piston":
                        dataCollector = new DataCollectorPiston(options, connector);
                        break;
                    /*case "onoff":
                        dataCollector = new DataCollector<IMyTerminalBlock>("onoff", "", options);
                        break;*/
                }

                if (dataCollector != null)
                {
                    Created++;
                    Manager.JobManager.queueJob((dataCollector as VISObject).getConstructionJob());
                    dataCollectors_.Add(dataCollector);
                    return dataCollector;
                }
                else
                    log(Console.LogType.Error, $"Invalid data collector name {name}");

                return null;
            }
        }
    }
}
