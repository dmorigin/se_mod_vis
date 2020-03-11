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
        public class GraphicBar : Graphic
        {
            public GraphicBar(Template template, Configuration.Options options)
                : base(template, options)
            {
                renderStyledBar_ = renderSimpleBar;
            }

            public override Graphic clone()
            {
                GraphicBar gfx = new GraphicBar(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataAccessor = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;
                gfx.VisibleThresholdA = VisibleThresholdA;
                gfx.VisibleOperatorA = VisibleOperatorA;
                gfx.VisibleThresholdB = VisibleThresholdB;
                gfx.VisibleOperatorB = VisibleOperatorB;
                gfx.VisibleCondition = VisibleCondition;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.vertical_ = vertical_;
                gfx.backgroundColor_ = backgroundColor_;
                gfx.borderSize_ = borderSize_;
                gfx.borderSizeType_ = borderSizeType_;
                gfx.borderColor_ = borderColor_;
                gfx.tiles_ = tiles_;
                gfx.tileSpace_ = tileSpace_;
                gfx.tileSpaceType_ = tileSpaceType_;
                gfx.renderStyledBar_ = renderStyledBar_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                if (DataAccessor == null || !isVisible(DataAccessor.indicator()))
                    return;

                if (Gradient.Count == 0)
                    addGradientColor(0f, Default.BarColor);

                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                float borderSize = borderSizeType_ == ValueType.Relative ? borderSize_ * (size.X < size.Y ? size.X : size.Y) : borderSize_;
                float tileSpace = tileSpaceType_ == ValueType.Relative ? tileSpace_ * (size.X < size.Y ? size.X : size.Y) : tileSpace_;

                renderStyledBar_(addSprite, rt, position, size, vertical_, DataAccessor.min() < 0.0, tiles_, tileSpace,
                    (float)DataAccessor.indicator(), Gradient, borderSize, borderColor_, backgroundColor_);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var handler = base.getConfigHandler();
                handler.add("bgcolor", configBackgroundColor);
                handler.add("border", configBorder);
                handler.add("style", configBarStyle);

                return handler;
            }

            Color backgroundColor_ = Default.BarBackgroundColor;
            bool configBackgroundColor(string key, string value, Configuration.Options options)
            {
                backgroundColor_ = Configuration.asColor(value, Default.BarBackgroundColor);
                return true;
            }

            float borderSize_ = Default.BarBorderSize;
            ValueType borderSizeType_ = Default.SizeType;
            Color borderColor_ = Default.BarBorderColor;
            bool configBorder(string key, string value, Configuration.Options options)
            {
                borderSize_ = Configuration.asFloat(value, Default.BarBorderSize);
                if (options.Count > 0)
                {
                    if (!toValueType(options[0], out borderSizeType_, Default.SizeType))
                        return false;

                    borderColor_ = options.asColor(1, Default.BarBorderColor);
                }

                return true;
            }

            delegate void RenderStyledBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size, 
                bool vertical, bool doubleSided, int tiles, float tileSpace, float ratio, Dictionary<float, Color> gradient,
                float borderSize, Color borderColor, Color backgroundColor);
            RenderStyledBar renderStyledBar_;
            bool vertical_ = Default.BarVertical;
            int tiles_ = 0;
            float tileSpace_ = Default.BarTileSpace;
            ValueType tileSpaceType_ = Default.BarTileSpaceType;
            bool configBarStyle(string key, string value, Configuration.Options options)
            {
                vertical_ = options.asBoolean(0, Default.BarVertical);

                switch (value.ToLower())
                {
                    case "simple":
                        renderStyledBar_ = Graphic.renderSimpleBar;
                        break;
                    case "segments":
                        renderStyledBar_ = Graphic.renderSegmentedBar;
                        break;
                    case "tiles":
                        renderStyledBar_ = Graphic.renderTiledBar;
                        tiles_ = options.asInteger(1, Default.BarTileCount);
                        tileSpace_ = options.asFloat(2, Default.BarTileSpace);
                        if (options.Count >= 4)
                        {
                            if (!toValueType(options[3], out tileSpaceType_, Default.BarTileSpaceType))
                                return false;
                        }
                        break;
                    default:
                        log(Console.LogType.Error, $"Invalid bar style '{value}'");
                        return false;
                }

                return true;
            }
            #endregion // Configuration
        }
    }
}
