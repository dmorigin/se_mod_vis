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
        public class TemplateManager : VISObject
        {
            public TemplateManager()
                : base("TemplateManager")
            {
            }


            List<Template> templates_ = new List<Template>();


            public Template createFromConfig(IMyTerminalBlock block)
            {
                return createFromConfig(block.CustomData);
            }


            public Template createFromConfig(string config)
            {
                Template template = createTemplate((templates_.Count + 1).ToString());

                // read config

                return template;
            }


            public Template createTemplate(string name)
            {
                if (name == string.Empty)
                {
                    log(Console.LogType.Error, $"Invalid template name. Failed to create.");
                    return null;
                }

                Template template = new Template(this, name);
                if (template.construct())
                    return template;

                log(Console.LogType.Error, $"Failed to construct template {name}");
                return null;
            }


            public Template loadTemplate(string name)
            {
                foreach(var template in templates_)
                {
                    if (template.TemplateName == name)
                        return template;
                }

                return null;
            }


            public bool saveTemplate(Template template)
            {
                if (loadTemplate(template.TemplateName) == null)
                {
                    templates_.Add(template);
                    return true;
                }

                return false;
            }
        }
    }
}
