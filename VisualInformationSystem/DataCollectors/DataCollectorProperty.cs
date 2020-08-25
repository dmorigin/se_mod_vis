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
        public class DataCollectorProperty : DataCollectorBase<IMyTerminalBlock>
        {
            public DataCollectorProperty(string collectorTypeName, string typeId, Configuration.Options options, string connector)
                : base(collectorTypeName, typeId, options, connector)
            {
                AcceptBlock = (block) =>
                {
                    float min, max;
                    if (getMinMax(block, out min, out max))
                    {
                        valueMin_ += min;
                        valueMax_ += max;
                        Blocks.Add(block);
                    }
                };
            }

            public override bool construct()
            {
                propertyName_ = Options[ConnectorName != "" ? 3 : 2];
                if (propertyName_ == "")
                    propertyName_ = CollectorTypeName;

                valueMax_ = 0f;
                valueMin_ = 0f;

                return base.construct();
            }

            public override void finalizeUpdate()
            {
                base.finalizeUpdate();
                ratio_ = clamp(valueCurrent_ / (valueMax_ - valueMin_), -1f, 1f);
            }

            protected override void update()
            {
                valueCurrent_ = 0f;
                foreach (IMyTerminalBlock block in Blocks)
                {
                    valueCurrent_ += getValue(block);
                    blocksOn_ += isOn(block) ? 1 : 0;
                    blocksFunctional_ += block.IsFunctional ? 1 : 0;
                }

                UpdateFinished = true;
            }

            string propertyName_ = "";
            PropertyType type_ = PropertyType.Single;
            float valueMin_ = 0f;
            float valueMax_ = 0f;
            float valueCurrent_ = 0f;
            float ratio_ = 0f;

            enum PropertyType
            {
                Single,
                Int64,
                Bool
            }

            float getValue(IMyTerminalBlock block)
            {
                switch (type_)
                {
                    case PropertyType.Bool:
                        return block.GetValue<bool>(propertyName_) ? 1f : 0f;
                    case PropertyType.Single:
                        return block.GetValue<Single>(propertyName_);
                    case PropertyType.Int64:
                        return block.GetValue<Int64>(propertyName_);
                }

                return 0f;
            }

            bool getMinMax(IMyTerminalBlock block, out float min, out float max)
            {
                var property = block.GetProperty(propertyName_);
                if (property != null)
                {
                    switch (property.TypeName)
                    {
                        case "Single":
                            min = block.GetMinimum<Single>(propertyName_);
                            max = block.GetMaximum<Single>(propertyName_);
                            type_ = PropertyType.Single;
                            return true;
                        case "Int64":
                            min = block.GetMinimum<Int64>(propertyName_);
                            max = block.GetMaximum<Int64>(propertyName_);
                            type_ = PropertyType.Int64;
                            return true;
                        case "Boolean":
                            min = 0f;
                            max = 1f;
                            type_ = PropertyType.Bool;
                            return true;
                    }
                }

                min = 0f;
                max = 0f;
                return false;
            }

            #region Data Accessors
            public override DataAccessor getDataAccessor(string name)
            {
                if (name.ToLower() == "value")
                    return new Value(this);

                return base.getDataAccessor(name);
            }

            class Value : DataAccessor
            {
                DataCollectorProperty dc_;
                public Value(DataCollectorProperty dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.ratio_;
                public override VISUnitType min() => new VISUnitType(dc_.valueMin_);
                public override VISUnitType max() => new VISUnitType(dc_.valueMax_);
                public override VISUnitType value() => new VISUnitType(dc_.valueCurrent_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (IMyTerminalBlock block in dc_.Blocks)
                    {
                        float min, max;
                        dc_.getMinMax(block, out min, out max);

                        float value = dc_.getValue(block);

                        ListContainer item = new ListContainer();
                        item.name = block.CustomName;
                        item.indicator = clamp(value / (max - min), -1f, 1f);
                        item.value = new VISUnitType(value);
                        item.min = new VISUnitType(min);
                        item.max = new VISUnitType(max);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessors
        }
    }
}
