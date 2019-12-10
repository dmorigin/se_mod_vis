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
        public class VISObject
        {
            private static int nextId_ = 1;

            


            public VISObject(string name = "")
            {
                id_ = VISObject.nextId_++;

                if (name == string.Empty)
                    name_ = $"ModObject_{id_}";
                else
                    name_ = name;
            }


            public Program App
            {
                get
                {
                    return Program.App;
                }
            }


            public void Echo(string line)
            {
                App.addEchoMessageLine(line);
            }


            private int id_ = 0;
            public int Id
            {
                get { return id_; }
            }


            private string name_ = "";
            public string Name
            {
                get { return name_; }
            }


            public VISManager Manager
            {
                get { return App.Manager; }
            }


            public void log(Console.LogType logType, string message)
            {
                Manager.Console.log(logType, message);
            }


            public void log(string message)
            {
                Manager.Console.log(Console.LogType.Info, message);
            }


            bool constructed_ = false;
            public bool Constructed
            {
                get { return constructed_; }
                protected set { constructed_ = value; }
            }

            public virtual bool construct()
            {
                constructed_ = true;
                return true;
            }
        }
    }
}
