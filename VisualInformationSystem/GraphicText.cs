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
        public class GraphicText : Graphic
        {
            public GraphicText(Template template, Configuration.Options options)
                : base(template, options)
            {
            }


            public override Graphic clone()
            {
                GraphicText gfx = new GraphicText(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataRetriever = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.useDefaultFont_ = useDefaultFont_;
                gfx.useFontSize_ = useFontSize_;
                gfx.useDefaultAlignment_ = useDefaultAlignment_;
                gfx.font_ = font_;
                gfx.fontSize_ = fontSize_;
                gfx.alignment_ = alignment_;
                gfx.text_.AddList(text_);

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }


            bool useDefaultFont_ = true;
            bool useFontSize_ = false;
            float fontSize_ = Default.FontSize;
            public float FontSize
            {
                get { return useDefaultFont_ ? Template.FontSize : fontSize_; }
            }


            string font_ = Default.Font;
            public string Font
            {
                get { return useDefaultFont_ ? Template.Font : font_; }
            }


            public Color FontColor
            {
                get { return useDefaultFont_ ? Template.FontColor : Color; }
            }


            bool useDefaultAlignment_ = true;
            TextAlignment alignment_ = Default.FontAlignment;
            public TextAlignment TextAlignment
            {
                get { return useDefaultAlignment_ ? Template.TextAlignment : alignment_; }
            }


            List<string> text_ = new List<string>();


            #region Configuration
            bool configFont(string key, string value, Configuration.Options options)
            {
                font_ = value != string.Empty ? value : Default.Font;
                fontSize_ = options.asFloat(0, 0f);
                if (fontSize_ == 0f)
                    useFontSize_ = false;
                else
                    useFontSize_ = true;

                Color = options.asColor(1, Default.FontColor);
                useDefaultFont_ = false;
                return true;
            }

            bool configText(string key, string value, Configuration.Options options)
            {
                text_.Add(value);
                return true;
            }

            bool configAlignment(string key, string value, Configuration.Options options)
            {
                string data = value.ToLower();
                switch (data)
                {
                    case "center":
                    case "c":
                        alignment_ = TextAlignment.CENTER;
                        break;
                    case "left":
                    case "l":
                        alignment_ = TextAlignment.LEFT;
                        break;
                    case "right":
                    case "r":
                        alignment_ = TextAlignment.RIGHT;
                        break;
                    default:
                        return false;
                }

                useDefaultAlignment_ = false;
                return true;
            }

            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler handler = base.getConfigHandler();
                handler.add("font", configFont);
                handler.add("text", configText);
                handler.add("alignment", configAlignment);

                return handler;
            }
            #endregion // Configuration


            protected override bool supportCheck(string name)
            {
                return true;
            }

            #region Rendering
            struct RenderData
            {
                public float fontSize;
                public Color fontColor;

                public Vector2 position;
                public List<string> lines;
            }

            RenderData renderData_ = new RenderData();

            public override void prepareRendering(Display display)
            {
                renderData_.fontSize = FontSize;
                renderData_.lines = new List<string>();
                Vector2 maxSize = new Vector2(0f, 0f);

                foreach (string text in text_)
                {
                    string line = DataCollector != null ? DataCollector.getText(text) : text;
                    Vector2 lineSize = display.measureLineInPixels(line, Font, renderData_.fontSize);

                    maxSize.X = lineSize.X > maxSize.X ? lineSize.X : maxSize.X;
                    maxSize.Y = lineSize.Y > maxSize.Y ? lineSize.Y : maxSize.Y;
                    renderData_.lines.Add(line);
                }

                if (!useFontSize_)
                {
                    Vector2 size = SizeType == Graphic.ValueType.Relative ? Size * display.RenderArea.Size : Size;
                    renderData_.fontSize = Math.Min(size.X / maxSize.X, size.Y / (maxSize.Y * renderData_.lines.Count));
                }

                Vector2 position = PositionType == Graphic.ValueType.Relative ? Position * display.RenderArea.Size : Position;
                renderData_.position = new Vector2(position.X, position.Y - ((maxSize.Y * (renderData_.lines.Count - 1)) * 0.5f));

                if (Gradient.Count > 0)
                    renderData_.fontColor = DataRetriever != null ? getGradientColor((float)DataRetriever.indicator()) : Color;
                else
                    renderData_.fontColor = FontColor;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                for (int c = 0; c < renderData_.lines.Count; c++)
                {
                    Graphic.renderTextLine(display, rt, addSprite, Font, renderData_.fontSize, 
                        new Vector2(renderData_.position.X, renderData_.position.Y + (c * renderData_.fontSize)), 
                        renderData_.fontColor, renderData_.lines[c], TextAlignment);
                }
            }
            #endregion // Rendering
        }
    }
}
