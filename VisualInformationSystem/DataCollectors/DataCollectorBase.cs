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
        public abstract class DataCollectorBase<T> : DataCollector where T : class
        {
            public DataCollectorBase(string collectorTypeName, string typeId, Configuration.Options options, string connector)
                : base(collectorTypeName, typeId, options, connector)
            {
                Blocks = new List<T>();
                AcceptBlock = (block) => Blocks.Add(block);

                if (connector == "")
                {
                    BlockName = Options[0];
                    IsGroup = Options.asBoolean(1, false);
                }
                else
                {
                    BlockName = Options[1];
                    IsGroup = Options.asBoolean(2, false);
                }
            }

            public override bool construct()
            {
                if (ReferenceGrid != null)
                {
                    if (BlockName != "")
                        getBlocks<T>(BlockName, IsGroup, AcceptBlock, TypeID);
                    else if (!getBlocks<T>(AcceptBlock, TypeID))
                        return false;
                }
                else if (ConnectorName == "")
                    log(Console.LogType.Error, $"No reference block");

                if (Blocks.Count == 0 && ConnectorName == "")
                    log(Console.LogType.Warning, $"No blocks found {Name}[{BlockName}{(IsGroup ? ":group" : "")}]");
                Constructed = true;
                return true;
            }

            public override bool reconstruct()
            {
                Blocks.Clear();
                return base.reconstruct();
            }

            public override void prepareUpdate()
            {
                blocksOn_ = 0;
                blocksFunctional_ = 0;
                UpdateFinished = false;
            }

            public override void finalizeUpdate()
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

            protected override void update()
            {
                foreach (IMyTerminalBlock block in Blocks)
                {
                    blocksOn_ += isOn(block) ? 1 : 0;
                    blocksFunctional_ += block.IsFunctional ? 1 : 0;
                }

                UpdateFinished = true;
            }

            public override string getVariable(string data)
            {
                return data
                    .Replace("%blockcount%", Blocks.Count.ToString())
                    .Replace("%blockname%", BlockName)
                    .Replace("%gridname%", ReferenceGrid != null ? ReferenceGrid.CubeGrid.CustomName : "")
                    .Replace("%isgroup%", IsGroup ? "true" : "false")
                    .Replace("%on%", blocksOn_.ToString())
                    .Replace("%off%", blocksOff_.ToString())
                    .Replace("%onratio%", new VISUnitType(onRatio_, unit: Unit.Percent))
                    .Replace("%offratio%", new VISUnitType(offRatio_, unit: Unit.Percent))
                    .Replace("%functional%", blocksFunctional_.ToString())
                    .Replace("%functionalratio%", new VISUnitType(blocksFunctionalRatio_, unit: Unit.Percent));
            }

            protected GetBlockDelegate<T> AcceptBlock
            {
                get;
                set;
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

            #region Data Accessor
            protected int blocksOn_ = 0;
            protected int blocksOff_ = 0;
            protected float onRatio_ = 0f;
            protected float offRatio_ = 0f;

            protected int blocksFunctional_ = 0;
            protected float blocksFunctionalRatio_ = 0f;

            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "on":
                        return new DAOn(this);
                    case "off":
                        return new DAOff(this);
                    case "functional":
                        return new DAFunctional(this);
                }

                return base.getDataAccessor(name);
            }

            class DAFunctional : DataAccessor
            {
                DataCollectorBase<T> dc_;
                public DAFunctional(DataCollectorBase<T> dc)
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
                DataCollectorBase<T> dc_;
                public DAOn(DataCollectorBase<T> dc)
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
                DataCollectorBase<T> dc_;
                public DAOff(DataCollectorBase<T> dc)
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

            #region Helper
            protected delegate void GetBlockDelegate<BlockType>(BlockType block) where BlockType : class;

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
            #endregion // Helper
        }
    }
}
