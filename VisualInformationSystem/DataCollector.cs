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
            public DataCollector(string collectorTypeName, string typeId, Configuration.Options options)
            {
                if (options == null)
                    options_ = new Configuration.Options(new List<string>());
                else
                    options_ = options;

                TypeID = typeId;
                CollectorTypeName = collectorTypeName;
                UpdateFinished = false;
                nextReconstruct_ = Manager.Timer.Ticks + Default.ReconstructInterval;
            }

            public override bool construct()
            {
                BlockName = Options[0];
                IsGroup = Options.asBoolean(1, false);

                if (BlockName != "")
                {
                    getBlocks<T>(BlockName, IsGroup, (block) =>
                    {
                        blocks_.Add(block);
                    }, TypeID);
                }
                else
                {
                    if (!getBlocks<T>((block) =>
                    {
                        blocks_.Add(block);
                    }, TypeID))
                    {
                        log(Console.LogType.Error, $"Failed to find blocks of type {TypeID}");
                        return false;
                    }
                }

                if (blocks_.Count == 0)
                    log(Console.LogType.Warning, $"No blocks found for {BlockName}:{(IsGroup ? "group" : "")}");
                Constructed = true;
                return true;
            }

            public virtual bool reconstruct()
            {
                blocks_.Clear();
                Constructed = false;
                return true;
            }

            public virtual string getText(string data)
            {
                return data
                    .Replace("%blockcount%", Blocks.Count.ToString())
                    .Replace("%blockname%", BlockName)
                    .Replace("%isgroup%", IsGroup ? "true" : "false")
                    .Replace("%on%", blocksOn_.ToString())
                    .Replace("%off%", blocksOff_.ToString())
                    .Replace("%onratio%", new ValueType(onRatio_, unit: Unit.Percent).pack().ToString())
                    .Replace("%offratio%", new ValueType(offRatio_, unit: Unit.Percent).pack().ToString());
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
            }

            public virtual void finalizeUpdate()
            {
                blocksOff_ = Blocks.Count - blocksOn_;
                onRatio_ = (float)blocksOn_ / (float)Blocks.Count;
                offRatio_ = (float)blocksOff_ / (float)Blocks.Count;
            }

            protected virtual void update()
            {
                foreach (IMyTerminalBlock block in Blocks)
                    blocksOn_ += isOn(block) ? 1 : 0;
                UpdateFinished = true;
            }

            public virtual Job getUpdateJob()
            {
                if (nextUpdate_ <= Manager.Timer.Ticks)
                    return new UpdateJob(this);
                return null;
            }
            #endregion // Update System

            #region Properties
            public string CollectorTypeName
            {
                get;
                protected set;
            }

            Configuration.Options options_ = null;
            public Configuration.Options Options
            {
                get { return options_; }
            }

            public string TypeID
            {
                get;
                protected set;
            }

            List<T> blocks_ = new List<T>();
            protected List<T> Blocks
            {
                get { return blocks_; }
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

            TimeSpan nextUpdate_ = new TimeSpan(0);
            TimeSpan nextReconstruct_ = new TimeSpan(0);
            TimeSpan maxInterval_ = Default.DCRefresh;
            public TimeSpan MaxUpdateInterval
            {
                get { return maxInterval_; }
                set { maxInterval_ = value; }
            }
            #endregion // Properties

            #region Data Accessor
            protected int blocksOn_ = 0;
            protected int blocksOff_ = 0;
            protected float onRatio_ = 0f;
            protected float offRatio_ = 0f;

            public virtual DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "on":
                        return new DAOn(this);
                    case "off":
                        return new DAOff(this);
                }

                log(Console.LogType.Error, $"Invalid data retriever {name}");
                return null;
            }

            class DAOn : DataAccessor
            {
                DataCollector<T> dc_;
                public DAOn(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.onRatio_;
                public override ValueType min() => new ValueType(0);
                public override ValueType max() => new ValueType(dc_.Blocks.Count);
                public override ValueType value() => new ValueType(dc_.blocksOn_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = isOn(block) ? 1.0 : 0.0;
                        item.value = new ValueType(item.indicator);
                        item.min = new ValueType(0);
                        item.max = new ValueType(1);

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
                public override ValueType min() => new ValueType(0);
                public override ValueType max() => new ValueType(dc_.Blocks.Count);
                public override ValueType value() => new ValueType(dc_.blocksOff_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = isOn(block) ? 0.0 : 1.0;
                        item.value = new ValueType(item.indicator);
                        item.min = new ValueType(0);
                        item.max = new ValueType(1);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor

            #region Jobs
            class UpdateJob : Job
            {
                public UpdateJob(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                DataCollector<T> dc_ = null;

                public override void prepareJob()
                {
                    dc_.prepareUpdate();
                    JobFinished = false;
                }

                public override void finalizeJob()
                {
                    dc_.finalizeUpdate();
                    dc_.nextUpdate_ = dc_.maxInterval_ + Manager.Timer.Ticks;

                    if (dc_.nextReconstruct_ <= Manager.Timer.Ticks)
                        JobManager.queueJob(new ReconstructJob(dc_));
                }

                public override void tick(TimeSpan delta)
                {
                    dc_.update();
                    JobFinished = dc_.UpdateFinished;
                }
            }


            class ReconstructJob : Job
            {
                public ReconstructJob(DataCollector<T> dc)
                {
                    dc_ = dc;
                }

                DataCollector<T> dc_ = null;

                public override void prepareJob()
                {
                    dc_.reconstruct();
                    JobFinished = false;
                }

                public override void finalizeJob()
                {
                    dc_.nextReconstruct_ = Manager.Timer.Ticks + Default.ReconstructInterval;
                }

                public override void tick(TimeSpan delta)
                {
                    if (dc_.construct() == false)
                    {
                        log(Console.LogType.Error, "Reconstruction job failed");
                        Manager.switchState(VISManager.State.Error);
                    }

                    JobFinished = dc_.Constructed;
                }
            }
            #endregion // Jobs

            #region Helper
            public virtual bool isSameCollector(IDataCollector other)
            {
                if (GetType() != other.GetType())
                    return false;

                return options_ == other.Options && TypeID == other.TypeID;
            }

            public virtual bool isSameCollector(string name, Configuration.Options options)
            {
                return CollectorTypeName == name && Options == options;
            }

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
                        if (!block.IsSameConstructAs(App.Me))
                            return false;

                        if (typeId == "" || (typeId != "" && block.BlockDefinition.TypeIdString == typeId))
                            callback(type);
                    }

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
                        if (typeId == "" || (typeId != "" && terminalBlock.BlockDefinition.TypeIdString == typeId))
                        {
                            callback(type);
                            return true;
                        }
                        else if (!ignoreTypeErr)
                            log(Console.LogType.Error, $"Block isn't of type {typeId}");
                    }
                    else if (!ignoreTypeErr)
                        log(Console.LogType.Error, $"Block {name} has type missmatch");

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
                    {
                        log(Console.LogType.Error, $"Block of name {name} dosen't exists");
                        return false;
                    }

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

            public static double clamp(double value, double min, double max) => value < min ? min : (value > max ? max : value);
            //public static float clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
            public static long clamp(long value, long min, long max) => value < min ? min : (value > max ? max : value);
            //public static int clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
            #endregion // Helper
        }
    }
}
