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
        public class DataCollectorThruster : DataCollector<IMyThrust>
        {
            public DataCollectorThruster(Configuration.Options options, string connector)
                : base("thruster", "", options, connector)
            {
                AcceptBlock = (block) =>
                {
                    // IMyThrust.GridThrustDirection
                    // X => 1 == Left, -1 Right
                    // Y => 1 == Down, -1 Up
                    // Z => 1 == Forward, -1 == Backwards

                    if (referenceController_ != null)
                    {
                        // direction
                        Matrix thrusterMatrix;
                        block.Orientation.GetMatrix(out thrusterMatrix);
                        var directionVector = Vector3D.Rotate(thrusterMatrix.Forward, referenceControllerMatrix_);
                        var orientation = Base6Directions.GetDirection(directionVector);

                        if (sameThrusterDirection(orientation) && sameThrusterType(block.DetailedInfo))
                            Blocks.Add(block);
                    }
                };
            }

            #region Construction
            int constructionStage_ = 0;
            public override bool construct()
            {
                int cfgIndex = ConnectorName != "" ? 3 : 2;

                Func<string, ThrusterType> getThrusterType = (type) =>
                {
                    // select thruster type
                    switch (type)
                    {
                        case "atmos":
                            return ThrusterType.Atmos;
                        case "ion":
                            return ThrusterType.Ion;
                        case "hydrogen":
                            return ThrusterType.Hydrogen;
                    }

                    return 0;
                };

                Func<string, ThrusterDirection> getThrusterDirection = (direction) =>
                {
                    // select direction filter
                    switch (direction)
                    {
                        case "lift":
                            return ThrusterDirection.Lift;
                        case "lower":
                            return ThrusterDirection.Lower;
                        case "left":
                            return ThrusterDirection.Left;
                        case "right":
                            return ThrusterDirection.Right;
                        case "accelerate":
                            return ThrusterDirection.Accelerate;
                        case "break":
                            return ThrusterDirection.Break;
                    }

                    return 0;
                };

                if (constructionStage_ == 0)
                {
                    // search for ship controller
                    referenceControllerName_ = Options[cfgIndex];
                    App.GridTerminalSystem.GetBlocksOfType<IMyShipController>(null, (scblock) =>
                    {
                        IMyShipController controller = scblock as IMyShipController;
                        if (controller != null && controller.IsSameConstructAs(ReferenceGrid))
                        {
                            if (!controller.ControlThrusters)
                                return false;

                            if (controller.CustomName == referenceControllerName_)
                                referenceController_ = controller;
                            else if (controller.IsMainCockpit)
                                referenceController_ = controller;
                            else if (controller.IsUnderControl)
                                referenceController_ = controller;
                        }

                        return false;
                    });

                    if (referenceController_ == null)
                        log(Console.LogType.Error, "No reference controller");
                    else
                    {
                        referenceController_.Orientation.GetMatrix(out referenceControllerMatrix_);
                        referenceControllerMatrix_ = Matrix.Invert(referenceControllerMatrix_);
                    }

                    constructionStage_ = 1;
                }
                else if (constructionStage_ == 1)
                {
                    // step through filter
                    for (; cfgIndex < Options.Count; ++cfgIndex)
                    {
                        thrusterType_ |= getThrusterType(Options[cfgIndex].ToLower());
                        thrusterDirection_ |= getThrusterDirection(Options[cfgIndex].ToLower());
                    }

                    thrusterType_ = thrusterType_ == 0 ? ThrusterType.All : thrusterType_;
                    thrusterDirection_ = thrusterDirection_ == 0 ? ThrusterDirection.All : thrusterDirection_;

                    return base.construct();
                }

                return true;
            }

            public override bool reconstruct()
            {
                constructionStage_ = 0;
                referenceController_ = null;
                thrusterType_ = 0;
                thrusterDirection_ = 0;
                return base.reconstruct();
            }
            #endregion // Construction

            #region Update
            float thrustCurrent_ = 0.0f;
            float thrustMax_ = 0.0f;
            float thrustRate_ = 0.0f;

            float overrideCurrent_ = 0.0f;
            float overrideRate_ = 0.0f;
            float overrideMax_ = 0.0f;

            public override void prepareUpdate()
            {
                base.prepareUpdate();

                updateIndex = 0;

                thrustMax_ = 0.0f;
                thrustCurrent_ = 0.0f;
                overrideCurrent_ = 0.0f;
                overrideMax_ = 0.0f;
            }

            int updateIndex = 0;
            protected override void update()
            {
                for (;
                    updateIndex < Blocks.Count && App.Runtime.CurrentInstructionCount < App.Runtime.MaxInstructionCount;
                    ++updateIndex)
                {
                    var thruster = Blocks[updateIndex];

                    thrustMax_ += thruster.MaxEffectiveThrust;
                    thrustCurrent_ += thruster.CurrentThrust;

                    overrideCurrent_ += thruster.ThrustOverride;
                    overrideMax_ += thruster.MaxThrust;

                    blocksOn_ += isOn(thruster) ? 1 : 0;
                    blocksFunctional_ += thruster.IsFunctional ? 1 : 0;
                }

                if (updateIndex >= Blocks.Count)
                    UpdateFinished = true;
            }

            public override void finalizeUpdate()
            {
                thrustRate_ = thrustMax_ > 0.0f ? thrustCurrent_ / thrustMax_ : 0.0f;
                overrideRate_ = overrideMax_ > 0.0f ? overrideCurrent_ / overrideMax_ : 0.0f;
                base.finalizeUpdate();
            }
            #endregion // Update

            string referenceControllerName_ = "";
            IMyShipController referenceController_ = null;
            Matrix referenceControllerMatrix_;

            [Flags]
            enum ThrusterDirection
            {
                All = 0xff,
                Lift = 1,
                Lower = 2,
                Left = 4,
                Right = 8,
                Accelerate = 16,
                Break = 32
            }
            ThrusterDirection thrusterDirection_ = 0;

            [Flags]
            enum ThrusterType
            {
                All = 0xff,
                Atmos = 1,
                Ion = 2,
                Hydrogen = 4
            }
            ThrusterType thrusterType_ = 0;

            bool sameThrusterDirection(Base6Directions.Direction orientation) =>
                (orientation == Base6Directions.Direction.Up && ((thrusterDirection_ & ThrusterDirection.Lower) != 0)) ||
                (orientation == Base6Directions.Direction.Down && ((thrusterDirection_ & ThrusterDirection.Lift) != 0)) ||
                (orientation == Base6Directions.Direction.Left && ((thrusterDirection_ & ThrusterDirection.Right) != 0)) ||
                (orientation == Base6Directions.Direction.Right && ((thrusterDirection_ & ThrusterDirection.Left) != 0)) ||
                (orientation == Base6Directions.Direction.Forward && ((thrusterDirection_ & ThrusterDirection.Break) != 0)) ||
                (orientation == Base6Directions.Direction.Backward && ((thrusterDirection_ & ThrusterDirection.Accelerate) != 0));

            static string typePattern_ = @"^Type: [\w\s]*(?<type>Ion|Atmospheric|Hydrogen)[\w\s]*$";
            bool sameThrusterType(string info)
            {
                var regex = new System.Text.RegularExpressions.Regex(typePattern_, System.Text.RegularExpressions.RegexOptions.Multiline);
                var match = regex.Match(info);
                if (match.Success)
                {
                    string type = match.Groups["type"].Value;
                    return
                        type == "Ion" && ((thrusterType_ & ThrusterType.Ion) != 0) ||
                        type == "Hydrogen" && ((thrusterType_ & ThrusterType.Hydrogen) != 0) ||
                        type == "Atmospheric" && ((thrusterType_ & ThrusterType.Atmos) != 0);
                }

                return false;
            }

            public override string getVariable(string data)
            {
                return base.getVariable(data)
                    .Replace("%currentthrust%", new VISUnitType(thrustCurrent_, unit: Unit.Newton).pack())
                    .Replace("%maxthrust%", new VISUnitType(thrustMax_, unit: Unit.Newton).pack())
                    .Replace("%thrustrate%", new VISUnitType(thrustRate_, unit: Unit.Percent).pack())
                    .Replace("%currentoverride%", new VISUnitType(overrideCurrent_, unit: Unit.Newton).pack())
                    .Replace("%maxoverride%", new VISUnitType(overrideMax_, unit: Unit.Newton).pack())
                    .Replace("%overriderate%", new VISUnitType(overrideRate_, unit: Unit.Percent).pack());
            }

            #region Accessors
            public override DataAccessor getDataAccessor(string name)
            {
                switch (name.ToLower())
                {
                    case "thrust":
                        return new Thrust(this);
                    case "override":
                        return new Override(this);
                }

                return base.getDataAccessor(name);
            }

            class Thrust : DataAccessor
            {
                DataCollectorThruster da_;

                public Thrust(DataCollectorThruster da)
                {
                    da_ = da;
                }

                public override double indicator() => da_.thrustRate_;
                public override VISUnitType min() => new VISUnitType(0.0, unit: Unit.Newton);
                public override VISUnitType max() => new VISUnitType(da_.thrustMax_, unit: Unit.Newton);
                public override VISUnitType value() => new VISUnitType(da_.thrustCurrent_, unit: Unit.Newton);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var thruster in da_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = thruster.CustomName;
                        item.indicator = thruster.CurrentThrust / thruster.MaxEffectiveThrust;
                        item.min = new VISUnitType(0, unit: Unit.Newton);
                        item.max = new VISUnitType(thruster.MaxEffectiveThrust, unit: Unit.Newton);
                        item.value = new VISUnitType(thruster.CurrentThrust, unit: Unit.Newton);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Override : DataAccessor
            {
                DataCollectorThruster da_;
                public Override(DataCollectorThruster da)
                {
                    da_ = da;
                }

                public override double indicator() => da_.overrideRate_;
                public override VISUnitType min() => new VISUnitType(0.0, unit: Unit.Newton);
                public override VISUnitType max() => new VISUnitType(da_.overrideMax_, unit: Unit.Newton);
                public override VISUnitType value() => new VISUnitType(da_.overrideCurrent_, unit: Unit.Newton);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var thruster in da_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = thruster.CustomName;
                        item.indicator = thruster.ThrustOverridePercentage;
                        item.min = new VISUnitType(0, unit: Unit.Newton);
                        item.max = new VISUnitType(thruster.MaxThrust, unit: Unit.Newton);
                        item.value = new VISUnitType(thruster.ThrustOverride, unit: Unit.Newton);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Accessors
        }
    }
}
