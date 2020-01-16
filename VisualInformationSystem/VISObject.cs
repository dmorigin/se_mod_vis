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
                Id = VISObject.nextId_++;
                Constructed = false;

                if (name == string.Empty)
                    Name = $"ModObject_{Id}";
                else
                    Name = name;
            }

            protected Program App
            {
                get
                {
                    return Program.App;
                }
            }

            public int Id
            {
                get;
                private set;
            }

            public string Name
            {
                get;
                private set;
            }

            public VISManager Manager
            {
                get { return App.Manager; }
            }

            public void log(Console.LogType logType, string message)
            {
                Manager.Console.log(logType, message);
            }

            public bool Constructed
            {
                get;
                protected set;
            }

            public virtual bool construct()
            {
                Constructed = true;
                return true;
            }

            public virtual Job getConstructionJob()
            {
                return new ConstructJob(this);
            }

            public class ConstructJob : Job
            {
                public ConstructJob(VISObject obj)
                {
                    obj_ = obj;
                }

                VISObject obj_ = null;

                public override void prepareJob()
                {
                    obj_.Constructed = false;
                    JobFinished = false;
                }

                public override void tick(TimeSpan delta)
                {
                    if (!obj_.construct())
                    {
                        log(Console.LogType.Error, $"Construction job failed");
                        Manager.switchState(VISManager.State.Error);
                    }

                    JobFinished = obj_.Constructed;
                }
            }
        }
    }
}
