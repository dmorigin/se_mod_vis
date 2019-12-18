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
                gfx.DataRetriever = DataRetriever;
                gfx.DataRetrieverName = DataRetrieverName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.lines_ = lines_;
                gfx.showBar_ = showBar_;
                gfx.showIcon_ = showIcon_;
                gfx.showText_ = showText_;
                gfx.barBackground_ = barBackground_;
                gfx.barHeight_ = barHeight_;
                gfx.autoScroll_ = autoScroll_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                Vector2 fontSize = showText_ == true ? display.FontSize * Template.FontSize : new Vector2(0);
                Vector2 barSize = showBar_ ? new Vector2(size.X, barHeight_ == 0f ? fontSize.Y : barHeight_) : new Vector2();
                float spacing = spacing_;
                float lineHeight = fontSize.Y + barSize.Y + spacing;

                if (lines_ > 0)
                {
                    float scale = (size.Y / lines_) / lineHeight;

                    fontSize *= scale;
                    barSize *= scale;
                    lineHeight = fontSize.Y + barSize.Y + spacing;
                }

                if (showIcon_ && !showText_)
                    barSize.X -= barSize.Y;

                int lines = (int)(size.Y / lineHeight);
                float textPositionY = position.Y - (size.Y * 0.5f) + (fontSize.Y * 0.5f);
                float barPositionY = position.Y - (size.Y * 0.5f) + (barSize.Y * 0.5f) + fontSize.Y;
                float iconPositionY = 0f;

                // icon
                Vector2 iconSize = new Vector2(0f, 0f);
                if (showIcon_)
                {
                    // adjust bar size if no text
                    if (showText_ == false)
                    {
                        barSize.X -= barSize.Y;
                        iconSize = new Vector2(barSize.Y, barSize.Y);
                    }
                    else
                        iconSize = new Vector2(fontSize.Y, fontSize.Y);

                    iconPositionY = position.Y - (size.Y * 0.5f) + (iconSize.Y * 0.5f);
                }

                float iconPositionX = position.X - (size.X * 0.5f) + (iconSize.X * 0.5f);
                float barPositionX = position.X - (size.X * 0.5f) + (barSize.X * 0.5f);
                float textLeftPositionX = position.X - (size.Y * 0.5f) + iconSize.X;
                float textRightPositionX = position.X + (size.Y * 0.5f);

                List<DataRetriever.ListContainer> container;
                DataRetriever.list(out container, (item) => showMissing_ || item.value > 0);

                // auto scroll
                if (autoScroll_ == true)
                {
                    if (lines >= container.Count)
                        autoScrollLine_ = 0;
                    else
                    {
                        autoScrollLine_ += autoScrollInc_;

                        // toggle inc
                        if (autoScrollLine_ >= (container.Count - lines) || autoScrollLine_ < 0)
                        {
                            autoScrollInc_ *= -1;
                            autoScrollLine_ += autoScrollInc_;
                        }
                    }
                }

                if (Gradient.Count == 0)
                    addGradientColor(0.0f, Template.FontColor);

                // render name
                for (int l = autoScrollLine_; l < (lines + autoScrollLine_) && l < container.Count; l++)
                {
                    var entry = container[l];
                    if (entry.value == 0.0 && !showMissing_)
                        continue;

                    // draw icon
                    if (showIcon_)
                    {
                        string iconType = $"{entry.type.TypeId}/{entry.type.SubtypeId}";
                        if (RenderTarget.spriteExist(iconType))
                        {
                            MySprite icon = new MySprite(SpriteType.TEXTURE, iconType, new Vector2(iconPositionX, iconPositionY), iconSize, Color.White);
                            addSprite(icon);
                            iconPositionY += lineHeight;
                        }
                    }

                    // draw bar
                    if (showBar_)
                    {
                        renderBar(addSprite, new Vector2(barPositionX, barPositionY), barSize, false, (float)entry.indicator, Gradient, barBackground_);
                        barPositionY += lineHeight;
                    }

                    // draw text
                    if (showText_)
                    {
                        string rightText = $"{entry.value.pack()}/{entry.max.pack()}";

                        renderTextLine(display, rt, addSprite, Template.Font, Template.FontSize, new Vector2(textLeftPositionX, textPositionY),
                            getGradientColor((float)entry.indicator), entry.name, TextAlignment.LEFT);
                        renderTextLine(display, rt, addSprite, Template.Font, Template.FontSize, new Vector2(textRightPositionX, textPositionY),
                            getGradientColor((float)entry.indicator), rightText, TextAlignment.RIGHT);

                        textPositionY += lineHeight;
                    }
                }
            }

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
                return true;
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
            float barHeight_ = 0f;
            Color barBackground_ = Program.Default.BarBackgroundColor;
            bool configShowBar(string key, string value, Configuration.Options options)
            {
                showBar_ = Configuration.asBoolean(value, false);

                if (options.Count > 0)
                {
                    barHeight_ = Configuration.asFloat(options[0], 0f);
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
