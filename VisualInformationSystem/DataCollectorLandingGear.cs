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
        public class DataCollectorLandingGear : DataCollector<IMyLandingGear>
        {
            public DataCollectorLandingGear(Configuration.Options options, string connector)
                : base("landinggear", "", options, connector)
            {
            }

            protected override void update()
            {
                locked_ = 0;
                int ratio = 0;

                foreach(var gear in Blocks)
                {
                    blocksOn_ += isOn(gear) ? 1 : 0;
                    blocksFunctional_ += gear.IsFunctional ? 1 : 0;
                    locked_ += gear.IsLocked ? 1 : 0;

                    ratio += gear.LockMode == LandingGearMode.Locked ? 2 : (gear.LockMode == LandingGearMode.ReadyToLock ? 1 : 0);
                }

                ratio_ = (ratio * 0.5f) / Blocks.Count;
                UpdateFinished = true;
            }

            int locked_ = 0;
            float ratio_ = 0f;

            public override string getVariable(string data)
            {
                return base.getVariable(data)
                    .Replace("%locked%", locked_.ToString())
                    .Replace("%unlocked%", (Blocks.Count - locked_).ToString())
                    .Replace("%ratio%", new VISUnitType(locked_ / (double)Blocks.Count, unit: Unit.Percent));
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                if (name.ToLower() == "status")
                    return new Status(this);
                return base.getDataAccessor(name);
            }

            class Status : DataAccessor
            {
                DataCollectorLandingGear dc_;
                public Status(DataCollectorLandingGear dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.ratio_;
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.Blocks.Count * dc_.ratio_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var gear in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = gear.CustomName;
                        item.indicator = gear.LockMode == LandingGearMode.Locked ? 1 : (gear.LockMode == LandingGearMode.ReadyToLock ? 0.5 : 0);
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);
                        item.value = new VISUnitType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
