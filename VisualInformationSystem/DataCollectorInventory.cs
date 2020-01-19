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
            public DataCollectorInventory(Configuration.Options options)
                : base("inventory", "", options)
            {
            }

            #region Construction Part
            int constructStage_ = 0;
            List<IMyTerminalBlock> construtBlocks_ = new List<IMyTerminalBlock>();
            int constructIndex_ = 0;

            void addInventoryBlock(IMyTerminalBlock block)
            {
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
                                Blocks.Add(block);
                                maxVolume_ += (double)inventory.MaxVolume;
                                break;
                            }
                        }
                    }
                    else
                    {
                        inventories_.Add(inventory);
                        Blocks.Add(block);
                        maxVolume_ += (double)inventory.MaxVolume;
                    }
                }
            }

            public override bool construct()
            {
                if (constructStage_ == 0)
                {
                    BlockName = Options[0];
                    IsGroup = Options.asBoolean(1, false);

                    // process item type
                    if (Options.Count > 2)
                    {
                        for (int it = 2; it < Options.Count; ++it)
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
                                VISItemType itemType = Default.ItemTypeMap.FirstOrDefault(pair => pair.Key == itemTypeName.ToLower()).Value;
                                if (!itemType) // try as typeId string
                                    itemType = itemTypeName;

                                if (!itemType)
                                {
                                    log(Console.LogType.Error, $"Invalid item type:{itemTypeName}");
                                    return false;
                                }

                                long defaultAmount = Default.AmountItems.FirstOrDefault(pair => pair.Key == itemType).Value;
                                itemTypes_.Add(new ItemType(itemType, defaultAmount > 0 ? defaultAmount : defaultMaxAmountItems_));
                            }
                        }
                    }

                    constructStage_ = 1;
                }
                else if (constructStage_ == 1)
                {
                    if (BlockName != "")
                    {
                        getBlocks<IMyTerminalBlock>(BlockName, IsGroup, (block) =>
                        {
                            if (block.HasInventory)
                                construtBlocks_.Add(block);
                        }, TypeID);
                    }
                    else
                    {
                        if (!getBlocks<IMyTerminalBlock>((block) =>
                        {
                            if (block.HasInventory)
                                construtBlocks_.Add(block);
                        }, TypeID))
                        {
                            log(Console.LogType.Error, $"Failed to find blocks of type {TypeID}");
                            return false;
                        }
                    }

                    constructStage_ = 2;
                }
                else if (constructStage_ == 2)
                {
                    for (; constructIndex_ < construtBlocks_.Count && 
                        App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount; 
                        constructIndex_++)
                        addInventoryBlock(construtBlocks_[constructIndex_]);

                    if (constructIndex_ >= construtBlocks_.Count)
                        constructStage_ = 3;
                }
                else if (constructStage_ == 3)
                {
                    construtBlocks_.Clear();
                    if (inventories_.Count == 0)
                        log(Console.LogType.Warning, $"No inventory blocks found:{BlockName}/{TypeID}");

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
                construtBlocks_.Clear();
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
                itemRatio_ = maxItems_ != 0 ? clamp((double)currentItems_ / (double)maxItems_, 0.0, 1.0) : 0.0;

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

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxitems%", new ValueType(maxItems_).pack().ToString())
                    .Replace("%currentitems%", new ValueType(currentItems_).pack().ToString())
                    .Replace("%itemratio%", new ValueType(itemRatio_, unit: Unit.Percent).pack().ToString())
                    .Replace("%maxvolume%", new ValueType(maxVolume_, unit: Unit.l).pack().ToString())
                    .Replace("%currentvolume%", new ValueType(currentVolume_, unit: Unit.l).pack().ToString())
                    .Replace("%volumeratio%", new ValueType(volumeRatio_, unit: Unit.Percent).pack().ToString());
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
                    case "":
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
                public override ValueType min() => new ValueType(0.0, Multiplier.K, Unit.l);
                public override ValueType max() => new ValueType(inv_.maxVolume_, Multiplier.K, Unit.l);
                public override ValueType value() => new ValueType(inv_.currentVolume_, Multiplier.K, Unit.l);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var inventory in inv_.inventories_)
                    {
                        double indicator = (double)inventory.CurrentVolume.RawValue / (double)inventory.MaxVolume.RawValue;

                        ListContainer item = new ListContainer();
                        item.indicator = clamp(indicator, 0.0, 1.0);
                        item.min = new ValueType(0, Multiplier.K, Unit.l);
                        item.max = new ValueType((double)inventory.MaxVolume, Multiplier.K, Unit.l);
                        item.value = new ValueType((double)inventory.CurrentVolume, Multiplier.K, Unit.l);
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
                public override ValueType min() => new ValueType(0.0);
                public override ValueType max() => new ValueType(inv_.maxItems_);
                public override ValueType value() => new ValueType(inv_.currentItems_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(var item in inv_.items_)
                    {
                        double indicator = (double)item.currentAmount / (double)item.maxAmount;

                        ListContainer entry = new ListContainer();
                        entry.indicator = clamp(indicator, 0.0, 1.0);
                        entry.min = new ValueType(0.0);
                        entry.max = new ValueType(item.maxAmount);
                        entry.value = new ValueType(item.currentAmount);
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
