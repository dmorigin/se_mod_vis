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

                speedMax_ = 0f;
                speedCurrent_ = 0f;
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

                speedMax_ = 100.0f; // ToDo: Find a more generic way to support speed mods
                speedCurrent_ = (float)referenceController_.GetShipSpeed();
                speedRatio_ = clamp(speedCurrent_ / speedMax_);

                pGravity_ = referenceController_.GetNaturalGravity().Length();
                aGravity_ = referenceController_.GetArtificialGravity().Length();

                MyShipMass mass = referenceController_.CalculateShipMass();
                massTotal_ = mass.TotalMass;
                massShip_ = mass.BaseMass;
                massInventory_ = massTotal_ - massShip_;
                massRatio_ = clamp(massInventory_ / (massShip_ * 1.5f)); // ToDo: find a more accurate way

                //var velocities = referenceController_.GetShipVelocities();

                UpdateFinished = true;
            }

            IMyShipController referenceController_ = null;
            IMyShipController mainController_ = null;

            float speedMax_ = 0f;
            float speedCurrent_ = 0f;
            float speedRatio_ = 0f;
            double pGravity_ = 0.0;
            double aGravity_ = 0.0;
            float massTotal_ = 0f;
            float massShip_ = 0f;
            float massInventory_ = 0f;
            float massRatio_ = 0f;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%maxspeed%", new VISUnitType(speedMax_, unit: Unit.Speed).pack())
                    .Replace("%currentspeed%", new VISUnitType(speedCurrent_, unit: Unit.Speed).pack())
                    .Replace("%speedratio%", new VISUnitType(speedRatio_, unit: Unit.Percent))
                    .Replace("%shipmass%", new VISUnitType(massShip_, Multiplier.K ,Unit.Gram).pack())
                    .Replace("%totalmass%", new VISUnitType(massTotal_, Multiplier.K, Unit.Gram).pack())
                    .Replace("%inventorymass%", new VISUnitType(massInventory_, Multiplier.K, Unit.Gram).pack())
                    .Replace("%massratio%", new VISUnitType(massRatio_, Multiplier.K, Unit.Gram).pack())
                    .Replace("%pgravity%", new VISUnitType(pGravity_, unit: Unit.Gravity).pack())
                    .Replace("%agravity%", new VISUnitType(aGravity_, unit: Unit.Gravity));
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "speed":
                        return new Speed(this);
                    case "mass":
                        return new Mass(this);
                }

                return base.getDataAccessor(name);
            }


            class Speed : DataAccessor
            {
                DataCollectorShipController dc_;
                public Speed(DataCollectorShipController dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => dc_.speedRatio_;
                public override VISUnitType min() => new VISUnitType(0, unit: Unit.Speed);
                public override VISUnitType max() => new VISUnitType(dc_.speedMax_, unit: Unit.Speed);
                public override VISUnitType value() => new VISUnitType(dc_.speedCurrent_, unit: Unit.Speed);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                }
            }

            class Mass : DataAccessor
            {
                DataCollectorShipController dc_;
                public Mass(DataCollectorShipController dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => 0;
                public override VISUnitType min() => new VISUnitType(dc_.massShip_, Multiplier.K, Unit.Gram);
                public override VISUnitType max() => new VISUnitType(dc_.massShip_ * 1.5, Multiplier.K, Unit.Gram);
                public override VISUnitType value() => new VISUnitType(dc_.massTotal_, Multiplier.K, Unit.Gram);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                }
            }
            #endregion // Data Accessor
        }
    }
}
