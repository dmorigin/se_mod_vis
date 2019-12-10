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

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                Vector2 fontSize = showText_ == true ? rt.FontSize * Template.FontSize : new Vector2(0);
                Vector2 barSize = showBar_ ? new Vector2(size.X, barHight_ == 0f ? fontSize.Y : barHight_) : new Vector2();
                float lineHeight = fontSize.Y + barSize.Y;
                int lines = lines_;

                if (lines <= 0)
                    lines = (int)(size.Y / lineHeight) + 1;
                else
                {
                    float scale = (size.Y / lines) / lineHeight;

                    fontSize *= scale;
                    barSize *= scale;
                    lineHeight = fontSize.Y + barSize.Y;
                }

                if (showIcon_ && !showText_)
                    barSize.X -= barSize.Y;

                float textPositionY = position.Y - (size.Y * 0.5f) + (fontSize.Y * 0.5f);
                float barPositionY = position.Y - (size.Y * 0.5f) + (barSize.Y * 0.5f) + fontSize.Y;
                float iconPositionY = 0f;

                // icon
                Vector2 iconSize = new Vector2();
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
                float textLeftPositionX = position.X - (size.Y * 0.5f);
                float textRightPositionX = position.X + (size.Y * 0.5f);

                // render name
                List<DataRetriever.ListContainer> container;
                DataRetriever.getList(out container);
                foreach(var entry in container)
                {
                    // draw icon
                    if (showIcon_)
                    {
                        string iconType = $"{entry.type.TypeId}/{entry.type.SubtypeId}";
                        if (rt.spriteExist(iconType))
                        {
                            MySprite icon = new MySprite(SpriteType.TEXTURE, iconType, new Vector2(iconPositionX, iconPositionY), iconSize, Color.White);
                            addSprite(icon);
                            iconPositionY += lineHeight;
                        }
                    }

                    // draw bar
                    if (showBar_)
                    {
                        renderBar(rt, addSprite, new Vector2(barPositionX, barPositionY), barSize, true, (float)entry.indicator, Gradient, barBackground_);
                        barPositionY += lineHeight;
                    }

                    // draw text
                    if (showText_)
                    {
                        renderTextLine(rt, addSprite, Template.Font, Template.FontSize, new Vector2(textLeftPositionX, textPositionY),
                            Template.FontColor, entry.name, TextAlignment.LEFT);
                        renderTextLine(rt, addSprite, Template.Font, Template.FontSize, new Vector2(textRightPositionX, textPositionY),
                            Template.FontColor, $"{entry.value}", TextAlignment.RIGHT);

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
                    config.add("lines", configLines);
                    config.add("showicon", configShowIcon);
                    config.add("showtext", configShowText);
                    config.add("showbar", configShowBar);
                    config.add("autoscroll", configAutoScroll);
                }

                return config;
            }

            int lines_ = 0; // if 0 the amount of lines will be automaticaly calculated
            bool configLines(string key, string value, Configuration.Options options)
            {
                lines_ = Configuration.asInteger(value, 0);
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
            bool configAutoScroll(string key, string value, Configuration.Options options)
            {
                autoScroll_ = Configuration.asBoolean(value, true);
                return true;
            }

            bool showBar_ = false;
            float barHight_ = 10f;
            Color barBackground_ = Color.Black;
            bool configShowBar(string key, string value, Configuration.Options options)
            {
                showBar_ = Configuration.asBoolean(value, false);

                if (options.Count > 0)
                {
                    barHight_ = Configuration.asFloat(options[0], 10f);
                    barBackground_ = Configuration.asColor(options[1], Color.Black);
                }
                return true;
            }
            #endregion // Configuration
        }
    }
}
