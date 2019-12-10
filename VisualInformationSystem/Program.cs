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
    partial class Program : MyGridProgram
    {
        protected static Program App = null;


        VISManager manager_ = null;
        public VISManager Manager
        {
            get { return manager_; }
        }


        public Program()
        {
            Program.App = this;
            manager_ = new VISManager(this);
            if (manager_.construct())
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            else
                Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        public void Save()
        {
            if (manager_ != null)
                manager_.onSave();
        }


        char[] runSymbol_ = { '-', '\\', '|', '/' };
        int runSymbolPos_ = 0;
        int nextUpdate = 0;

        string messages_ = "";
        void addEchoMessageLine(string line)
        {
            messages_ += line.Trim() + "\n";
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (manager_ != null)
                manager_.onTick(argument, updateSource);

            if (nextUpdate-- <= 0)
            {
                string msg = "Visual Information System\n===========================\n";
                msg += $"Running: {runSymbol_[runSymbolPos_++]}\n";
                msg += $"State: {Manager.CurrentState}\n";
                msg += $"Messages:\n{messages_}";

                Echo(msg);
                if (runSymbolPos_ >= 4)
                    runSymbolPos_ = 0;

                nextUpdate = 0;
            }
        }
    }
}
