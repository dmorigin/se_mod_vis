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
                gfx.DataRetriever = gfx.DataCollector.getDataRetriever(DataRetrieverName);
                gfx.DataRetrieverName = DataRetrieverName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.vertical_ = vertical_;
                gfx.backgroundColor_ = backgroundColor_;
                gfx.borderSize_ = borderSize_;
                gfx.borderColor_ = borderColor_;
                gfx.tiles_ = tiles_;
                gfx.tileSpace_ = tileSpace_;
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
                if (DataRetriever == null)
                    return;

                if (Gradient.Count == 0)
                    addGradientColor(0f, Default.BarColor);

                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                renderStyledBar_(addSprite, position, size, vertical_, DataRetriever.min() < 0.0, tiles_, tileSpace_,
                    (float)DataRetriever.indicator(), Gradient, borderSize_, borderColor_, backgroundColor_);
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
            Color borderColor_ = Default.BarBackgroundColor;
            bool configBorder(string key, string value, Configuration.Options options)
            {
                borderSize_ = Configuration.asFloat(value, Default.BarBorderSize);
                borderColor_ = options.asColor(0, Default.BarBorderColor);
                return true;
            }

            delegate void RenderStyledBar(AddSpriteDelegate addSprite, Vector2 position, Vector2 size, bool vertical, bool doubleSided,
                int tiles, float tileSpace, float ratio, Dictionary<float, Color> gradient, float borderSize, Color borderColor, Color backgroundColor);
            RenderStyledBar renderStyledBar_;
            bool vertical_ = Default.BarVertical;
            int tiles_ = 0;
            float tileSpace_ = Default.BarTileSpace;
            bool configBarStyle(string key, string value, Configuration.Options options)
            {
                switch(value.ToLower())
                {
                    case "simple":
                        renderStyledBar_ = Graphic.renderSimpleBar;
                        vertical_ = options.asBoolean(0, Default.BarVertical);
                        break;
                    case "segmented":
                        renderStyledBar_ = Graphic.renderSegmentedBar;
                        vertical_ = options.asBoolean(0, Default.BarVertical);
                        break;
                    case "tiled":
                        renderStyledBar_ = Graphic.renderTiledBar;
                        vertical_ = options.asBoolean(0, Default.BarVertical);
                        tiles_ = options.asInteger(1, 0);
                        tileSpace_ = options.asFloat(2, Default.BarTileSpace);
                        break;
                    default:
                        return false;
                }

                return true;
            }
            #endregion // Configuration
        }
    }
}
