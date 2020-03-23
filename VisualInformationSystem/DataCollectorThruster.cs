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
                        /*
                        Vector3D myself = block.WorldMatrix.Up;
                        myself = Vector3D.Rotate(myself, Matrix.Invert(referenceController_.WorldMatrix));
                        var orientation = Base6Directions.GetDirection(myself);
                        */

                        /*
                        Matrix matrix;
                        block.Orientation.GetMatrix(out matrix);
                        var orientation = Base6Directions.GetDirection((matrix * referenceControllerMatrix_).
                            GetDirectionVector(referenceController_.Orientation.Forward));
                        */

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

                    return 0; //ThrusterType.All;
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

                    return 0; //ThrusterDirection.All;
                };

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
                    //referenceControllerMatrix_ = Matrix.Transpose(referenceControllerMatrix_);
                }

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
            #endregion // Construction

            #region Update
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
        }
    }
}
