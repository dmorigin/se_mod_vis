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
        public class GraphicSlider : Graphic
        {
            public GraphicSlider(ContentContainer template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name) => true;

            public override void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                if (DataAccessor == null || !isVisible(DataAccessor.indicator()))
                    return;

                if (Gradient.Count == 0)
                    addGradientColor(0f, Default.BarColor);

                //Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                //Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                base.render(display, rt, addSprite);

                // draw slider
                renderSlider(addSprite, rt, RenderData.Position, RenderData.InnerSize, DataAccessor.min() < 0.0,
                    (float)DataAccessor.indicator(), Gradient, sliderOrientation_, sliderWidth_, sliderColor_);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var config = base.getConfigHandler();
                config.add("setslider", configSetSlider);

                return config;
            }

            SliderOrientation sliderOrientation_ = SliderOrientation.Top;
            float sliderWidth_ = 0.03f;
            Color sliderColor_ = Color.WhiteSmoke;
            bool configSetSlider(string key, string value, Configuration.Options options)
            {
                switch (value.ToLower())
                {
                    case "top":
                    case "t":
                        sliderOrientation_ = SliderOrientation.Top;
                        break;
                    case "left":
                    case "l":
                        sliderOrientation_ = SliderOrientation.Left;
                        break;
                    case "bottom":
                    case "b":
                        sliderOrientation_ = SliderOrientation.Bottom;
                        break;
                    case "right":
                    case "r":
                        sliderOrientation_ = SliderOrientation.Right;
                        break;
                    default:
                        log(Console.LogType.Error, $"Invalid slider orientation '{value}'");
                        return false;
                }

                sliderWidth_ = options.asFloat(0, 0.03f);
                sliderColor_ = options.asColor(1, Color.WhiteSmoke);
                return true;
            }
            #endregion // Configuration
        }
    }
}
