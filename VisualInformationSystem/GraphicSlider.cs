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
            public GraphicSlider(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            public override bool construct()
            {
                return base.construct();
            }

            public override Graphic clone()
            {
                GraphicSlider gfx = new GraphicSlider(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataAccessor = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;
                gfx.VisibleThreshold = VisibleThreshold;
                gfx.VisibleOperator = VisibleOperator;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.sliderColor_ = sliderColor_;
                gfx.sliderOrientation_ = sliderOrientation_;
                gfx.sliderWidth_ = sliderWidth_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                if (DataAccessor == null)
                    return;

                if (Gradient.Count == 0)
                    addGradientColor(0f, Default.BarColor);

                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                // draw slider
                renderSlider(addSprite, rt, position, size, DataAccessor.min() < 0.0, (float)DataAccessor.indicator(),
                    Gradient, sliderOrientation_, sliderWidth_, sliderColor_);
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
