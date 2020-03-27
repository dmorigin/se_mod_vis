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
        public class DataCollectorProduction<BlockType> : DataCollector<BlockType> where BlockType: class
        {
            public DataCollectorProduction(string collectorTypeName, string typeId, Unit unit, Configuration.Options options, string connector)
                : base(collectorTypeName, typeId, options, connector)
            {
                unit_ = unit;
            }

            static List<KeyValuePair<string, string>> ItemTypeLookupMap = new List<KeyValuePair<string, string>>();
            static bool LookupMapDone = false;

            public override bool construct()
            {
                if (!LookupMapDone)
                {
                    RenderTarget.getSprites($"^{Default.MyObjectBuilder}_(?<id>.+)/(?<sub>.+)$", (match) =>
                    {
                        if (match.Groups["id"].Value != "Ore" && match.Groups["id"].Value != "Ingot")
                            ItemTypeLookupMap.Add(new KeyValuePair<string, string>(match.Groups["sub"].Value, match.Value));
                    });
                    LookupMapDone = true;
                }

                bool ret = base.construct();

                foreach (IMyProductionBlock block in Blocks)
                {
                    if (!currentPerBlock_.ContainsKey(block.EntityId))
                    {
                        currentPerBlock_.Add(block.EntityId, 0);
                        maxPerBlock_.Add(block.EntityId, 0);
                    }
                }

                return ret;
            }

            #region Update Process
            int blockIndex_ = 0;

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                blockIndex_ = 0;
                currentQueuedItems_ = 0;

                // clear current amount
                foreach (var item in items_)
                    item.CurrentAmount = 0;
            }

            public override void finalizeUpdate()
            {
                maxQueuedItems_ = currentQueuedItems_ == 0 ? 0 : Math.Max(maxQueuedItems_, currentQueuedItems_);
                ratio_ = maxQueuedItems_ == 0 ? 0 : (float)currentQueuedItems_ / (float)maxQueuedItems_;

                // remove finished items
                items_.RemoveAll((item) => item.CurrentAmount == 0);

                // rebuild max amount
                foreach (var item in items_)
                    item.MaxAmount = Math.Max(item.CurrentAmount, item.MaxAmount);

                base.finalizeUpdate();
            }

            protected override void update()
            {
                for (;
                    blockIndex_ < Blocks.Count && 
                    App.Runtime.CurrentInstructionCount < Default.MaxInstructionCount;
                    blockIndex_++)
                {
                    IMyProductionBlock block = Blocks[blockIndex_] as IMyProductionBlock;

                    blocksOn_ += isOn(block) ? 1 : 0;
                    blocksFunctional_ += block.IsFunctional ? 1 : 0;

                    // get list of all items in the queue
                    List<MyProductionItem> queue = new List<MyProductionItem>();
                    block.GetQueue(queue);

                    long currentAmount = 0;

                    // rebuild current amount with current production state
                    foreach(var item in queue)
                    {
                        currentAmount += (long)item.Amount;
                        currentQueuedItems_ += (long)item.Amount;
                        int index = items_.FindIndex((entry) => entry.BlueprintId == item.BlueprintId.ToString());
                        if (index >= 0)
                            items_[index].CurrentAmount += (long)item.Amount;
                        else
                            items_.Add(new ProductionItem(item));
                    }

                    currentPerBlock_[block.EntityId] = currentAmount;
                    maxPerBlock_[block.EntityId] = currentAmount == 0 ? 0 : Math.Max(maxPerBlock_[block.EntityId], currentAmount);
                }

                UpdateFinished = blockIndex_ >= Blocks.Count;
            }
            #endregion // Update Process

            long maxQueuedItems_ = 0;
            long currentQueuedItems_ = 0;
            float ratio_ = 0f;
            Unit unit_ = Unit.None;

            List<ProductionItem> items_ = new List<ProductionItem>();
            Dictionary<long, long> currentPerBlock_ = new Dictionary<long, long>();
            Dictionary<long, long> maxPerBlock_ = new Dictionary<long, long>();

            class ProductionItem
            {
                public ProductionItem(MyProductionItem item)
                {
                    Type = convertToItemType(item.BlueprintId);
                    BlueprintId = item.BlueprintId.ToString();
                    MaxAmount = (long)item.Amount;
                    CurrentAmount = (long)item.Amount;
                }

                public long MaxAmount
                {
                    get;
                    set;
                }

                public long CurrentAmount
                {
                    get;
                    set;
                }

                public string BlueprintId
                {
                    get;
                    private set;
                }

                public VISItemType Type
                {
                    get;
                    private set;
                }

                static string isOrePattern = @"^(?<id>.+)OreToIngot$";
                VISItemType convertToItemType(MyDefinitionId item)
                {
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(isOrePattern);
                    var match = regex.Match(item.SubtypeName);
                    if (match.Success)
                        return $"{Default.MyObjectBuilder}_Ingot/{match.Groups["id"].Value}";

                    string subtype = removeFromEnd(item.SubtypeName, "_Blueprint");
                    subtype = removeFromEnd(subtype, "Magazine");

                    int index = ItemTypeLookupMap.FindIndex((pair) => pair.Key == subtype);
                    if (index >= 0)
                        return ItemTypeLookupMap[index].Value;

                    return $"{item.TypeId.ToString()}/{subtype}";
                }
            }

            public override string getVariable(string data)
            {
                return base.getVariable(data)
                    .Replace("%maxamount%", new VISUnitType(maxQueuedItems_, unit: unit_).pack().ToString())
                    .Replace("%currentamount%", new VISUnitType(currentQueuedItems_, unit: unit_).pack().ToString())
                    .Replace("%ratio%", new VISUnitType(ratio_, unit: Unit.Percent));
            }

            #region Data Accessors
            public override DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "items":
                        return new Items(this);
                    case "overview":
                        return new Overview(this);
                }

                return base.getDataAccessor(name);
            }

            class Items : DataAccessor
            {
                DataCollectorProduction<BlockType> dc_;
                public Items(DataCollectorProduction<BlockType> dc)
                {
                    dc_ = dc;
                }

                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.maxQueuedItems_, unit: dc_.unit_);
                public override VISUnitType value() => new VISUnitType(dc_.currentQueuedItems_, unit: dc_.unit_);
                public override double indicator() => dc_.ratio_;

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var item in dc_.items_)
                    {
                        ListContainer entry = new ListContainer();
                        entry.indicator = clamp((double)item.CurrentAmount / (double)item.MaxAmount);
                        entry.min = new VISUnitType(0);
                        entry.max = new VISUnitType(item.MaxAmount, unit: dc_.unit_);
                        entry.value = new VISUnitType(item.CurrentAmount, unit: dc_.unit_);
                        entry.name = item.Type.Type.SubtypeId;
                        entry.type = item.Type;

                        if (filter == null || (filter != null && filter(entry)))
                            container.Add(entry);
                    }
                }
            }

            class Overview : DataAccessor
            {
                DataCollectorProduction<BlockType> dc_;
                public Overview(DataCollectorProduction<BlockType> dc)
                {
                    dc_ = dc;
                }

                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.maxQueuedItems_, unit: dc_.unit_);
                public override VISUnitType value() => new VISUnitType(dc_.currentQueuedItems_, unit: dc_.unit_);
                public override double indicator() => dc_.ratio_;

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(IMyProductionBlock block in dc_.Blocks)
                    {
                        long max = dc_.maxPerBlock_[block.EntityId];
                        long cur = dc_.currentPerBlock_[block.EntityId];

                        ListContainer entry = new ListContainer();
                        entry.indicator = clamp((double)cur / (double)max);
                        entry.min = new VISUnitType(0);
                        entry.max = new VISUnitType(max);
                        entry.value = new VISUnitType(cur);
                        entry.name = block.CustomName;

                        if (filter == null || (filter != null && filter(entry)))
                            container.Add(entry);
                    }
                }
            }
            #endregion // Data Accessors
        }
    }
}
