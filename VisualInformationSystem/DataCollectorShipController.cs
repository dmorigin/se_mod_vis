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
        public class DataCollectorShipController : DataCollector<IMyShipController>
        {
            public DataCollectorShipController(Configuration.Options options)
                : base("shipcontroller", "", options)
            {
            }

            public override bool construct()
            {
                AcceptBlock = (block) =>
                {
                    if (block.IsMainCockpit)
                        mainController_ = block;
                    Blocks.Add(block);
                };

                return base.construct();
            }

            public override void prepareUpdate()
            {
                base.prepareUpdate();
                referenceController_ = null;
                mainController_ = null;
            }

            protected override void update()
            {
                if (Blocks.Count <= 0)
                    return;

                referenceController_ = Blocks[0];

                foreach(var controller in Blocks)
                {
                    blocksOn_ += isOn(controller) ? 1 : 0;

                    if (controller.CanControlShip && controller.CanControlShip)
                        referenceController_ = controller;
                }

                UpdateFinished = true;
            }

            IMyShipController referenceController_ = null;
            IMyShipController mainController_ = null;

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                return base.getDataAccessor(name);
            }
            #endregion // Data Accessor
        }
    }
}
