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
        public class DisplayManager : VISObject
        {
            public DisplayManager()
                : base("DisplayManager")
            {
            }


            List<Display> displays_ = new List<Display>();

            static int genericDisplayGroupId_ = 0;
            public Display createDisplay(string groupId)
            {
                Display display;

                if (groupId == Program.Default.EmptyDisplayGroupID)
                {
                    groupId = $"genericDisplayGroup_{++genericDisplayGroupId_}";
                    log(Console.LogType.Info, $"Create new display: group({groupId})");
                    display = new Display(groupId);
                }
                else if (getDisplayGroup(groupId) == null)
                {
                    log(Console.LogType.Info, $"Create new display: group({groupId})");
                    display = new Display(groupId);
                }
                else
                {
                    log(Console.LogType.Error, $"Multiple display group detected: {groupId}");
                    return null;
                }

                display.construct();
                return display;
            }


            public bool addDisplay(Display display)
            {
                if (getDisplayGroup(display.GroupID) == null)
                {
                    displays_.Add(display);
                    return true;
                }

                return false;
            }


            public Display getDisplayGroup(string id)
            {
                foreach(var display in displays_)
                {
                    if (display.GroupID == id)
                        return display;
                }

                return null;
            }
        }
    }
}
