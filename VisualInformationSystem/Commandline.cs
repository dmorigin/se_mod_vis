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
        public class CommandLine
        {
            public delegate bool SwitchHandler(CommandLine cmdLine, string name);


            MyCommandLine parser_ = new MyCommandLine();
            List<SwitchHandler> handler_ = new List<SwitchHandler>();


            public CommandLine()
            {
            }


            public void registerHandler(SwitchHandler handler)
            {
                handler_.Add(handler);
            }


            public void unregisterHandler(SwitchHandler handler)
            {
                handler_.Remove(handler);
            }


            public string switchArgument(string name, int index)
            {
                if (parser_.Switch(name) == true)
                    return parser_.Switch(name, index);
                return "";
            }


            public int switchArgumentInt(string name, int index)
            {
                string arg = switchArgument(name, index);
                int value = 0;
                int.TryParse(arg, out value);
                return value;
            }


            public float switchArgumentFloat(string name, int index)
            {
                string arg = switchArgument(name, index);
                float value = 0;
                float.TryParse(arg, out value);
                return value;
            }


            public void process(string args)
            {
                if (parser_.TryParse(args) == true)
                {
                    // step through all switches
                    foreach (var name in parser_.Switches)
                    {
                        // process a single switch
                        foreach (var handler in handler_)
                        {
                            if (handler(this, name) == true)
                                break;
                        }
                    }
                }
            }
        }
    }
}
