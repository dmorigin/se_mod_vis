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
        public abstract class DataCollector<T> : VISObject, IDataCollector where T : class
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
                Connector = null;
                ConnectorName = connector;
                Blocks = new List<T>();
                AcceptBlock = (block) => Blocks.Add(block);

                MaxUpdateInterval = Default.DCRefresh;
                ReconstructRetry = Default.ExceptionRetry;
                nextReconstruct_ = Manager.Timer.Ticks + Default.ReconstructInterval;

                if (connector == "")
                {
                    BlockName = Options[0];
                    IsGroup = Options.asBoolean(1, false);
                }
                else
                {
                    BlockName = Options[1];
                    IsGroup = Options.asBoolean(2, false);
                    Manager.JobManager.registerTimedJob(new WatchConnector(this));
                }

                UpdateFinished = false;
            }

            #region Construction
            public override bool construct()
            {
                if (ConnectorName != "" && Connector == null)
                    log(Console.LogType.Error, $"Connector \"{ConnectorName}\" not found");

                if (ReferenceGrid != null)
                {
                    if (BlockName != "")
                        getBlocks<T>(BlockName, IsGroup, AcceptBlock, TypeID);
                    else if (!getBlocks<T>(AcceptBlock, TypeID))
                        return false;
                }

                if (Blocks.Count == 0 && ConnectorName == "")
                    log(Console.LogType.Warning, $"No blocks found {Name}[{BlockName}{(IsGroup ? ":group" : "")}]");
                Constructed = true;
                return true;
            }

            public virtual bool reconstruct()
            {
                Blocks.Clear();
                Constructed = false;
                return true;
            }

            bool NeedReconstruct => (nextReconstruct_ <= Manager.Timer.Ticks) || ReconstructOnConnector;

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

            protected IMyShipConnector Connector
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
            #endregion // Construction

            public virtual string getText(string data)
            {
                return data
                    .Replace("%blockcount%", Blocks.Count.ToString())
                    .Replace("%blockname%", BlockName)
                    .Replace("%isgroup%", IsGroup ? "true" : "false")
                    .Replace("%on%", blocksOn_.ToString())
                    .Replace("%off%", blocksOff_.ToString())
                    .Replace("%onratio%", new VISUnitType(onRatio_, unit: Unit.Percent))
                    .Replace("%offratio%", new VISUnitType(offRatio_, unit: Unit.Percent))
                    .Replace("%functional%", blocksFunctional_.ToString())
                    .Replace("%functionalratio%", new VISUnitType(blocksFunctionalRatio_, unit: Unit.Percent));
            }

            #region Update System
            public bool UpdateFinished
            {
                get;
                protected set;
            }

            public virtual void prepareUpdate()
            {
                blocksOn_ = 0;
                blocksFunctional_ = 0;
            }

            public virtual void finalizeUpdate()
            {
                if (Blocks.Count > 0)
                {
                    blocksOff_ = Blocks.Count - blocksOn_;
                    onRatio_ = (float)blocksOn_ / (float)Blocks.Count;
                    offRatio_ = (float)blocksOff_ / (float)Blocks.Count;

                    blocksFunctionalRatio_ = (float)blocksFunctional_ / (float)Blocks.Count;
                }
                else
                {
                    blocksOff_ = 0;
                    onRatio_ = 0f;
                    offRatio_ = 0f;
                    blocksFunctionalRatio_ = 0f;
                }
            }

            protected virtual void update()
            {
                foreach (IMyTerminalBlock block in Blocks)
                {
                    blocksOn_ += isOn(block) ? 1 : 0;
                    blocksFunctional_ += block.IsFunctional ? 1 : 0;
                }

                UpdateFinished = true;
            }
            #endregion // Update System

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

            protected List<T> Blocks
            {
                get;
                private set;
            }

            public bool IsGroup
            {
                get;
                protected set;
            }

            public string BlockName
            {
                get;
                protected set;
            }

            protected GetBlockDelegate<T> AcceptBlock
            {
                get;
                set;
            }

            TimeSpan nextUpdate_ = new TimeSpan(0);
            TimeSpan nextReconstruct_ = new TimeSpan(0);
            public TimeSpan MaxUpdateInterval
            {
                get;
                set;
            }
            #endregion // Properties

            #region Data Accessor
            protected int blocksOn_ = 0;
            protected int blocksOff_ = 0;
            protected float onRatio_ = 0f;
            protected float offRatio_ = 0f;

            protected int blocksFunctional_ = 0;
            protected float blocksFunctionalRatio_ = 0f;

            public virtual DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "on":
                        return new DAOn(this);
                    case "off":
                        return new DAOff(this);
                    case "functional":
                        return new DAFunctional(this);
                }

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

            class DAFunctional : DataAccessor
            {
                DataCollector<T> dc_;
                public DAFunctional(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.blocksFunctionalRatio_;
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.blocksFunctional_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = block.IsFunctional ? 1.0 : 0.0;
                        item.value = new VISUnitType(item.indicator);
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class DAOn : DataAccessor
            {
                DataCollector<T> dc_;
                public DAOn(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.onRatio_;
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.blocksOn_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = isOn(block) ? 1.0 : 0.0;
                        item.value = new VISUnitType(item.indicator);
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class DAOff : DataAccessor
            {
                DataCollector<T> dc_;
                public DAOff(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.offRatio_;
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.blocksOff_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = isOn(block) ? 0.0 : 1.0;
                        item.value = new VISUnitType(item.indicator);
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor

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
                public UpdateJob(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                DataCollector<T> dc_;

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
                public ReconstructJob(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                DataCollector<T> dc_;

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
                DataCollector<T> dc_;
                bool connected_ = false;

                public WatchConnector(DataCollector<T> dc)
                {
                    dc_ = dc;
                    Interval = Default.WatchConnectorInterval;
                }

                public override void tick(TimeSpan delta)
                {
                    dc_.Connector = App.GridTerminalSystem.GetBlockWithName(dc_.ConnectorName) as IMyShipConnector;
                    if (dc_.Connector != null && dc_.Constructed)
                    {
                        bool connected = dc_.Connector.Status == MyShipConnectorStatus.Connected;
                        if (connected != connected_)
                            dc_.ReconstructOnConnector = true;
                        connected_ = connected;
                        dc_.ReferenceGrid = connected ? dc_.Connector.OtherConnector : null;
                    }
                    else
                        dc_.ReferenceGrid = null;
                }
            }
            #endregion // Jobs

            #region Helper
            public virtual bool isSameCollector(string name, Configuration.Options options, string connector) => 
                CollectorTypeName == name && Options.equals(options) && ConnectorName == connector;

            protected delegate void GetBlockDelegate<BlockType>(BlockType block) where BlockType: class;

            /*!
             * Get all blocks of type x
             */
            protected bool getBlocks<BlockType>(GetBlockDelegate<BlockType> callback, string typeId = "") where BlockType : class
            {
                App.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(null, (block) =>
                {
                    BlockType type = block as BlockType;
                    if (type != null)
                    {
                        if (!block.IsSameConstructAs(ReferenceGrid))
                            return false;

                        if (typeId == "" || (typeId != "" && block.BlockDefinition.TypeIdString == typeId))
                            callback(type);
                    }

                    //log(Console.LogType.Error, $"Failed to find blocks of type {typeId}");
                    return false;
                });

                return true;
            }

            /*!
             */
            protected bool getBlocks<BlockType>(string name, bool isGroup, GetBlockDelegate<BlockType> callback, string typeId = "") where BlockType : class
            {
                Func<IMyTerminalBlock, bool, bool> check = (terminalBlock, ignoreTypeErr) =>
                {
                    BlockType type = terminalBlock as BlockType;
                    if (type != null)
                    {
                        if ((typeId == "" || (typeId != "" && terminalBlock.BlockDefinition.TypeIdString == typeId)) &&
                            terminalBlock.IsSameConstructAs(ReferenceGrid))
                        {
                            callback(type);
                            return true;
                        }
                        else if (!ignoreTypeErr)
                            log(Console.LogType.Error, $"Block isn't of type {typeId}");
                    }
                    else if (!ignoreTypeErr)
                        log(Console.LogType.Error, $"Block \"{name}\" has type missmatch");

                    return false;
                };

                if (isGroup == true)
                {
                    IMyBlockGroup group = App.GridTerminalSystem.GetBlockGroupWithName(name);
                    if (group != null)
                    {
                        group.GetBlocks(null, (block) =>
                        {
                            check(block, true);
                            return false;
                        });
                    }
                }
                else
                {
                    IMyTerminalBlock block = App.GridTerminalSystem.GetBlockWithName(name);
                    if (block == null)
                        log(Console.LogType.Error, $"Block \"{name}\" dosen't exists");
                    else
                        return check(block, false);
                }

                return false;
            }

            public static bool isOn(IMyTerminalBlock block)
            {
                var prop = block.GetProperty("OnOff");
                if (prop != null)
                    return block.GetValue<bool>("OnOff") && block.IsFunctional;
                return block.IsFunctional;
            }

            public static double clamp(double value, double min = 0.0, double max = 1.0) => value < min ? min : (value > max ? max : value);
            public static float clamp(float value, float min = 0f, float max = 1f) => value < min ? min : (value > max ? max : value);
            public static long clamp(long value, long min = 0, long max = 1) => value < min ? min : (value > max ? max : value);
            //public static int clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
            #endregion // Helper
        }
    }
}
