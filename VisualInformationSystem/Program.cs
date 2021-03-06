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
    partial class Program : MyGridProgram
    {
        const string VERSION = "0.72b";

        const string IconNameSquareSimple = "SquareSimple";
        const string IconNameCircle = "Circle";

        static Program App = null;
        VISManager Manager
        {
            get;
            set;
        }

        Statistics statistics_ = new Statistics();
        void registerException(Exception exp)
        {
            statistics_.registerException(exp);
        }

        static string removeFromEnd(string data, string search)
        {
            if (data.EndsWith(search))
                return data.Remove(data.Length - search.Length);
            return data;
        }

        public Program()
        {
            App = this;
            Manager = new VISManager();
            if (Manager.construct())
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            else
                Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Manager.onTick(argument, updateSource);
            statistics_.tick(this);
        }
    }
}
