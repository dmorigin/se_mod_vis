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
            public GraphicIcon(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            public override bool construct()
            {
                return base.construct();
            }

            public override Graphic clone()
            {
                GraphicIcon gfx = new GraphicIcon(Template, Options);
                gfx.construct();

                gfx.DataCollector = DataCollector;
                gfx.DataRetriever = DataRetriever;
                gfx.DataRetrieverName = DataRetrieverName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.iconName_ = iconName_;
                gfx.icon_ = icon_;
                gfx.showOn_ = showOn_;
                gfx.thresholdOnMin_ = thresholdOnMin_;
                gfx.thresholdOnMax_ = thresholdOnMax_;
                gfx.blink_ = blink_;
                gfx.blinkToggle_ = blinkToggle_;

                return gfx;
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                if (DataRetriever != null)
                    if (showOn_ == true && (DataRetriever.indicator() < thresholdOnMin_ && DataRetriever.indicator() > thresholdOnMax_))
                        return;

                if (blink_ && blinkToggle_)
                    return;
                blinkToggle_ = !blinkToggle_;

                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                icon_(addSprite, iconName_, position, size, rotation_, Color);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var config = base.getConfigHandler();
                config.add("icon", configIcon);
                config.add("showon", configShowOn);
                config.add("blink", configBlink);

                return config;
            }

            string iconName_ = "";
            Icon.Render icon_ = null;
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

            bool showOn_ = false;
            float thresholdOnMin_ = -1f;
            float thresholdOnMax_ = 1f;
            bool configShowOn(string key, string value, Configuration.Options options)
            {
                thresholdOnMin_ = Configuration.asFloat(value, -1f);
                thresholdOnMax_ = options.asFloat(0, 1f);
                showOn_ = true;
                return true;
            }

            bool blink_ = false;
            bool blinkToggle_ = true;
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
            #endregion // Configuration
        }
    }
}
