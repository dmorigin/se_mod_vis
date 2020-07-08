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
            public GraphicText(ContentContainer template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name) => true;


            /*public override Graphic clone()
            {
                GraphicText gfx = new GraphicText(Template, Options);

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

                gfx.useDefaultFont_ = useDefaultFont_;
                gfx.sizeIsSet = sizeIsSet;
                gfx.useDefaultAlignment_ = useDefaultAlignment_;
                gfx.font_ = font_;
                gfx.fontSize_ = fontSize_;
                gfx.alignment_ = alignment_;
                gfx.text_.AddList(text_);

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }*/

            bool useDefaultFont_ = true;
            bool sizeIsSet = false;
            string font_ = Default.Font;
            float fontSize_ = Default.FontSize;
            bool useDefaultAlignment_ = true;
            TextAlignment alignment_ = Default.FontAlignment;

            public float FontSize => useDefaultFont_ ? Template.FontSize : fontSize_;
            public string Font => useDefaultFont_ ? Template.Font : font_;
            public Color FontColor => useDefaultFont_ ? Template.FontColor : Color;
            public TextAlignment TextAlignment => useDefaultAlignment_ ? Template.TextAlignment : alignment_;

            List<string> text_ = new List<string>();

            #region Configuration
            class Config : ConfigHandler
            {
                GraphicText gfx_;
                public Config(GraphicText gfx)
                    : base(gfx)
                {
                    gfx_ = gfx;
                    add("font", configFont);
                    add("text", configText);
                    add("alignment", configAlignment);
                }

                bool configFont(string key, string value, Configuration.Options options)
                {
                    gfx_.font_ = value != string.Empty ? value : Default.Font;
                    gfx_.fontSize_ = options.asFloat(0, 0f);
                    gfx_.Color = options.asColor(1, Default.FontColor);
                    gfx_.useDefaultFont_ = false;
                    return true;
                }

                bool configText(string key, string value, Configuration.Options options)
                {
                    gfx_.text_.Add(value);
                    return true;
                }

                bool configAlignment(string key, string value, Configuration.Options options)
                {
                    string data = value.ToLower();
                    switch (data)
                    {
                        case "center":
                        case "c":
                            gfx_.alignment_ = TextAlignment.CENTER;
                            break;
                        case "left":
                        case "l":
                            gfx_.alignment_ = TextAlignment.LEFT;
                            break;
                        case "right":
                        case "r":
                            gfx_.alignment_ = TextAlignment.RIGHT;
                            break;
                        default:
                            return false;
                    }

                    gfx_.useDefaultAlignment_ = false;
                    return true;
                }

                protected override bool configSize(string key, string value, Configuration.Options options)
                {
                    gfx_.sizeIsSet = true;
                    return base.configSize(key, value, options);
                }
            }

            public override ConfigHandler getConfigHandler() => new Config(this);
            #endregion // Configuration

            #region Rendering
            class RenderDataText : RenderDataBase
            {
                public float FontSize;
                public Color FontColor;

                public Vector2 TextPosition;
                public float LineHeight;
                public List<string> Lines;

                public bool Visible;
            }

            protected override RenderDataBase createRenderDataObj() => new RenderDataText();

            string processTextLine(string text)
            {
                string line = text
                    .Replace("%time_hhmmss%", DateTime.Now.ToString("HH:mm:ss"))
                    .Replace("%time_hhmm%", DateTime.Now.ToString("HH:mm"))
                    .Replace("%date_ddmmyyyy%", DateTime.Now.ToString("dd.MM.yyyy"))
                    .Replace("%date_mmddyyyy%", DateTime.Now.ToString("MM/dd/yyyy"));

                if (DataAccessor != null)
                {
                    line = line
                        .Replace("%min%", DataAccessor.min().pack())
                        .Replace("%max%", DataAccessor.max().pack())
                        .Replace("%value%", DataAccessor.value().pack())
                        .Replace("%indicator%", new Program.VISUnitType(DataAccessor.indicator(), unit: Unit.Percent).pack());
                }

                return DataCollector != null ? DataCollector.getVariable(line) : line;
            }

            public override void prepareRendering(Display display)
            {
                float fontSize = FontSize;
                bool dynFontSize = fontSize == 0f || (useDefaultFont_ && sizeIsSet);
                RenderDataText renderData = RenderData as RenderDataText;

                renderData.Lines = new List<string>();
                Vector2 maxSize = new Vector2(0f, 0f);

                Func<string, string> calcMaxSize = (text) =>
                {
                    string line = processTextLine(text);
                    Vector2 lineSize = display.measureLineInPixels(line, Font, dynFontSize ? 1f : fontSize);

                    maxSize.X = Math.Max(maxSize.X, lineSize.X);
                    maxSize.Y = Math.Max(maxSize.Y, lineSize.Y);

                    return line;
                };

                foreach (string text in text_)
                {
                    // add text from display text field
                    if (text == "%display_text_field%")
                    {
                        string[] lines = display.Text.Split('\n');
                        foreach (string line in lines)
                            renderData.Lines.Add(calcMaxSize(line));
                    }
                    else
                        renderData.Lines.Add(calcMaxSize(text));
                }
                
                if (dynFontSize)
                {
                    //Vector2 size = SizeType == Graphic.ValueType.Relative ? Size * display.RenderArea.Size : Size;
                    base.prepareRendering(display);
                    renderData.FontSize = Math.Min(renderData.InnerSize.X / maxSize.X, renderData.InnerSize.Y / (maxSize.Y * renderData.Lines.Count));
                    renderData.LineHeight = maxSize.Y * renderData.FontSize;
                }
                else
                {
                    Size = maxSize;
                    SizeType = ValueType.Absolute;
                    base.prepareRendering(display);
                    var min = Math.Min(renderData.InnerSize.X / maxSize.X, renderData.InnerSize.Y / (maxSize.Y * renderData.Lines.Count));
                    renderData.FontSize = fontSize * min;
                    renderData.LineHeight = maxSize.Y * min;
                }

                //Vector2 position = PositionType == Graphic.ValueType.Relative ? Position * display.RenderArea.Size : Position;
                renderData.TextPosition = new Vector2(renderData.Position.X,
                    renderData.Position.Y - ((renderData.LineHeight * (renderData.Lines.Count - 1)) * 0.5f));

                if (Gradient.Count > 0)
                    renderData.FontColor = DataAccessor != null ? getGradientColor((float)DataAccessor.indicator()) : Color;
                else
                    renderData.FontColor = FontColor;

                renderData.Visible = DataAccessor != null ? isVisible(DataAccessor.indicator()) : true;
            }

            public override void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                RenderDataText renderData = RenderData as RenderDataText;
                if (!renderData.Visible)
                    return;

                for (int c = 0; c < renderData.Lines.Count; c++)
                {
                    renderTextLine(display, rt, addSprite, Font, renderData.FontSize, 
                        new Vector2(renderData.TextPosition.X, renderData.TextPosition.Y + (c * renderData.LineHeight)), 
                        renderData.FontColor, renderData.Lines[c], TextAlignment);
                }
            }
            #endregion // Rendering
        }
    }
}
