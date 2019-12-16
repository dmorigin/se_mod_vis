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
            }

            public override Graphic clone()
            {
                GraphicBar gfx = new GraphicBar(Template, Options);
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

                gfx.vertical_ = vertical_;
                gfx.backgroundColor_ = backgroundColor_;

                return gfx;
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                float ratio = 0f;
                if (DataRetriever != null)
                    ratio = (float)DataRetriever.indicator();
                ratio = ratio < 0f ? 0f : (ratio > 1f ? 1f : ratio);

                Graphic.renderBar(rt, addSprite, position, size, vertical_, ratio, Gradient, backgroundColor_);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var handler = base.getConfigHandler();
                handler.add("vertical", configVertical);
                handler.add("backgroundcolor", configBackgroundColor);

                return handler;
            }

            bool vertical_ = Program.Default.BarVertical;
            bool configVertical(string key, string value, Configuration.Options options)
            {
                vertical_ = Configuration.asBoolean(value, Program.Default.BarVertical);
                return true;
            }

            Color backgroundColor_ = Program.Default.BarBackgroundColor;
            bool configBackgroundColor(string key, string value, Configuration.Options options)
            {
                backgroundColor_ = Configuration.asColor(value, Program.Default.BarBackgroundColor);
                return true;
            }
            #endregion // Configuration
        }
    }
}
