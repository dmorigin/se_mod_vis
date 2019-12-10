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
        public abstract class DataCollector<T> : Job, IDataCollector where T : class
        {
            public DataCollector(string typeId, Configuration.Options options)
            {
                if (options == null)
                    options_ = new Configuration.Options(new List<string>());
                else
                    options_ = options;

                typeId_ = typeId;
            }


            public override bool construct()
            {
                if (Options.Count < 2)
                {
                    log(Console.LogType.Error, $"Invalid options for check type");
                    return false;
                }

                BlockName = Options[0];
                IsGroup = Options.getAsBoolean(1, false);

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

                Constructed = true;
                return true;
            }

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

            public virtual string getText(string data)
            {
                return data;
            }

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


            public abstract DataRetriever getDataRetriever(string name);


            /*!
             * Internal method which is called if this object needs to be
             * updateing it's state.
             */
            protected virtual void update()
            {
            }


            TimeSpan lastUpdate_ = new TimeSpan();
            TimeSpan maxInterval_ = TimeSpan.FromSeconds(1.0);


            public override void tick(TimeSpan delta)
            {
                if ((lastUpdate_ + maxInterval_) < Manager.Timer.Ticks)
                {
                    update();
                    lastUpdate_ = Manager.Timer.Ticks;
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
        }
    }
}
