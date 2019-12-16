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
                : base("", options)
            {
            }

            bool compareItemTypes(MyItemType a, MyItemType b)
            {
                if (a.SubtypeId != "" && b.SubtypeId != "")
                    return a.TypeId == b.TypeId && a.SubtypeId == b.SubtypeId;

                return a.TypeId == b.TypeId;
            }

            #region Construction Part
            string pattern = "^(MyObjectBuilder_[Ingot|Ore|PhysicalGunObject|Component|AmmoMagazine])[/]{0,1}([0-9a-zA-Z]+)$";
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
                            if (acceptedItems.Exists(x => compareItemTypes(x, item.type)))
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
                            string itemType = Options[it];
                            switch (itemType.ToLower())
                            {
                                case "ammo":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_AmmoMagazine", ""), defaultMaxAmountItems_));
                                    continue;
                                case "component":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Component", ""), defaultMaxAmountItems_));
                                    continue;
                                case "handtool":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_PhysicalGunObject", ""), defaultMaxAmountItems_));
                                    continue;
                                case "ore":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Ore", ""), defaultMaxAmountItems_));
                                    continue;
                                case "ingot":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Ingot", ""), defaultMaxAmountItems_));
                                    continue;
                                case "ice":
                                    itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Ore", "Ice"), defaultMaxAmountItems_));
                                    continue;
                            }

                            int amount = 0;
                            if (System.Text.RegularExpressions.Regex.IsMatch(itemType, pattern))
                            {
                                string[] parts = itemType.Split('/');
                                if (parts.Length == 1)
                                    itemTypes_.Add(new ItemType(new MyItemType(parts[0], ""), defaultMaxAmountItems_));
                                else if (parts.Length == 2)
                                    itemTypes_.Add(new ItemType(new MyItemType(parts[0], parts[1]), defaultMaxAmountItems_));
                                else
                                {
                                    log(Console.LogType.Error, $"Invalid item type:{itemType}");
                                    return false;
                                }
                                maxItems_ += defaultMaxAmountItems_;
                            }
                            else if (int.TryParse(itemType, out amount))
                            {
                                if (itemTypes_.Count > 0)
                                {
                                    itemTypes_[itemTypes_.Count - 1].amount = amount;
                                    maxItems_ -= defaultMaxAmountItems_ - amount;
                                }
                                else
                                    defaultMaxAmountItems_ = amount;
                            }
                            else
                            {
                                log(Console.LogType.Error, $"Invalid item type {itemType}");
                                return false;
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
                        App.Runtime.CurrentInstructionCount < Program.Default.MaxInstructionCount; 
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

                    maxItems_ = maxItems_ == 0 ? (defaultMaxAmountItems_ * inventories_.Count) : maxItems_;
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
                currentVolume_ = 0.0;
                currentItems_ = 0;
                invIndex_ = 0;
                items_.Clear();
                UpdateFinished = false;
            }

            protected override void update()
            {
                for (; invIndex_ < inventories_.Count && 
                    App.Runtime.CurrentInstructionCount < Program.Default.MaxInstructionCount; 
                    invIndex_++)
                {
                    IMyInventory inventory = inventories_[invIndex_];
                    currentVolume_ += (double)inventory.CurrentVolume;

                    inventory.GetAcceptedItems(null, (itemType) =>
                    {
                        int itemTypeIndex = itemTypes_.FindIndex(x => compareItemTypes(x.type, itemType));
                        if (itemTypes_.Count > 0 && itemTypeIndex < 0)
                            return false;

                        var amount = inventory.GetItemAmount(itemType);
                        int index = items_.FindIndex(x => compareItemTypes(x.type, itemType));
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
                                item.maxAmount = defaultMaxAmountItems_;

                            items_.Add(item);
                        }

                        return false;
                    });
                }

                if (invIndex_ >= inventories_.Count)
                {
                    UpdateFinished = true;
                    volumeRation_ = maxVolume_ != 0 ? currentVolume_ / maxVolume_ : 0.0;
                    itemRation_ = maxItems_ != 0 ? currentItems_ / maxItems_ : 0;
                    itemRation_ = itemRation_ > 1 ? 1 : itemRation_;
                }
            }
            #endregion // Update Part

            public override string CollectorTypeName
            {
                get { return "inventory"; }
            }

            class ItemType
            {
                public ItemType(MyItemType itemType, long a)
                {
                    type = itemType;
                    amount = a;
                }

                public MyItemType type;
                public long amount;
            }

            List<ItemType> itemTypes_ = new List<ItemType>();
            List<IMyInventory> inventories_ = new List<IMyInventory>();
            double currentVolume_ = 0;
            double maxVolume_ = 0;
            double volumeRation_ = 0;
            long maxItems_ = 0;
            long currentItems_ = 0;
            double itemRation_ = 0;

            long defaultMaxAmountItems_ = Program.Default.AmountItems;

            class InventoryItem
            {
                public long currentAmount;
                public long maxAmount;
                public MyItemType type;
            }
            List<InventoryItem> items_ = new List<InventoryItem>();

            #region Data Retriever
            public override DataRetriever getDataRetriever(string name)
            {
                switch (name.ToLower())
                {
                    case "":
                    case "capacity":
                        return new Capacity(this);
                    case "count":
                        return new Count(this);
                }

                log(Console.LogType.Error, $"Invalid data retriever {name}");
                return null;
            }

            class Capacity : DataRetriever
            {
                DataCollectorInventory inv_ = null;
                public Capacity(DataCollectorInventory inventory)
                {
                    inv_ = inventory;
                }

                public override double indicator()
                {
                    return inv_.volumeRation_;
                }

                public override ValueType min()
                {
                    return new ValueType(0.0, Multiplier.K, Unit.l);
                }

                public override ValueType max()
                {
                    return new ValueType(inv_.maxVolume_, Multiplier.K, Unit.l);
                }

                public override ValueType value()
                {
                    return new ValueType(inv_.currentVolume_, Multiplier.K, Unit.l);
                }

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var inventory in inv_.inventories_)
                    {
                        double indicator = inventory.CurrentVolume.RawValue / (double)inventory.MaxVolume.RawValue;

                        ListContainer item = new ListContainer();
                        item.onoff = true;
                        item.indicator = indicator > 1.0 ? 1.0 : (indicator < 0.0 ? 0.0 : indicator);
                        item.min = new ValueType(0, Multiplier.K, Unit.l);
                        item.max = new ValueType((double)inventory.MaxVolume, Multiplier.K, Unit.l);
                        item.value = new ValueType((double)inventory.CurrentVolume, Multiplier.K, Unit.l);
                        item.name = (inventory.Owner as IMyTerminalBlock).CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Count : DataRetriever
            {
                DataCollectorInventory inv_ = null;
                public Count(DataCollectorInventory inv)
                {
                    inv_ = inv;
                }

                public override double indicator()
                {
                    return inv_.itemRation_;
                }

                public override ValueType min()
                {
                    return new ValueType(0.0);
                }

                public override ValueType max()
                {
                    return new ValueType(inv_.maxItems_);
                }

                public override ValueType value()
                {
                    return new ValueType(inv_.currentItems_);
                }

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(var item in inv_.items_)
                    {
                        double indicator = item.currentAmount / (double)item.maxAmount;

                        ListContainer entry = new ListContainer();
                        entry.onoff = true;
                        entry.indicator = indicator > 1.0 ? 1.0 : (indicator < 0.0 ? 0.0 : indicator);
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
            #endregion // Data Retriever
        }
    }
}
