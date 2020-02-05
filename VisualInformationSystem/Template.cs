﻿using Sandbox.Game.EntityComponents;
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
        public class Template : VISObject
        {
            public Template(TemplateManager templateManager, string name)
                : base($"Template_{name}")
            {
                TemplateManager = templateManager;
                TemplateName = name;

                // setup defaults
                Refresh = Default.Refresh;
                BackgroundColor = Default.BackgroundColor;
                Font = Default.Font;
                FontSize = Default.FontSize;
                FontColor = Default.FontColor;
                TextAlignment = Default.FontAlignment;
            }

            public virtual Settings getConfigHandler()
            {
                return new Settings(this);
            }

            public virtual void merge(Template template)
            {
                Refresh = template.Refresh;
                BackgroundColor = template.BackgroundColor;
                Font = template.Font;
                FontSize = template.FontSize;
                FontColor = template.FontColor;
                TextAlignment = template.TextAlignment;

                foreach (var gfx in template.graphics_)
                    graphics_.Add(gfx.clone());
                graphics_.OrderBy(x => x.ZPosition);
            }

            #region Properties
            public TemplateManager TemplateManager
            {
                get;
                private set;
            }

            public string TemplateName
            {
                get;
                set;
            }

            public TimeSpan Refresh
            {
                get;
                protected set;
            }

            public Color BackgroundColor
            {
                get;
                protected set;
            }

            public string Font
            {
                get;
                protected set;
            }

            public float FontSize
            {
                get;
                protected set;
            }

            public Color FontColor
            {
                get;
                protected set;
            }

            public TextAlignment TextAlignment
            {
                get;
                protected set;
            }

            List<Graphic> graphics_ = new List<Graphic>();
            public Graphic getGraphic(int index)
            {
                if (index < graphics_.Count)
                    return graphics_[index];
                return null;
            }

            public IEnumerable<Graphic> getGraphics()
            {
                return graphics_.AsReadOnly();
            }

            public int Graphics
            {
                get { return graphics_.Count; }
            }
            #endregion // Properties

            public class Settings : Configuration.Handler
            {
                public Settings(Template template)
                {
                    tpl_ = template;

                    // fill up handler
                    add("usetemplate", configUseTemplate);
                    add("refresh", configRefresh);
                    add("bgcolor", configBackgroundColor);
                    add("font", configFont);
                    add("alignment", configAlignment);
                    add("graphic", configGraphic);
                }

                Template tpl_ = null;

                #region Handler
                bool configUseTemplate(string key, string value, Configuration.Options options)
                {
                    if (value != string.Empty)
                    {
                        Template template = tpl_.TemplateManager.loadTemplate(value);
                        if (template == null)
                        {
                            tpl_.log(Console.LogType.Error, $"Invalid template name:{value}");
                            return false;
                        }

                        tpl_.merge(template);
                        return true;
                    }
                    return false;
                }

                bool configRefresh(string key, string value, Configuration.Options options)
                {
                    float seconds = Configuration.asFloat(value, Default.RefreshInSec);
                    tpl_.Refresh = TimeSpan.FromSeconds(seconds);
                    return true;
                }

                bool configBackgroundColor(string key, string value, Configuration.Options options)
                {
                    tpl_.BackgroundColor = Configuration.asColor(value, Default.BackgroundColor);
                    return true;
                }

                bool configFont(string key, string value, Configuration.Options options)
                {
                    tpl_.Font = value != string.Empty ? value : Default.Font;
                    tpl_.FontSize = options.asFloat(0, Default.FontSize);
                    tpl_.FontColor = options.asColor(1, Default.FontColor);
                    return true;
                }

                bool configAlignment(string key, string value, Configuration.Options options)
                {
                    string data = value.ToLower();
                    switch (data)
                    {
                        case "center":
                        case "c":
                            tpl_.TextAlignment = TextAlignment.CENTER;
                            break;
                        case "left":
                        case "l":
                            tpl_.TextAlignment = TextAlignment.LEFT;
                            break;
                        case "right":
                        case "r":
                            tpl_.TextAlignment = TextAlignment.RIGHT;
                            break;
                        default:
                            return false;
                    }

                    return true;
                }

                bool configGraphic(string key, string value, Configuration.Options options)
                {
                    Graphic graphic = null;

                    switch (value.ToLower())
                    {
                        case "text":
                            graphic = new GraphicText(tpl_, options);
                            break;
                        case "battery":
                            graphic = new GraphicBattery(tpl_, options);
                            break;
                        case "list":
                            graphic = new GraphicList(tpl_, options);
                            break;
                        case "bar":
                            graphic = new GraphicBar(tpl_, options);
                            break;
                        case "icon":
                            graphic = new GraphicIcon(tpl_, options);
                            break;
                        case "slider":
                            graphic = new GraphicSlider(tpl_, options);
                            break;
                        case "test":
                            graphic = new GraphicTest(tpl_, options);
                            break;
                        case "curvedbar":
                            graphic = new GraphicCurvedBar(tpl_, options);
                            break;
                    }

                    if (graphic != null)
                    {
                        if (graphic.construct())
                        {
                            setSubHandler(graphic.getConfigHandler());
                            tpl_.graphics_.Add(graphic);
                            tpl_.graphics_.OrderBy(x => x.ZPosition);
                            return true;
                        }
                        else
                            tpl_.log(Console.LogType.Error, $"Failed to construct graphic:{value}");
                    }
                    else
                        tpl_.log(Console.LogType.Error, $"Invalid graphic type:{value}");
                    return false;
                }
                #endregion // Handler
            }
        }
    }
}
