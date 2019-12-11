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
        public class Template : VISObject
        {
            public Template(TemplateManager templateManager, string name)
                : base($"Template_{name}")
            {
                templateManager_ = templateManager;
                templateName_ = name;
            }

            public override bool construct()
            {
                log(Console.LogType.Info, "Construct template manager");
                return base.construct();
            }

            public virtual Settings getConfigHandler()
            {
                return new Settings(this);
            }

            #region Properties
            TemplateManager templateManager_ = null;
            public TemplateManager TemplateManager
            {
                get { return templateManager_; }
            }

            string templateName_ = "";
            public string TemplateName
            {
                get { return templateName_; }
                set { templateName_ = value; }
            }

            string useTemplate_ = "";
            public string UseTemplate
            {
                get { return useTemplate_; }
                protected set { useTemplate_ = value; }
            }

            TimeSpan updateInterval_ = Program.Default.UpdateInterval;
            public TimeSpan UpdateInterval
            {
                get { return updateInterval_; }
                protected set { updateInterval_ = value; }
            }

            Color backgroundColor_ = Program.Default.BackgroundColor;
            public Color BackgroundColor
            {
                get { return backgroundColor_; }
                protected set { backgroundColor_ = value; }
            }

            string font_ = Program.Default.Font;
            public string Font
            {
                get { return font_; }
                protected set { font_ = value; }
            }

            float fontSize_ = Program.Default.FontSize;
            public float FontSize
            {
                get { return fontSize_; }
                protected set { fontSize_ = value; }
            }

            Color fontColor_ = Program.Default.FontColor;
            public Color FontColor
            {
                get { return fontColor_; }
                protected set { fontColor_ = value; }
            }

            TextAlignment alignment_ = Program.Default.FontAlignment;
            public TextAlignment TextAlignment
            {
                get { return alignment_; }
                protected set { alignment_ = value; }
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
                    template_ = template;

                    // fill up handler
                    add("usetemplate", configUseTemplate);
                    add("updateinterval", configUpdateInterval);
                    add("backgroundcolor", configBackgroundColor);
                    add("font", configFont);
                    add("alignment", configAlignment);
                    add("graphic", configGraphic);
                }

                Template template_ = null;
                public Template Template
                {
                    get { return template_; }
                }

                #region Handler
                bool configUseTemplate(string key, string value, Configuration.Options options)
                {
                    if (value != string.Empty)
                    {
                        Template.UseTemplate = value;
                        return true;
                    }
                    return false;
                }

                bool configUpdateInterval(string key, string value, Configuration.Options options)
                {
                    float seconds = Configuration.asFloat(value, Program.Default.UpdateIntervalInSec);
                    Template.UpdateInterval = TimeSpan.FromSeconds(seconds);
                    return true;
                }

                bool configBackgroundColor(string key, string value, Configuration.Options options)
                {
                    Template.BackgroundColor = Configuration.asColor(value, Program.Default.BackgroundColor);
                    return true;
                }

                bool configFont(string key, string value, Configuration.Options options)
                {
                    Template.Font = value != string.Empty ? value : Program.Default.Font;
                    Template.FontSize = options.getAsFloat(0, Program.Default.FontSize);
                    Template.FontColor = options.getAsColor(1, Program.Default.FontColor);
                    return true;
                }

                bool configAlignment(string key, string value, Configuration.Options options)
                {
                    string data = value.ToLower();
                    switch (data)
                    {
                        case "center":
                        case "c":
                            Template.TextAlignment = TextAlignment.CENTER;
                            break;
                        case "left":
                        case "l":
                            Template.TextAlignment = TextAlignment.LEFT;
                            break;
                        case "right":
                        case "r":
                            Template.TextAlignment = TextAlignment.RIGHT;
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
                            graphic = new GraphicText(Template, options);
                            break;
                        case "battery":
                            graphic = new GraphicBattery(Template, options);
                            break;
                        case "list":
                            graphic = new GraphicList(Template, options);
                            break;
                    }

                    if (graphic != null)
                    {
                        setSubHandler(graphic.getConfigHandler());
                        template_.graphics_.Add(graphic);
                        return true;
                    }

                    Template.log(Console.LogType.Error, $"Invalid graphic type {value}");
                    return false;
                }
                #endregion // Handler
            }
        }
    }
}
