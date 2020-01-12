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
            public DataCollector(string typeId, Configuration.Options options)
            {
                if (options == null)
                    options_ = new Configuration.Options(new List<string>());
                else
                    options_ = options;

                typeId_ = typeId;
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
                    }, typeId_);
                }
                else
                {
                    if (!getBlocks<T>((block) =>
                    {
                        blocks_.Add(block);
                    }, typeId_))
                    {
                        log(Console.LogType.Error, $"Failed to find blocks of type {typeId_}");
                        return false;
                    }
                }

                if (blocks_.Count == 0)
                    log(Console.LogType.Warning, $"No blocks found for {blockName_}:{(isGroup_ ? "group" : "")}");
                Constructed = true;
                return true;
            }

            public virtual bool reconstruct()
            {
                blocks_.Clear();
                Constructed = false;
                return true;
            }

            #region Update System
            bool updateFinished_ = false;
            public bool UpdateFinished
            {
                get { return updateFinished_; }
                protected set { updateFinished_ = value; }
            }

            public virtual void prepareUpdate()
            {
                updateFinished_ = true;
            }

            public virtual void finalizeUpdate()
            {
            }

            protected virtual void update()
            {
            }

            public virtual Job getUpdateJob()
            {
                if (nextUpdate_ <= Manager.Timer.Ticks)
                    return new UpdateJob(this);
                return null;
            }
            #endregion // Update System

            #region Properties
            public virtual string CollectorTypeName
            {
                get;
            }

            Configuration.Options options_ = null;
            public Configuration.Options Options
            {
                get { return options_; }
            }

            string typeId_ = "";
            public string TypeID
            {
                get { return typeId_; }
            }

            List<T> blocks_ = new List<T>();
            protected List<T> Blocks
            {
                get { return blocks_; }
            }

            bool isGroup_ = false;
            public bool IsGroup
            {
                get { return isGroup_; }
                protected set { isGroup_ = value; }
            }

            string blockName_ = "";
            public string BlockName
            {
                get { return blockName_; }
                protected set { blockName_ = value; }
            }

            TimeSpan nextUpdate_ = new TimeSpan(0);
            TimeSpan nextReconstruct_ = new TimeSpan(0);
            TimeSpan maxInterval_ = Default.DCRefresh;
            public TimeSpan MaxUpdateInterval
            {
                get { return maxInterval_; }
                set { maxInterval_ = value; }
            }

            public virtual string getText(string data)
            {
                return data.Replace("%blockcount%", Blocks.Count.ToString())
                    .Replace("%blockname%", BlockName)
                    .Replace("%isgroup%", IsGroup ? "true" : "false");
            }
            #endregion // Properties

            public virtual bool isSameCollector(IDataCollector other)
            {
                if (GetType() != other.GetType())
                    return false;

                return options_ == other.Options && typeId_ == other.TypeID;
            }

            public virtual bool isSameCollector(string name, Configuration.Options options)
            {
                return CollectorTypeName == name && Options == options;
            }

            public abstract DataAccessor getDataAccessor(string name);


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
                return block.GetValue<bool>("OnOff") && block.IsFunctional;
            }
        }
    }
}
