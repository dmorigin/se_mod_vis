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

            string pattern = "^(MyObjectBuilder\\_[Ingot|Ore|PhysicalGunObject|Component|AmmoMagazine])\\/([0-9a-zA-Z]+)$";
            void addInventoryBlock(IMyTerminalBlock block)
            {
                long maxVolume = 0;
                if (block.InventoryCount > 0)
                {
                    for (int i = 0; i < block.InventoryCount; i++)
                    {
                        IMyInventory inventory = block.GetInventory(i);
                        bool accepted = true;

                        if (itemTypes_.Count > 0)
                        {
                            List<MyItemType> itemTypes = new List<MyItemType>();
                            inventory.GetAcceptedItems(itemTypes);
                            foreach (ItemType item in itemTypes_)
                            {
                                if (!itemTypes.Exists(x => x == item.type))
                                {
                                    accepted = false;
                                    break;
                                }
                            }
                        }

                        if (accepted)
                        {
                            inventories_.Add(inventory);
                            maxVolume += inventory.MaxVolume.RawValue;
                            Blocks.Add(block);
                        }
                    }
                }

                maxVolume_ = maxVolume;
            }

            public override bool construct()
            {
                BlockName = Options[0];
                IsGroup = Options.getAsBoolean(1, false);
                long maxItems = 0;

                // process item type
                if (Options.Count > 2)
                {
                    for (int it = 2; it < Options.Count; ++it)
                    {
                        string itemType = Options[it];
                        switch (itemType.ToLower())
                        {
                            case "ammo":
                                itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_AmmoMagazine", "")));
                                continue;
                            case "components":
                                itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Component", "")));
                                continue;
                            case "handtools":
                                itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_PhysicalGunObject", "")));
                                continue;
                            case "ore":
                                itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Ore", "")));
                                continue;
                            case "ingots":
                                itemTypes_.Add(new ItemType(new MyItemType("MyObjectBuilder_Ingot", "")));
                                continue;
                        }

                        int amount = 0;
                        if (System.Text.RegularExpressions.Regex.IsMatch(itemType, pattern))
                        {
                            string[] parts = itemType.Split('/');
                            itemTypes_.Add(new ItemType(new MyItemType(parts[0], parts[1])));
                        }
                        else if (int.TryParse(itemType, out amount))
                        {
                            itemTypes_[itemTypes_.Count - 1].amount = amount;
                            maxItems += amount;
                        }
                        else
                        {
                            log(Console.LogType.Error, $"Invalid item type {itemType}");
                            return false;
                        }
                    }
                }


                if (BlockName != "")
                {
                    getBlocks<IMyTerminalBlock>(BlockName, IsGroup, (block) =>
                    {
                        addInventoryBlock(block);
                    }, TypeID);
                }
                else
                {
                    if (!getBlocks<IMyTerminalBlock>((block) =>
                    {
                        addInventoryBlock(block);
                    }, TypeID))
                    {
                        log(Console.LogType.Error, $"Failed to find blocks of type {TypeID}");
                        return false;
                    }
                }

                update();
                maxItems_ = maxItems;
                Constructed = true;
                return true;
            }

            public override bool reconstruct()
            {
                itemTypes_.Clear();
                inventories_.Clear();
                return base.reconstruct();
            }

            protected override void update()
            {
                long currentVolume = 0;
                long currentItems = 0;
                items_.Clear();

                foreach(IMyInventory inventory in inventories_)
                {
                    currentVolume += inventory.CurrentVolume.RawValue;

                    List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();
                    foreach (MyInventoryItem inventoryItem in inventoryItems)
                    {
                        int index = items_.FindIndex(x => x.type == inventoryItem.Type);
                        if (index >= 0)
                        {
                            items_[index].currentAmount += inventoryItem.Amount.RawValue;
                            currentItems += inventoryItem.Amount.RawValue;
                        }
                        else
                        {
                            InventoryItem item = new InventoryItem();
                            item.currentAmount = inventoryItem.Amount.RawValue;
                            item.maxAmount = itemTypes_.Find(x => x.type == inventoryItem.Type).amount;
                            item.type = inventoryItem.Type;
                        }
                    }
                }

                currentVolume_ = currentVolume;
                currentItems_ = currentItems;
                volumeRation_ = currentVolume_ / maxVolume_;
                itemRation_ = currentItems_ / maxItems_;
                itemRation_ = itemRation_ > 1 ? 1 : itemRation_;
            }

            public override string CollectorTypeName
            {
                get { return "inventory"; }
            }

            class ItemType
            {
                public ItemType(MyItemType itemType)
                {
                    type = itemType;
                    amount = 0;
                }

                public MyItemType type;
                public int amount;
            }
            List<ItemType> itemTypes_ = new List<ItemType>();
            List<IMyInventory> inventories_ = new List<IMyInventory>();
            double currentVolume_ = 0;
            double maxVolume_ = 0;
            double volumeRation_ = 0;
            double maxItems_ = 0;
            double currentItems_ = 0;
            double itemRation_ = 0;

            class InventoryItem
            {
                public long currentAmount;
                public long maxAmount;
                public MyItemType type;
            }
            List<InventoryItem> items_ = new List<InventoryItem>();


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

                public override double getIndicator()
                {
                    return inv_.volumeRation_;
                }

                public override ValueType getMin()
                {
                    return new ValueType(0, unit:Unit.l);
                }

                public override ValueType getMax()
                {
                    return new ValueType(inv_.maxVolume_, unit: Unit.l);
                }

                public override ValueType getValue()
                {
                    return new ValueType(inv_.currentVolume_, unit: Unit.l);
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();

                    // list of all inventories
                    foreach (var inventory in inv_.inventories_)
                    {
                        ListContainer item = new ListContainer();
                        item.onoff = true;
                        item.indicator = inventory.CurrentVolume.RawValue / inventory.MaxVolume.RawValue;
                        item.min = new ValueType(0, unit:Unit.l);
                        item.max = new ValueType(inventory.MaxVolume.RawValue, unit:Unit.l);
                        item.value = new ValueType(inventory.CurrentVolume.RawValue, unit: Unit.l);
                        item.name = (inventory.Owner as IMyTerminalBlock).CustomName;
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

                public override double getIndicator()
                {
                    return inv_.itemRation_;
                }

                public override ValueType getMin()
                {
                    return new ValueType(0);
                }

                public override ValueType getMax()
                {
                    return new ValueType(inv_.maxItems_);
                }

                public override ValueType getValue()
                {
                    return new ValueType(inv_.currentItems_);
                }

                public override void getList(out List<ListContainer> container)
                {
                    container = new List<ListContainer>();
                    foreach(var item in inv_.items_)
                    {
                        ListContainer entry = new ListContainer();
                        double indicator = item.currentAmount / item.maxAmount;

                        entry.onoff = true;
                        entry.indicator = indicator > 1 ? 1 : indicator;
                        entry.min = new ValueType(0);
                        entry.max = new ValueType(item.maxAmount);
                        entry.value = new ValueType(item.currentAmount);
                        entry.name = item.type.SubtypeId;
                        entry.type = item.type;
                        container.Add(entry);
                    }
                }
            }
        }
    }
}
