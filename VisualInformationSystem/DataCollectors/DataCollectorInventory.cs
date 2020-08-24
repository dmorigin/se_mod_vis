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
        public class DataCollectorInventory : DataCollectorBase<IMyTerminalBlock>
        {
            public DataCollectorInventory(Configuration.Options options, string typeId, string connector)
                : base("inventory", typeId, options, connector)
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

            class CheckInventoryBlock
            {
                DataCollectorInventory dc_ = null;
                IMyTerminalBlock block_ = null;

                public CheckInventoryBlock(DataCollectorInventory dc, IMyTerminalBlock block)
                {
                    dc_ = dc;
                    block_ = block;
                    Check = checkBlock;
                }

                public Func<bool> Check
                {
                    get;
                    private set;
                }

                bool addBlock = false;
                int inventoryIndex_ = 0;
                bool checkBlock()
                {
                    if (inventoryIndex_ < block_.InventoryCount)
                    {
                        var inventory = block_.GetInventory(inventoryIndex_);
                        acceptedItems_.Clear();
                        acceptedItemIndex_ = 0;
                        inventory.GetAcceptedItems(null, (itemType) =>
                        {
                            int index = dc_.itemTypeFilter_.FindIndex(x => VISItemType.compareItemTypes(x.type, itemType));
                            ItemType item = new ItemType();
                            item.type = itemType;

                            if (index >= 0)
                            {
                                item.amount = dc_.itemTypeFilter_[index].amount;
                                acceptedItems_.Add(item);
                            }
                            else if (dc_.itemTypeFilter_.Count == 0)
                            {
                                long defaultAmount = Default.AmountItems.FirstOrDefault(pair => pair.Key.Equals(item.type)).Value;
                                item.amount = defaultAmount > 0 ? defaultAmount : dc_.defaultMaxAmountItems_;
                                acceptedItems_.Add(item);
                            }

                            return false;
                        });

                        if (acceptedItems_.Count > 0)
                        {
                            dc_.inventories_.Add(inventory);
                            dc_.maxVolume_ += (double)inventory.MaxVolume;
                            addBlock = true;
                        }

                        Check = checkInventory;
                        return false;
                    }

                    if (addBlock)
                        dc_.Blocks.Add(block_);
                    return true;
                }

                struct ItemType
                {
                    public MyItemType type;
                    public long amount;
                }

                List<ItemType> acceptedItems_ = new List<ItemType>();
                int acceptedItemIndex_ = 0;
                bool checkInventory()
                {
                    for (; acceptedItemIndex_ < acceptedItems_.Count &&
                        dc_.App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount;
                        ++acceptedItemIndex_)
                    {
                        var itemType = acceptedItems_[acceptedItemIndex_];

                        // check item exists
                        int index = dc_.items_.FindIndex((item) => item.type.Equals(itemType.type));
                        var amount = block_.GetInventory(inventoryIndex_).GetItemAmount(itemType.type);

                        if (index >= 0)
                            dc_.items_[index].currentAmount += (long)amount;
                        else
                        {
                            InventoryItem invItem = new InventoryItem();
                            invItem.type = itemType.type;
                            invItem.maxAmount = itemType.amount;
                            invItem.currentAmount = (long)amount;
                            dc_.items_.Add(invItem);
                        }
                    }

                    if (acceptedItemIndex_ >= acceptedItems_.Count)
                    {
                        Check = checkBlock;
                        inventoryIndex_++;
                    }
                    return false;
                }
            }

            bool runStage_ = false;
            CheckInventoryBlock blockScanner_ = null;
            public override bool construct()
            {
                if (constructStage_ == 0)
                {
                    if (ReferenceGrid != null)
                    {
                        if (BlockName != "")
                            getBlocks(BlockName, IsGroup, AcceptBlock, TypeID);
                        else if (!getBlocks(AcceptBlock, TypeID))
                            return false;

                        constructStage_ = 1;
                    }
                    else
                    {
                        if (ConnectorName == "")
                            log(Console.LogType.Error, $"No reference block");
                        Constructed = true;
                    }
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
                            if (itemTypeFilter_.Count > 0)
                                itemTypeFilter_[itemTypeFilter_.Count - 1].amount = amount;
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
                            itemTypeFilter_.Add(new ItemType(itemType, defaultAmount > 0 ? defaultAmount : defaultMaxAmountItems_));
                        }
                    }

                    constructStage_ = 2;
                }
                else if (constructStage_ == 2)
                {
                    if (!runStage_)
                    {
                        itemTypeFilter_.Sort((a, b) => a.type.Group && !b.type.Group ? 1 : (!a.type.Group && b.type.Group ? -1 : 0));
                        runStage_ = true;
                    }

                    while (constructIndex_ < constructBlocks_.Count &&
                        App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount)
                    {
                        if (blockScanner_ == null)
                            blockScanner_ = new CheckInventoryBlock(this, constructBlocks_[constructIndex_]);

                        if(blockScanner_.Check())
                        {
                            blockScanner_ = null;
                            constructIndex_++;
                        }
                    }

                    if (constructIndex_ >= constructBlocks_.Count)
                        constructStage_ = 3;
                }
                else if (constructStage_ == 3)
                {
                    constructBlocks_.Clear();
                    if (inventories_.Count == 0)
                        log(Console.LogType.Warning, $"No inventories found {Name}[{BlockName}{(IsGroup ? ":group" : "")}]");

                    if (itemTypeFilter_.Count > 0)
                        foreach (var it in itemTypeFilter_)
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
                itemTypeFilter_.Clear();
                inventories_.Clear();
                constructStage_ = 0;
                constructIndex_ = 0;
                constructBlocks_.Clear();
                items_.Clear();
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

                foreach (var item in items_)
                    item.currentAmount = 0;
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

                    foreach(var item in items_)
                    {
                        var amount = inventory.GetItemAmount(item.type);
                        item.currentAmount += (long)amount;
                        currentItems_ += (long)amount;
                    }
                }

                if (invIndex_ >= inventories_.Count)
                    base.update();
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

            List<ItemType> itemTypeFilter_ = new List<ItemType>();
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
                    .Replace("%volumeratio%", new VISUnitType(volumeRatio_, unit: Unit.Percent))
                    .Replace("%inventories%", inventories_.Count.ToString())
                    .Replace("%itemtypes%", itemTypeFilter_.Count.ToString());
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
