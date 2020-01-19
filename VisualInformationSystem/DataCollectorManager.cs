﻿using Sandbox.Game.EntityComponents;
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
                IDataCollector collector = dataCollectors_.Find(x => x.isSameCollector(name, options));
                if (collector != null)
                    return collector;

                return createDataCollector(name, options);
            }


            private IDataCollector createDataCollector(string name, Configuration.Options options)
            {
                IDataCollector dataCollector = null;
                switch (name)
                {
                    case "hydrogen":
                        dataCollector = new DataCollectorGasTank("MyObjectBuilder_HydrogenTank", options);
                        break;
                    case "oxygen":
                        dataCollector = new DataCollectorGasTank("MyObjectBuilder_OxygenTank", options);
                        break;
                    case "inventory":
                        dataCollector = new DataCollectorInventory(options);
                        break;
                    case "battery":
                        dataCollector = new DataCollectorBattery(options);
                        break;
                    case "solar":
                        dataCollector = new DataCollectorPowerProducer<IMySolarPanel>("solar", "", options);
                        break;
                    case "windturbine":
                        dataCollector = new DataCollectorPowerProducer<IMyPowerProducer>("windturbine", "MyObjectBuilder_WindTurbine", options);
                        break;
                    case "reactor":
                        dataCollector = new DataCollectorReactor(options);
                        break;
                    case "generator":
                        dataCollector = new DataCollectorGenerator(options);
                        break;
                    case "powerproducer":
                        dataCollector = new DataCollectorPowerProducer<IMyPowerProducer>("powerproducer", "", options);
                        break;
                    case "airvent":
                        dataCollector = new DataCollectorAirVent(options);
                        break;
                    case "jumpdrive":
                        dataCollector = new DataCollectorJumpDrive(options);
                        break;
                    case "landinggear":
                        dataCollector = new DataCollectorLandingGear(options);
                        break;
                    case "connector":
                        dataCollector = new DataCollectorConnector(options);
                        break;
                }

                if (dataCollector != null)
                {
                    Manager.JobManager.queueJob((dataCollector as VISObject).getConstructionJob());
                    return dataCollector;
                }
                else
                    log(Console.LogType.Error, $"Invalid data collector name {name}");

                return null;
            }
        }
    }
}
