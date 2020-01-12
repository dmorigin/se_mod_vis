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
            public GraphicList(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            public override Graphic clone()
            {
                GraphicList gfx = new GraphicList(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataRetriever = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.lines_ = lines_;
                gfx.spacing_ = spacing_;
                gfx.showBar_ = showBar_;
                gfx.showIcon_ = showIcon_;
                gfx.showText_ = showText_;
                gfx.showMissing_ = showMissing_;
                gfx.barBackground_ = barBackground_;
                gfx.barThickness_ = barThickness_;
                gfx.autoScroll_ = autoScroll_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            #region Rendering
            struct RenderData
            {
                public int lines;
                public float lineHeight;

                public Vector2 iconPosition;
                public Vector2 iconSize;

                public Vector2 barPosition;
                public Vector2 barSize;

                public float textLeftPositionX;
                public float textRightPositionX;
                public float textPositionY;

                public List<DataAccessor.ListContainer> container;
            }

            RenderData renderData_ = new RenderData();

            public override void prepareRendering(Display display)
            {
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                float fontHeight = showText_ == true ? Default.CharHeight * Template.FontSize : 0f;
                renderData_.barSize = showBar_ ? new Vector2(size.X, barThickness_ == 0f ? fontHeight : barThickness_) : new Vector2();
                renderData_.lineHeight = fontHeight + renderData_.barSize.Y + spacing_;

                if (lines_ > 0)
                {
                    float scale = (size.Y / lines_) / renderData_.lineHeight;

                    fontHeight *= scale;
                    renderData_.barSize.Y *= scale;
                    renderData_.lineHeight = fontHeight + renderData_.barSize.Y + spacing_;
                }

                if (showIcon_ && !showText_)
                    renderData_.barSize.X -= renderData_.barSize.Y;

                renderData_.lines = (int)Math.Ceiling(size.Y / renderData_.lineHeight);
                renderData_.textPositionY = position.Y - (size.Y * 0.5f) + (fontHeight * 0.5f);
                renderData_.barPosition.Y = position.Y - (size.Y * 0.5f) + (renderData_.barSize.Y * 0.5f) + fontHeight;

                // icon
                if (showIcon_)
                {
                    // adjust bar size if no text
                    if (showText_ == false)
                    {
                        renderData_.barSize.X -= renderData_.barSize.Y;
                        renderData_.iconSize = new Vector2(renderData_.barSize.Y, renderData_.barSize.Y);
                    }
                    else
                        renderData_.iconSize = new Vector2(fontHeight, fontHeight);

                    renderData_.iconPosition.Y = position.Y - (size.Y * 0.5f) + (renderData_.iconSize.Y * 0.5f);
                }

                renderData_.iconPosition.X = position.X - (size.X * 0.5f) + (renderData_.iconSize.X * 0.5f);
                renderData_.barPosition.X = position.X - (size.X * 0.5f) + (renderData_.barSize.X * 0.5f);
                renderData_.textLeftPositionX = position.X - (size.X * 0.5f) + renderData_.iconSize.X;
                renderData_.textRightPositionX = position.X + (size.X * 0.5f);

                DataRetriever.list(out renderData_.container, (item) => showMissing_ || item.value > 0);

                // auto scroll
                if (autoScroll_ == true)
                {
                    if (renderData_.lines >= renderData_.container.Count)
                        autoScrollLine_ = 0;
                    else
                    {
                        autoScrollLine_ += autoScrollInc_;

                        // toggle inc
                        if (autoScrollLine_ >= (renderData_.container.Count - renderData_.lines) || autoScrollLine_ < 0)
                        {
                            autoScrollInc_ *= -1;
                            autoScrollLine_ += autoScrollInc_;
                        }
                    }
                }

                if (Gradient.Count == 0)
                    addGradientColor(0.0f, Template.FontColor);
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Vector2 iconPosition = renderData_.iconPosition;
                Vector2 barPosition = renderData_.barPosition;
                float textPositionY = renderData_.textPositionY;

                // render name
                for (int l = autoScrollLine_; l < (renderData_.lines + autoScrollLine_) && l < renderData_.container.Count; l++)
                {
                    var entry = renderData_.container[l];
                    if (entry.value == 0.0 && !showMissing_)
                        continue;

                    // draw icon
                    if (showIcon_)
                    {
                        string iconType = $"{entry.type.TypeId}/{entry.type.SubtypeId}";
                        if (RenderTarget.spriteExist(iconType))
                        {
                            addSprite(new MySprite(SpriteType.TEXTURE, iconType, iconPosition, renderData_.iconSize, Color.White));
                            iconPosition.Y += renderData_.lineHeight;
                        }
                    }

                    // draw bar
                    if (showBar_)
                    {
                        renderSimpleBar(addSprite, rt, barPosition, renderData_.barSize, false, false, 0, 0f,
                            (float)entry.indicator, Gradient, 0f, Default.BarBorderColor, barBackground_);
                        barPosition.Y += renderData_.lineHeight;
                    }

                    // draw text
                    if (showText_)
                    {
                        string rightText = $"{entry.value.pack()}/{entry.max.pack()}";

                        renderTextLine(display, rt, addSprite, Template.Font, Template.FontSize, 
                            new Vector2(renderData_.textLeftPositionX, textPositionY), 
                            getGradientColor((float)entry.indicator), entry.name, TextAlignment.LEFT);
                        renderTextLine(display, rt, addSprite, Template.Font, Template.FontSize,
                            new Vector2(renderData_.textRightPositionX, textPositionY),
                            getGradientColor((float)entry.indicator), rightText, TextAlignment.RIGHT);

                        textPositionY += renderData_.lineHeight;
                    }
                }
            }
            #endregion // Rendering

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler config = base.getConfigHandler();
                if (config != null)
                {
                    config.add("setline", configSetLine);
                    config.add("showicon", configShowIcon);
                    config.add("showtext", configShowText);
                    config.add("showbar", configShowBar);
                    config.add("autoscroll", configAutoScroll);
                    config.add("showmissing", configShowMissing);
                }

                return config;
            }

            int lines_ = 0; // if 0 the amount of lines will be automaticaly calculated
            float spacing_ = 7f;
            bool configSetLine(string key, string value, Configuration.Options options)
            {
                spacing_ = Configuration.asFloat(value, 7f);
                lines_ = options.asInteger(0, 0);
                return true;
            }

            bool configCols(string key, string value, Configuration.Options options)
            {
                return false;
            }

            bool showIcon_ = false;
            bool configShowIcon(string key, string value, Configuration.Options options)
            {
                showIcon_ = Configuration.asBoolean(value, false);
                return true;
            }

            bool showText_ = true;
            bool configShowText(string key, string value, Configuration.Options options)
            {
                showText_ = Configuration.asBoolean(value, true);
                return true;
            }

            bool autoScroll_ = true;
            int autoScrollLine_ = 0;
            int autoScrollInc_ = 1;
            bool configAutoScroll(string key, string value, Configuration.Options options)
            {
                autoScroll_ = Configuration.asBoolean(value, true);
                return true;
            }

            bool showBar_ = false;
            float barThickness_ = 0f;
            Color barBackground_ = Default.BarBackgroundColor;
            bool configShowBar(string key, string value, Configuration.Options options)
            {
                showBar_ = Configuration.asBoolean(value, false);

                if (options.Count > 0)
                {
                    barThickness_ = Configuration.asFloat(options[0], 0f);
                    barBackground_ = Configuration.asColor(options[1], Template.BackgroundColor);
                }
                return true;
            }

            bool showMissing_ = false;
            bool configShowMissing(string key, string value, Configuration.Options options)
            {
                showMissing_ = Configuration.asBoolean(value, false);
                return true;
            }
            #endregion // Configuration
        }
    }
}
