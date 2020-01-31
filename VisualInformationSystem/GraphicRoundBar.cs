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
        public class GraphicRoundBar : Graphic
        {
            public GraphicRoundBar(Template template, Configuration.Options options)
                : base(template, options)
            {
                log(Console.LogType.Warning, "Round bar graphic is beta");
            }

            public override Graphic clone()
            {
                GraphicRoundBar gfx = new GraphicRoundBar(Template, Options);

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

                gfx.minDegree_ = minDegree_;
                gfx.maxDegree_ = maxDegree_;
                gfx.colorLerp_ = colorLerp_;
                gfx.steps_ = steps_;
                gfx.thickness_ = thickness_;
                gfx.icon_ = icon_;
                gfx.iconName_ = iconName_;
                gfx.iconColor_ = iconColor_;
                gfx.iconUseColor_ = iconUseColor_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            protected override bool supportCheck(string name) => name != "battery";

            #region Rendering
            struct RenderData
            {
                public Vector2 position;
                public Vector2 size;
                public float ratio;

                public Vector2 iconSize;
                public Color iconColor;
            }
            RenderData renderData_ = new RenderData();

            public override void prepareRendering(Display display)
            {
                renderData_.size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                renderData_.position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                renderData_.ratio = (float)DataAccessor.indicator();

                renderData_.iconSize = renderData_.size * 0.45f;
                if (iconUseColor_)
                    renderData_.iconColor = iconColor_;
                else if (colorLerp_)
                    getGradientColorLerp(renderData_.ratio, Gradient, out renderData_.iconColor);
                else
                    renderData_.iconColor = getGradientColor(renderData_.ratio);
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Graphic.renderEllipseBar(addSprite, rt, renderData_.position, renderData_.size, minDegree_, maxDegree_, 
                    steps_, thickness_, renderData_.ratio, Gradient, colorLerp_, Template.BackgroundColor);

                if (icon_ != null)
                    icon_(addSprite, rt, iconName_, renderData_.position, renderData_.iconSize, 0f, renderData_.iconColor);
            }
            #endregion // Rendering

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var handler = base.getConfigHandler();
                handler.add("style", configStyle);
                handler.add("icon", configIcon);

                return handler;
            }

            float minDegree_ = -50f;
            float maxDegree_ = 230f;
            bool colorLerp_ = true;
            int steps_ = 40;
            float thickness_ = 0.07f;
            bool configStyle(string key, string value, Configuration.Options options)
            {
                minDegree_ = Configuration.asFloat(value, minDegree_);
                maxDegree_ = options.asFloat(0, maxDegree_);
                colorLerp_ = options.asBoolean(1, colorLerp_);

                if (minDegree_ > maxDegree_)
                    minDegree_ -= 360f;

                steps_ = options.asInteger(2, steps_);
                thickness_ = options.asFloat(3, thickness_);

                return true;
            }

            Icon.Render icon_ = null;
            string iconName_ = "";
            Color iconColor_ = Default.FontColor;
            bool iconUseColor_ = true;
            bool configIcon(string key, string value, Configuration.Options options)
            {
                iconName_ = value;
                Graphic.Icon.getIcon(value);

                if (options.Count >= 1)
                {
                    iconColor_ = options.asColor(0, Template.FontColor);
                    iconUseColor_ = true;
                }
                else
                    iconUseColor_ = false;

                return true;
            }
            #endregion // Configuration
        }
    }
}
