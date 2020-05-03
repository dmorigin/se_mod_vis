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
        public interface IDataCollector
        {
            bool reconstruct();
            void prepareUpdate();
            void finalizeUpdate();

            void queueJob();

            bool UpdateFinished { get; }
            string CollectorTypeName { get; }
            Configuration.Options Options { get; }
            string TypeID { get; }
            TimeSpan MaxUpdateInterval { get; set; }

            string getVariable(string data);
            bool isSameCollector(string name, Configuration.Options options, string connector);

            DataAccessor getDataAccessor(string name);
        }
    }
}
