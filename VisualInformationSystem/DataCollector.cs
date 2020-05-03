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
        public abstract class DataCollector : VISObject, IDataCollector
        {
            static int CollectorIDCounter = 0;
            public DataCollector(string collectorTypeName, string typeId, Configuration.Options options, string connector)
                : base($"DC:{collectorTypeName}/{typeId}:{CollectorIDCounter++}")
            {
                if (options == null)
                    Options = new Configuration.Options(new List<string>());
                else
                    Options = options;

                TypeID = typeId;
                CollectorTypeName = collectorTypeName;
                ReferenceGrid = App.Me;
                ConnectorName = connector;

                MaxUpdateInterval = Default.DCRefresh;
                ReconstructRetry = Default.ExceptionRetry;
                nextReconstruct_ = Manager.Timer.Ticks + Default.ReconstructInterval;

                if (connector != "")
                {
                    WatchConnector job = Manager.JobManager.getTimedJob(WatchConnector.JobName(ConnectorName)) as WatchConnector;
                    if (job != null)
                        job.add(this);
                    else
                        Manager.JobManager.registerTimedJob(new WatchConnector(ConnectorName, this));
                }

                UpdateFinished = false;
            }

            public virtual bool reconstruct()
            {
                Constructed = false;
                return true;
            }

            bool NeedReconstruct => (nextReconstruct_ <= Manager.Timer.Ticks) || ReconstructOnConnector;

            public virtual string getVariable(string data) => data;

            public virtual void prepareUpdate() { }
            public virtual void finalizeUpdate() { }
            protected virtual void update()
            {
                UpdateFinished = true;
            }

            #region Properties
            public string CollectorTypeName
            {
                get;
                protected set;
            }

            public Configuration.Options Options
            {
                get;
                private set;
            }

            public string TypeID
            {
                get;
                protected set;
            }

            TimeSpan nextUpdate_ = new TimeSpan(0);
            TimeSpan nextReconstruct_ = new TimeSpan(0);
            public TimeSpan MaxUpdateInterval
            {
                get;
                set;
            }

            public bool UpdateFinished
            {
                get;
                protected set;
            }

            protected bool ReconstructOnConnector
            {
                get;
                set;
            }

            protected IMyTerminalBlock ReferenceGrid
            {
                get;
                set;
            }

            protected string ConnectorName
            {
                get;
                private set;
            }

            int ReconstructRetry
            {
                get;
                set;
            }
            #endregion // Properties

            public virtual DataAccessor getDataAccessor(string name)
            {
                if (name != "")
                    log(Console.LogType.Error, $"Invalid data accessor {name}");
                return new Dummy();
            }

            class Dummy : DataAccessor
            {
                public override double indicator() => 0;
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(0);
                public override VISUnitType value() => new VISUnitType(0);
                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                }
            }

            #region Jobs
            public void queueJob()
            {
                if (NeedReconstruct)
                    Manager.JobManager.queueJob(new ReconstructJob(this));

                if (nextUpdate_ <= Manager.Timer.Ticks)
                    Manager.JobManager.queueJob(new UpdateJob(this));
            }

            class UpdateJob : Job
            {
                public UpdateJob(DataCollector dc)
                {
                    dc_ = dc;
                }

                DataCollector dc_;

                public override void prepareJob()
                {
                    dc_.prepareUpdate();
                    JobFinished = false;
                }

                public override void finalizeJob()
                {
                    dc_.finalizeUpdate();
                    dc_.nextUpdate_ = dc_.MaxUpdateInterval + Manager.Timer.Ticks;
                    dc_.ReconstructRetry = Default.ExceptionRetry;
                }

                public override void tick(TimeSpan delta)
                {
                    dc_.update();
                    JobFinished = dc_.UpdateFinished;
                }

                public override bool handleException()
                {
                    log(Console.LogType.Error, $"Update failed[{dc_.Name}]:Retry => {dc_.ReconstructRetry}");
                    if (dc_.ReconstructRetry-- > 0)
                    {
                        JobManager.queueJob(new UpdateJob(dc_), true);
                        JobManager.queueJob(new ReconstructJob(dc_), true);
                        return true;
                    }
                    return false;
                }
            }

            class ReconstructJob : Job
            {
                public ReconstructJob(DataCollector dc)
                {
                    dc_ = dc;
                }

                DataCollector dc_;

                public override void prepareJob()
                {
                    dc_.reconstruct();
                    JobFinished = false;
                }

                public override void finalizeJob()
                {
                    dc_.nextReconstruct_ = Manager.Timer.Ticks + Default.ReconstructInterval;
                    dc_.ReconstructOnConnector = false;
                }

                public override void tick(TimeSpan delta)
                {
                    if (dc_.construct() == false)
                    {
                        log(Console.LogType.Error, $"Reconstruction failed[{dc_.Name}]");
                        Manager.switchState(VISManager.State.Error);
                    }

                    JobFinished = dc_.Constructed;
                }
            }


            class WatchConnector : JobTimed
            {
                List<DataCollector> dcs_ = new List<DataCollector>();
                bool connected_ = false;
                string connectorName_ = "";
                IMyShipConnector connector_ = null;

                public WatchConnector(string connectorName, DataCollector dc)
                    : base(JobName(connectorName))
                {
                    add(dc);
                    connectorName_ = connectorName;
                    Interval = Default.WatchConnectorInterval;
                }

                public void add(DataCollector dc) => dcs_.Add(dc);
                public static string JobName(string name) => $"Job:WatchConnector:{name}";

                public override void tick(TimeSpan delta)
                {
                    if (connector_ == null)
                        connector_ = App.GridTerminalSystem.GetBlockWithName(connectorName_) as IMyShipConnector;

                    try
                    {
                        if (connector_ != null)
                        {
                            bool connected = connector_.Status == MyShipConnectorStatus.Connected;
                            var otherConnector = connected ? connector_.OtherConnector : null;
                            var reconstruct = connected_ != connected;

                            foreach (var dc in dcs_)
                            {
                                if (dc.Constructed)
                                {
                                    if (reconstruct)
                                        dc.ReconstructOnConnector = true;
                                    dc.ReferenceGrid = otherConnector;
                                }
                            }

                            connected_ = connected;
                        }
                        else
                        {
                            foreach (var dc in dcs_)
                                dc.ReferenceGrid = null;
                        }
                    }
                    catch (Exception)
                    {
                        foreach (var dc in dcs_)
                            dc.ReferenceGrid = null;
                    }
                }
            }
            #endregion // Jobs

            public virtual bool isSameCollector(string name, Configuration.Options options, string connector) => 
                CollectorTypeName == name && Options.equals(options) && ConnectorName == connector;

            public static double clamp(double value, double min = 0.0, double max = 1.0) => value < min ? min : (value > max ? max : value);
            public static float clamp(float value, float min = 0f, float max = 1f) => value < min ? min : (value > max ? max : value);
            public static long clamp(long value, long min = 0, long max = 1) => value < min ? min : (value > max ? max : value);
            //public static int clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        }
    }
}
