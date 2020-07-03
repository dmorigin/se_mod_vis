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
        /*public class TemplateManager : VISObject
        {
            public TemplateManager()
                : base("TemplateManager")
            {
            }

            public override bool construct()
            {
                // create default templates
                createDefaultTemplate("Base", "font:debug:0.8:179,237,255\nalignment:left\nbgcolor:0,88,151");
                createDefaultTemplate("BaseBlack", "font:debug:0.8:255,255,255\nalignment:left\nbgcolor:0,0,0");
                createDefaultTemplate("Transparent", "font:debug:0.8:0,88,151\nbgcolor:0,0,0");

                return base.construct();
            }

            List<ContentContainer> templates_ = new List<ContentContainer>();

            public ContentContainer createDefaultTemplate(string name, string config)
            {
                ContentContainer template = createTemplate($"Default_{name}");
                if (template != null)
                {
                    Configuration.Handler handler = template.getConfigHandler();
                    if (!Configuration.Process(handler, config, false, (key, value, options) =>
                    {
                        log(Console.LogType.Error, $"Read config: \"{key}\", \"{value}\"");
                        return false;
                    }))
                    {
                        log(Console.LogType.Error, $"Failed to read template configuration");
                        return null;
                    }

                    saveTemplate(template);
                    return template;
                }

                return null;
            }

            public ContentContainer createTemplate(string name)
            {
                if (name == string.Empty)
                {
                    log(Console.LogType.Error, $"Invalid template name. Failed to create.");
                    return null;
                }

                ContentContainer template = new ContentContainer(this, name);
                if (template.construct())
                    return template;

                log(Console.LogType.Error, $"Failed to construct template {name}");
                return null;
            }

            public ContentContainer loadTemplate(string name)
            {
                var tpl = templates_.Find(x => x.TemplateName == name);
                return tpl != null ? tpl : null;
            }

            public bool saveTemplate(ContentContainer template)
            {
                if (loadTemplate(template.TemplateName) == null)
                {
                    log(Console.LogType.Info, $"Template {template.TemplateName} stored");
                    templates_.Add(template);
                    return true;
                }

                return false;
            }
        }*/
    }
}
