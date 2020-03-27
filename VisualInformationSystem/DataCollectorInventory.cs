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
        public class DataCollectorInventory : DataCollector<IMyTerminalBlock>
        {
            public DataCollectorInventory(Configuration.Options options, string connector)
                : base("inventory", "", options, connector)
            {
                AcceptBlock = (block) =>
                {
                    if (block.HasInventory)
                        constructBlocks_.Add(block);
                };
            }

            #region Construction Part
            int constructStage_ = 0;
            List<IMyTerminalBlock> constructBlocks_ = new List<IMyTerminalBlock>();
            int constructIndex_ = 0;

            void addInventoryBlock(IMyTerminalBlock block)
            {
                bool addBlock = false;

                for (int i = 0; i < block.InventoryCount; i++)
                {
                    IMyInventory inventory = block.GetInventory(i);

                    if (itemTypes_.Count > 0)
                    {
                        List<MyItemType> acceptedItems = new List<MyItemType>();
                        inventory.GetAcceptedItems(acceptedItems);
                        foreach (ItemType item in itemTypes_)
                        {
                            if (acceptedItems.Exists(x => item.type.Equals(x)))
                            {
                                inventories_.Add(inventory);
                                //Blocks.Add(block);
                                addBlock = true;
                                maxVolume_ += (double)inventory.MaxVolume;
                                break;
                            }
                        }
                    }
                    else
                    {
                        inventories_.Add(inventory);
                        //Blocks.Add(block);
                        addBlock = true;
                        maxVolume_ += (double)inventory.MaxVolume;
                    }
                }

                if (addBlock == true)
                    Blocks.Add(block);
            }

            public override bool construct()
            {
                if (constructStage_ == 0)
                {
                    if (ConnectorName != "" && Connector == null)
                        log(Console.LogType.Error, $"Connector \"{ConnectorName}\" not found");

                    if (ReferenceGrid != null)
                    {
                        if (BlockName != "")
                            getBlocks<IMyTerminalBlock>(BlockName, IsGroup, AcceptBlock, TypeID);
                        else if (!getBlocks<IMyTerminalBlock>(AcceptBlock, TypeID))
                            return false;

                        constructStage_ = 1;
                    }
                    else
                        Constructed = true;
                }
                else if (constructStage_ == 1)
                {
                    // process item type
                    for (int it = ConnectorName != "" ? 3 : 2; it < Options.Count; ++it)
                    {
                        string itemTypeName = Options[it];
                        int amount = 0;

                        if (int.TryParse(itemTypeName, out amount))
                        {
                            if (itemTypes_.Count > 0)
                                itemTypes_[itemTypes_.Count - 1].amount = amount;
                            else
                                defaultMaxAmountItems_ = amount;
                        }
                        else
                        {
                            VISItemType itemType;
                            if (!Default.ItemTypeMap.TryGetValue(itemTypeName.ToLower(), out itemType))
                                itemType = $"{Default.MyObjectBuilder}_{itemTypeName}";

                            if (!itemType)
                            {
                                log(Console.LogType.Error, $"Invalid item type:{itemTypeName}");
                                return false;
                            }

                            long defaultAmount = Default.AmountItems.FirstOrDefault(pair => pair.Key == itemType).Value;
                            itemTypes_.Add(new ItemType(itemType, defaultAmount > 0 ? defaultAmount : defaultMaxAmountItems_));
                        }
                    }

                    constructStage_ = 2;
                }
                else if (constructStage_ == 2)
                {
                    itemTypes_.Sort((a, b) => a.type.Group && !b.type.Group ? 1 : (!a.type.Group && b.type.Group ? -1 : 0));

                    for (; constructIndex_ < constructBlocks_.Count && 
                        App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount; 
                        constructIndex_++)
                        addInventoryBlock(constructBlocks_[constructIndex_]);

                    if (constructIndex_ >= constructBlocks_.Count)
                        constructStage_ = 3;
                }
                else if (constructStage_ == 3)
                {
                    constructBlocks_.Clear();
                    if (inventories_.Count == 0)
                        log(Console.LogType.Warning, $"No blocks found {Name}[{BlockName}{(IsGroup ? ":group" : "")}]");

                    if (itemTypes_.Count > 0)
                        foreach (var it in itemTypes_)
                            maxItems_ += it.amount;
                    else
                    {
                        foreach (var it in Default.AmountItems)
                            maxItems_ += it.Key.Group ? it.Value * 10 : it.Value;
                    }

                    Constructed = true;
                }

                return true;
            }

            public override bool reconstruct()
            {
                maxItems_ = 0;
                maxVolume_ = 0.0;
                itemTypes_.Clear();
                inventories_.Clear();
                constructStage_ = 0;
                constructIndex_ = 0;
                constructBlocks_.Clear();
                return base.reconstruct();
            }
            #endregion // Construction Part

            #region Update Part
            int invIndex_ = 0;

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                currentVolume_ = 0.0;
                currentItems_ = 0;
                invIndex_ = 0;
                items_.Clear();
                UpdateFinished = false;
            }

            public override void finalizeUpdate()
            {
                volumeRatio_ = maxVolume_ != 0 ? currentVolume_ / maxVolume_ : 0.0;
                itemRatio_ = maxItems_ != 0 ? clamp((double)currentItems_ / (double)maxItems_) : 0.0;

                base.finalizeUpdate();
            }

            protected override void update()
            {
                for (; invIndex_ < inventories_.Count && 
                    App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount; 
                    invIndex_++)
                {
                    IMyInventory inventory = inventories_[invIndex_];
                    currentVolume_ += (double)inventory.CurrentVolume;

                    inventory.GetAcceptedItems(null, (itemType) =>
                    {
                        int itemTypeIndex = itemTypes_.FindIndex(x => VISItemType.compareItemTypes(x.type, itemType));
                        if (itemTypes_.Count > 0 && itemTypeIndex < 0)
                            return false;

                        var amount = inventory.GetItemAmount(itemType);
                        int index = items_.FindIndex(x => VISItemType.compareItemTypes(x.type, itemType));
                        if (index >= 0)
                        {
                            items_[index].currentAmount += (long)amount;
                            currentItems_ += (long)amount;
                        }
                        else
                        {
                            InventoryItem item = new InventoryItem();
                            item.currentAmount = (long)amount;
                            item.type = itemType;

                            if (itemTypeIndex >= 0)
                                item.maxAmount = itemTypes_[itemTypeIndex].amount;
                            else
                            {
                                long defaultAmount = Default.AmountItems.FirstOrDefault(pair => pair.Key.Equals(item.type)).Value;
                                item.maxAmount = defaultAmount > 0 ? defaultAmount : defaultMaxAmountItems_;
                            }

                            items_.Add(item);
                        }

                        return false;
                    });
                }

                if (invIndex_ >= inventories_.Count)
                {
                    base.update();
                    UpdateFinished = true;
                }
            }
            #endregion // Update Part

            class ItemType
            {
                public ItemType(MyItemType itemType, long a)
                {
                    type = itemType;
                    amount = a;
                }

                public VISItemType type;
                public long amount;
            }

            List<ItemType> itemTypes_ = new List<ItemType>();
            List<IMyInventory> inventories_ = new List<IMyInventory>();
            double currentVolume_ = 0;
            double maxVolume_ = 0;
            double volumeRatio_ = 0;
            long maxItems_ = 0;
            long currentItems_ = 0;
            double itemRatio_ = 0;

            public override string getVariable(string data)
            {
                return base.getVariable(data)
                    .Replace("%maxitems%", new VISUnitType(maxItems_).pack())
                    .Replace("%currentitems%", new VISUnitType(currentItems_).pack())
                    .Replace("%itemratio%", new VISUnitType(itemRatio_, unit: Unit.Percent))
                    .Replace("%maxvolume%", new VISUnitType(maxVolume_, unit: Unit.Liter).pack())
                    .Replace("%currentvolume%", new VISUnitType(currentVolume_, unit: Unit.Liter).pack())
                    .Replace("%volumeratio%", new VISUnitType(volumeRatio_, unit: Unit.Percent));
            }

            long defaultMaxAmountItems_ = Default.MaxAmountItems;

            class InventoryItem
            {
                public long currentAmount;
                public long maxAmount;
                public MyItemType type;
            }
            List<InventoryItem> items_ = new List<InventoryItem>();

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "capacity":
                        return new Capacity(this);
                    case "items":
                        return new Items(this);
                }

                return base.getDataAccessor(name);
            }

            class Capacity : DataAccessor
            {
                DataCollectorInventory inv_ = null;
                public Capacity(DataCollectorInventory inventory)
                {
                    inv_ = inventory;
                }

                public override double indicator() => inv_.volumeRatio_;
                public override VISUnitType min() => new VISUnitType(0.0, Multiplier.K, Unit.Liter);
                public override VISUnitType max() => new VISUnitType(inv_.maxVolume_, Multiplier.K, Unit.Liter);
                public override VISUnitType value() => new VISUnitType(inv_.currentVolume_, Multiplier.K, Unit.Liter);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var inventory in inv_.inventories_)
                    {
                        double indicator = (double)inventory.CurrentVolume.RawValue / (double)inventory.MaxVolume.RawValue;

                        ListContainer item = new ListContainer();
                        item.indicator = clamp(indicator);
                        item.min = new VISUnitType(0, Multiplier.K, Unit.Liter);
                        item.max = new VISUnitType((double)inventory.MaxVolume, Multiplier.K, Unit.Liter);
                        item.value = new VISUnitType((double)inventory.CurrentVolume, Multiplier.K, Unit.Liter);
                        item.name = (inventory.Owner as IMyTerminalBlock).CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Items : DataAccessor
            {
                DataCollectorInventory inv_ = null;
                public Items(DataCollectorInventory inv)
                {
                    inv_ = inv;
                }

                public override double indicator() => inv_.itemRatio_;
                public override VISUnitType min() => new VISUnitType(0.0);
                public override VISUnitType max() => new VISUnitType(inv_.maxItems_);
                public override VISUnitType value() => new VISUnitType(inv_.currentItems_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(var item in inv_.items_)
                    {
                        double indicator = (double)item.currentAmount / (double)item.maxAmount;

                        ListContainer entry = new ListContainer();
                        entry.indicator = clamp(indicator);
                        entry.min = new VISUnitType(0.0);
                        entry.max = new VISUnitType(item.maxAmount);
                        entry.value = new VISUnitType(item.currentAmount);
                        entry.name = item.type.SubtypeId;
                        entry.type = item.type;

                        if (filter == null || (filter != null && filter(entry)))
                            container.Add(entry);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
