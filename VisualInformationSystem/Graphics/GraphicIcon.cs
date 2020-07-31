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
        public class GraphicIcon : Graphic
        {
            public GraphicIcon(ContentContainer template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name) => true;

            bool toggleShow_ = true; // render data only
            public override void prepareRendering(Display display)
            {
                base.prepareRendering(display);
                toggleShow_ = !toggleShow_;
            }

            public override void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Color color = Color;
                bool visible = true;
                if (DataAccessor != null)
                {
                    visible = isVisible(DataAccessor.indicator());
                    getGradientColorLerp((float)DataAccessor.indicator(), Gradient, out color);
                }

                if (!visible || (!toggleShow_ && blink_))
                    return;

                //Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                base.render(display, rt, addSprite);

                icon_(addSprite, rt, iconName_,
                    //PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position,
                    RenderData.Position,
                    RenderData.InnerSize,
                    thicknessSizeType_ == ValueType.Relative ? thickness_ * RenderData.InnerSize.X : thickness_,
                    rotation_, color);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var config = base.getConfigHandler();
                config.add("icon", configIcon);
                config.add("blink", configBlink);
                config.add("rotation", configRotation);
                config.add("thickness", configThickness);

                return config;
            }

            string iconName_ = "";
            Icon.Render icon_ = (addSprite, rt, name, position, size, thickness, rotation, color) => { };
            bool configIcon(string key, string value, Configuration.Options options)
            {
                iconName_ = value;
                icon_ = Icon.getIcon(value);
                if (icon_ == null)
                {
                    log(Console.LogType.Error, $"Invalid icon name:{value}");
                    return false;
                }
                return true;
            }

            bool blink_ = false;
            bool configBlink(string key, string value, Configuration.Options options)
            {
                blink_ = Configuration.asBoolean(value, false);
                return true;
            }

            float rotation_ = 0f;
            bool configRotation(string key, string value, Configuration.Options options)
            {
                // 360° == 2*pi
                rotation_ = (float)((Configuration.asFloat(value, 0f) / 180f) * Math.PI);
                return true;
            }

            float thickness_ = Default.Thickness;
            ValueType thicknessSizeType_ = Default.SizeType;
            bool configThickness(string key, string value, Configuration.Options options)
            {
                thickness_ = Configuration.asFloat(value, Default.Thickness);
                if (options.Count > 0)
                    toValueType(options[0], out thicknessSizeType_, Default.SizeType);
                return true;
            }
            #endregion // Configuration
        }
    }
}
