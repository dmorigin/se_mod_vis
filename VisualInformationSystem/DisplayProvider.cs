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
        public class DisplayProvider : VISObject
        {
            IMyTextSurfaceProvider surfaceProvider_ = null;


            public DisplayProvider(string name, IMyTextSurfaceProvider provider)
                : base($"Provider_{name}")
            {
                surfaceProvider_ = provider;
            }


            public override bool construct()
            {
                if (surfaceProvider_ == null)
                {
                    log(Console.LogType.Error, $"Missing SE surface provider");
                    return false;
                }

                IMyTerminalBlock block = surfaceProvider_ as IMyTerminalBlock;
                if (block != null)
                {
                    ConfigHandler config = new ConfigHandler(this);
                    if (Configuration.Process(config, block, (key, value, options) =>
                    {
                        log(Console.LogType.Error, $"Invalid display provider config: {key}, {value}");
                        return false;
                    }))
                    {
                        log(Console.LogType.Info, $"Provider({block.CustomName}) settings read");
                        Constructed = true;
                        return true;
                    }
                    else
                        log(Console.LogType.Error, $"Faild to read Provider({block.CustomName}) config");
                }
                else
                {
                    log(Console.LogType.Error, $"Invalid provider block");
                }

                log(Console.LogType.Error, "SE Terminal block invalid");
                return false;
            }


            class ConfigHandler : Configuration.Handler
            {
                DisplayProvider provider_ = null;
                public ConfigHandler(DisplayProvider provider)
                {
                    provider_ = provider;

                    add("display", configDisplay);
                }


                bool configDisplay(string key, string value, Configuration.Options options)
                {
                    // set surface index
                    int displayId = Configuration.asInteger(value, Program.Default.DisplayID);
                    string groupId = Program.Default.EmptyDisplayGroupID;
                    Vector2I coordinate = Program.Default.DisplayCoordinate;

                    // extract group id
                    if (options.Count == 2)
                    {
                        // extract group id
                        groupId = options[0];

                        // extract coordinates
                        coordinate = options.getAsVector(1, Program.Default.DisplayCoordinate);
                    }

                    Display display = null;

                    // create as group
                    if (groupId != Program.Default.EmptyDisplayGroupID)
                    {
                        display = provider_.Manager.DisplayManager.getDisplayGroup(groupId);
                        if (display == null)
                        {
                            provider_.log(Console.LogType.Info, $"Create new display group:{groupId}");
                            display = provider_.Manager.DisplayManager.createDisplay(groupId);
                            if (display == null)
                            {
                                provider_.log(Console.LogType.Error, $"Failed to create display group:{groupId}");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        display = provider_.Manager.DisplayManager.createDisplay(Program.Default.EmptyDisplayGroupID);
                        if (display == null)
                        {
                            provider_.log(Console.LogType.Error, $"Failed to create display");
                            return false;
                        }
                    }

                    // create render target to display
                    if (!display.addRenderTarget(provider_.surfaceProvider_.GetSurface(displayId), coordinate))
                    {
                        provider_.log(Console.LogType.Error, $"Render target exists: {groupId}:{coordinate}");
                        return false;
                    }

                    // configure display
                    if (coordinate == new Vector2I(0, 0))
                    {
                        Template template = provider_.Manager.TemplateManager.createTemplate(display.GroupID);
                        if (template == null)
                        {
                            provider_.log(Console.LogType.Error, "Failed to create template config handler");
                            return false;
                        }

                        provider_.log(Console.LogType.Debug, "set sub config handler");
                        setSubHandler(template.getConfigHandler());
                        display.Template = template;
                    }

                    return true;
                }
            }
        }
    }
}
