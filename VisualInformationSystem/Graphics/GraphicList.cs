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
        public class GraphicList : Graphic
        {
            public GraphicList(ContentContainer template, Configuration.Options options)
                : base(template, options)
            {
                VisibleOperatorA = greater;
                VisibleThresholdA = 0.0;
            }

            protected override bool supportCheck(string name)
            {
                return name != "shipcontroller";
            }

            /*public override Graphic clone()
            {
                GraphicList gfx = new GraphicList(Template, Options);

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

                gfx.lines_ = lines_;
                gfx.spacing_ = spacing_;

                gfx.textShow_ = textShow_;
                gfx.textStyle_ = textStyle_;

                gfx.barShow_ = barShow_;
                gfx.barEmbedded_ = barEmbedded_;
                gfx.barRenderMethod_ = barRenderMethod_;
                gfx.barThickness_ = barThickness_;
                gfx.barThicknessType_ = barThicknessType_;
                gfx.barBackground_ = barBackground_;

                gfx.iconShow_ = iconShow_;
                gfx.autoScroll_ = autoScroll_;
                gfx.autoScrollInc_ = autoScrollInc_;

                gfx.replace_ = new Dictionary<double, string>();
                foreach (var pair in replace_)
                    gfx.replace_.Add(pair.Key, pair.Value);
                gfx.replace_ = gfx.replace_.OrderByDescending(x => x.Key).ToDictionary(a => a.Key, b => b.Value);

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }*/

            #region Rendering
            class RenderDataList : RenderDataBase
            {
                public int Lines;
                public float LineHeight;
                public float FontSize;

                public Vector2 IconPosition;
                public Vector2 IconSize;

                public Vector2 BarPosition;
                public Vector2 BarSize;
                public int BarTiles;
                public float BarTileSpacing;

                public float TextLeftPositionX;
                public float TextRightPositionX;
                public float TextPositionY;

                public List<DataAccessor.ListContainer> Container;
            }

            //RenderDataList renderData = new RenderDataList();
            protected override RenderDataBase createRenderDataObj() => new RenderDataList();

            public override void prepareRendering(Display display)
            {
                base.prepareRendering(display);
                RenderDataList renderData = RenderData as RenderDataList;
                //Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                //Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = RenderData.InnerSize;
                Vector2 position = RenderData.Position;

                float fontHeight = textShow_ == true ? Default.CharHeight * Template.FontSize : 0f;
                float barThickness = barThicknessType_ == ValueType.Relative ? barThickness_ * fontHeight : barThickness_;

                renderData.LineHeight = fontHeight + spacing_;
                if (barShow_)
                {
                    renderData.BarSize = new Vector2(size.X, barThickness);
                    renderData.LineHeight += barEmbedded_ ? 0f : renderData.BarSize.Y;
                }
                else
                    renderData.BarSize = new Vector2();

                if (lines_ > 0)
                {
                    float scale = (size.Y / lines_) / renderData.LineHeight;

                    fontHeight *= scale;
                    renderData.BarSize.Y *= scale;
                    renderData.LineHeight *= scale;
                    renderData.Lines = lines_;
                    renderData.FontSize = Template.FontSize * scale;
                }
                else
                {
                    renderData.Lines = (int)(size.Y / renderData.LineHeight);
                    renderData.FontSize = Template.FontSize;
                }

                if (iconShow_ && (!textShow_ || barEmbedded_))
                    renderData.BarSize.X -= renderData.BarSize.Y;

                // calculate bar tiles
                if (barShow_)
                {
                    renderData.BarTiles = (int)((renderData.BarSize.X / renderData.BarSize.Y) * 2f);
                    renderData.BarTileSpacing = renderData.BarSize.X * 0.01f;
                }

                // calculate Y position
                renderData.TextPositionY = position.Y - (size.Y * 0.5f) + (fontHeight * 0.5f);
                renderData.BarPosition.Y = position.Y - (size.Y * 0.5f) + (renderData.BarSize.Y * 0.5f) + (barEmbedded_ ? 0f : fontHeight);

                // icon
                if (iconShow_)
                {
                    // adjust bar size if no text
                    if (textShow_ == false)
                    {
                        renderData.BarSize.X -= renderData.BarSize.Y;
                        renderData.IconSize = new Vector2(renderData.BarSize.Y, renderData.BarSize.Y);
                    }
                    else
                        renderData.IconSize = new Vector2(fontHeight, fontHeight);

                    renderData.IconPosition.Y = position.Y - (size.Y * 0.5f) + (renderData.IconSize.Y * 0.5f);
                }

                // calculate X position
                renderData.IconPosition.X = position.X - (size.X * 0.5f) + (renderData.IconSize.X * 0.5f);
                renderData.BarPosition.X = position.X - (size.X * 0.5f) + (renderData.BarSize.X * 0.5f) + (barEmbedded_ ? renderData.IconSize.X : 0f);
                renderData.TextLeftPositionX = position.X - (size.X * 0.5f) + renderData.IconSize.X;
                renderData.TextRightPositionX = position.X + (size.X * 0.5f);
                renderData.BarSize = new Vector2(renderData.BarSize.Y, renderData.BarSize.X);

                // filter list
                DataAccessor.list(out renderData.Container, (item) => isVisible(item.indicator));

                // auto scroll
                if (autoScroll_ == true)
                {
                    if (renderData.Lines >= renderData.Container.Count)
                        autoScrollLine_ = 0;
                    else
                    {
                        autoScrollLine_ += autoScrollInc_;

                        if (autoScrollLine_ < 0)
                        {
                            autoScrollLine_ = 0;
                            autoScrollInc_ *= -1;
                        }
                        else if (autoScrollLine_ > (renderData.Container.Count - renderData.Lines))
                        {
                            autoScrollLine_ = renderData.Container.Count - renderData.Lines;
                            autoScrollInc_ *= -1;
                        }
                    }
                }

                if (Gradient.Count == 0)
                    addGradientColor(0.0f, Template.FontColor);
            }

            public override void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                base.render(display, rt, addSprite);
                RenderDataList renderData = RenderData as RenderDataList;
                Vector2 iconPosition = renderData.IconPosition;
                Vector2 barPosition = renderData.BarPosition;
                float textPositionY = renderData.TextPositionY;

                // render name
                for (int l = autoScrollLine_; l < (renderData.Lines + autoScrollLine_) && l < renderData.Container.Count; l++)
                {
                    var entry = renderData.Container[l];

                    // draw icon
                    if (iconShow_)
                    {
                        string iconType = $"{entry.type.TypeId}/{entry.type.SubtypeId}";
                        if (RenderTarget.spriteExist(iconType))
                            addSprite(new MySprite(SpriteType.TEXTURE, iconType, iconPosition + rt.DisplayOffset, renderData.IconSize, Color.White));
                        iconPosition.Y += renderData.LineHeight;
                    }

                    // draw bar
                    if (barShow_)
                    {
                        barRenderMethod_(addSprite, rt, barPosition, renderData.BarSize, (float)Math.PI * 0.5f, false, renderData.BarTiles, 
                            renderData.BarTileSpacing, IconNameSquareSimple, (float)entry.indicator, Gradient, 0f, Default.BarBorderColor, barBackground_);
                        barPosition.Y += renderData.LineHeight;
                    }

                    // draw text
                    if (textShow_)
                    {
                        renderTextLine(display, rt, addSprite, Template.Font, renderData.FontSize, 
                            new Vector2(renderData.TextLeftPositionX, textPositionY), 
                            getGradientColor((float)entry.indicator), entry.name, TextAlignment.LEFT);

                        if (textStyle_ != TextStyle.OnlyName)
                        {
                            renderTextLine(display, rt, addSprite, Template.Font, renderData.FontSize,
                                new Vector2(renderData.TextRightPositionX, textPositionY),
                                getGradientColor((float)entry.indicator), getRightText(entry), TextAlignment.RIGHT);
                        }

                        textPositionY += renderData.LineHeight;
                    }
                }
            }

            string getRightText(DataAccessor.ListContainer entry)
            {
                if (replace_.Count == 0)
                    return textStyle_ == TextStyle.CurrentValue ? $"{entry.value.pack()}" : $"{entry.value.pack()}/{entry.max.pack()}";

                foreach (var pair in replace_)
                {
                    if (pair.Key <= entry.value)
                        return pair.Value;
                }

                return "";
            }
            #endregion // Rendering

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler config = base.getConfigHandler();
                if (config != null)
                {
                    config.add("text", configText);
                    config.add("bar", configBar);
                    config.add("icon", configIcon);
                    config.add("setline", configSetLine);
                    config.add("autoscroll", configAutoScroll);
                    config.add("replace", configReplace);
                }

                return config;
            }

            enum TextStyle
            {
                Normal,
                OnlyName,
                CurrentValue,
            };
            bool textShow_ = true;
            TextStyle textStyle_ = TextStyle.Normal;
            bool configText(string key, string value, Configuration.Options options)
            {
                textShow_ = Configuration.asBoolean(value, true);
                if (textShow_ == true)
                {
                    switch (options[0].ToLower())
                    {
                        case "":
                        case "normal":
                            textStyle_ = TextStyle.Normal;
                            break;
                        case "onlyname":
                            textStyle_ = TextStyle.OnlyName;
                            break;
                        case "minvalue":
                        case "currentvalue":
                            textStyle_ = TextStyle.CurrentValue;
                            break;
                        default:
                            return false;
                    }
                }

                return true;
            }
            
            bool barShow_ = false;
            bool barEmbedded_ = false;
            RenderStyledBar barRenderMethod_;
            float barThickness_ = Default.ListBarThickness;
            ValueType barThicknessType_ = Default.ListBarThicknessType;
            Color barBackground_ = Default.BarBackgroundColor;
            bool configBar(string key, string value, Configuration.Options options)
            {
                barShow_ = true;
                barEmbedded_ = Configuration.asBoolean(value, false);

                switch (options[0].ToLower())
                {
                    case "simple":
                        barRenderMethod_ = Graphic.renderSimpleBar;
                        break;
                    case "segments":
                        barRenderMethod_ = Graphic.renderSegmentedBar;
                        break;
                    case "tiles":
                        barRenderMethod_ = Graphic.renderTiledBar;
                        break;
                    default:
                        log(Console.LogType.Error, $"Invalid list bar style: {options[0]}");
                        return false;
                }

                barThickness_ = Configuration.asFloat(options[1], Default.ListBarThickness);
                if (!toValueType(options[2], out barThicknessType_, Default.ListBarThicknessType))
                    return false;
                if (barThickness_ <= 0f || barEmbedded_)
                {
                    barThickness_ = 1f;
                    barThicknessType_ = ValueType.Relative;
                }

                barBackground_ = Configuration.asColor(options[3], Template.BackgroundColor);
                return true;
            }

            bool iconShow_ = false;
            bool configIcon(string key, string value, Configuration.Options options)
            {
                iconShow_ = Configuration.asBoolean(value, false);
                return true;
            }

            int lines_ = 0; // if 0 the amount of lines will be automaticaly calculated
            float spacing_ = 7f;
            bool configSetLine(string key, string value, Configuration.Options options)
            {
                spacing_ = Configuration.asFloat(value, 7f);
                lines_ = options.asInteger(0, 0);
                return true;
            }

            bool autoScroll_ = true;
            int autoScrollLine_ = 0;
            int autoScrollInc_ = 1;
            bool configAutoScroll(string key, string value, Configuration.Options options)
            {
                autoScroll_ = Configuration.asBoolean(value, true);
                autoScrollInc_ = options.asInteger(0, 1);
                return true;
            }

            Dictionary<double, string> replace_ = new Dictionary<double, string>();
            bool configReplace(string key, string value, Configuration.Options options)
            {
                double val = Configuration.asFloat(value);
                string text = options[0];
                replace_[val] = text;
                replace_ = replace_.OrderByDescending(x => x.Key).ToDictionary(a => a.Key, b => b.Value);
                return true;
            }
            #endregion // Configuration
        }
    }
}
