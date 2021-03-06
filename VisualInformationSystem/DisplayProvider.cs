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
        public class DisplayProvider : VISObject
        {
            IMyTextSurfaceProvider surfaceProvider_ = null;
            string config_ = "";
            string customName_ = "";

            public DisplayProvider(string name, IMyTextSurfaceProvider provider, string config = "")
                : base($"Provider_{name}")
            {
                surfaceProvider_ = provider;
                config_ = config;
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
                    customName_ = block.CustomName.Trim().Replace(" ", "");
                    ConfigHandler config = new ConfigHandler(this);
                    if (Configuration.Process(config, config_ != "" ? config_ : block.CustomData, 
                        config_ != "" ? false : Default.ShareCustomData, (key, value, options) =>
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
                    log(Console.LogType.Error, $"Invalid provider block");

                return false;
            }


            class ConfigHandler : Configuration.Handler
            {
                DisplayProvider provider_ = null;
                public ConfigHandler(DisplayProvider provider)
                {
                    provider_ = provider;

                    add("display", configDisplay);
                    add("screen", configScreen);
                }


                bool configDisplay(string key, string value, Configuration.Options options)
                {
                    provider_.log(Console.LogType.Error, "Config display is depricated. Use 'screen' instead!");
                    return configScreen(key, value, options);
                }


                bool configScreen(string key, string value, Configuration.Options options)
                {
                    // set surface index
                    int screenId = Configuration.asInteger(value, Default.DisplayID);
                    string groupId = Default.EmptyDisplayGroupID;
                    Vector2I coordinate = Default.DisplayCoordinate;

                    // extract group id
                    if (options.Count == 2)
                    {
                        // extract group id
                        groupId = options[0];

                        // extract coordinates
                        coordinate = options.asVector(1, Default.DisplayCoordinate);
                    }

                    if (screenId < 0 || screenId >= provider_.surfaceProvider_.SurfaceCount)
                    {
                        provider_.log(Console.LogType.Error, $"Invalid display id: {screenId}");
                        return false;
                    }

                    Display display = Display.createDisplay(groupId, screenId, provider_.customName_);
                    if (display == null)
                        return false;

                    // create render target to display
                    RenderTargetID RTID = RenderTargetID.fromSurfaceProvider(provider_.surfaceProvider_, screenId);
                    if (!display.addRenderTarget(provider_.surfaceProvider_.GetSurface(screenId), RTID, coordinate))
                    {
                        provider_.log(Console.LogType.Error, $"Render target exists: {groupId}:{coordinate}");
                        return false;
                    }

                    // configure display
                    if (coordinate == new Vector2I(0, 0))
                    {
                        // setup display text
                        IMyTextPanel lcdPanel = provider_.surfaceProvider_ as IMyTextPanel;
                        if (lcdPanel != null)
                            display.PanelConnector = new Display.PanelConnectorObj(lcdPanel);

                        // create content container
                        ContentContainer container = new ContentContainer(display.GroupId);
                        setSubHandler(container.getConfigHandler());
                        display.ContentContainer = container;
                    }

                    return true;
                }
            }
        }
    }
}
