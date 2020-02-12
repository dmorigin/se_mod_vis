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
        public class DataCollectorPiston : DataCollector<IMyPistonBase>
        {
            public DataCollectorPiston(Configuration.Options options, string connector)
                : base("piston", "", options, connector)
            {
            }

            protected override void update()
            {
                minPosition_ = 0f;
                maxPosition_ = 0f;
                currentPosition_ = 0f;
                extending_ = 0;
                retracting_ = 0;

                foreach (var piston in Blocks)
                {
                    blocksOn_ += isOn(piston) ? 1 : 0;

                    minPosition_ += piston.MinLimit;
                    maxPosition_ += piston.MaxLimit;
                    currentPosition_ += piston.CurrentPosition;

                    extending_ += piston.Status == PistonStatus.Extending ? 1 : 0;
                    retracting_ += piston.Status == PistonStatus.Retracting ? 1 : 0;
                }

                ratioPosition_ = maxPosition_ == 0f ? 0f : currentPosition_ / (maxPosition_ - minPosition_);
                UpdateFinished = true;
            }

            float minPosition_ = 0f;
            float maxPosition_ = 10f;
            float currentPosition_ = 0f;
            float ratioPosition_ = 0f;

            int extending_ = 0;
            int retracting_ = 0;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%minpos%", new VISUnitType(minPosition_, unit: Unit.Meter))
                    .Replace("%maxpos%", new VISUnitType(maxPosition_, unit: Unit.Meter))
                    .Replace("%currentpos%", new VISUnitType(currentPosition_, unit: Unit.Meter))
                    .Replace("%ratiopos%", new VISUnitType(ratioPosition_, unit: Unit.Percent))
                    .Replace("%extending%", extending_.ToString())
                    .Replace("%retracting%", retracting_.ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "position":
                        return new Position(this);
                    case "extending":
                        return new Extending(this);
                    case "retracting":
                        return new Retracting(this);
                }
                return base.getDataAccessor(name);
            }

            class Position : DataAccessor
            {
                DataCollectorPiston dc_;
                public Position(DataCollectorPiston dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.ratioPosition_;
                public override VISUnitType min() => new VISUnitType(dc_.minPosition_, unit: Unit.Meter);
                public override VISUnitType max() => new VISUnitType(dc_.maxPosition_, unit: Unit.Meter);
                public override VISUnitType value() => new VISUnitType(dc_.currentPosition_, unit: Unit.Meter);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach(var piston in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.indicator = piston.CurrentPosition / (piston.MaxLimit - piston.MinLimit);
                        item.min = new VISUnitType(piston.MinLimit, unit: Unit.Meter);
                        item.max = new VISUnitType(piston.MaxLimit, unit: Unit.Meter);
                        item.value = new VISUnitType(piston.CurrentPosition, unit: Unit.Meter);
                        item.name = piston.CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Extending : DataAccessor
            {
                DataCollectorPiston dc_;
                public Extending(DataCollectorPiston dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.extending_ / dc_.Blocks.Count;
                public override VISUnitType min() => new VISUnitType(0.0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.extending_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var piston in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.indicator = piston.Status == PistonStatus.Extending ? 1.0 : 0.0;
                        item.min = new VISUnitType(0.0);
                        item.max = new VISUnitType(1.0);
                        item.value = new VISUnitType(item.indicator);
                        item.name = piston.CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Retracting : DataAccessor
            {
                DataCollectorPiston dc_;
                public Retracting(DataCollectorPiston dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.retracting_ / dc_.Blocks.Count;
                public override VISUnitType min() => new VISUnitType(0.0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.retracting_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var piston in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.indicator = piston.Status == PistonStatus.Retracting ? 1.0 : 0.0;
                        item.min = new VISUnitType(0.0);
                        item.max = new VISUnitType(1.0);
                        item.value = new VISUnitType(item.indicator);
                        item.name = piston.CustomName;

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
