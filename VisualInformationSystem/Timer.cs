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
        public class Timer
        {
            TimeSpan ticks_ = new TimeSpan();
            TimeSpan delta_ = new TimeSpan();
            bool stopped_ = true;
            bool paused_ = false;


            public Timer()
            {
            }


            public void stop()
            {
                stopped_ = true;
                ticks_ = new TimeSpan();
                delta_ = new TimeSpan();
            }


            public void start()
            {
                stopped_ = false;
            }


            public void pause()
            {
                paused_ = true;
            }


            public void resume()
            {
                paused_ = false;
            }


            public void update(TimeSpan elapsed)
            {
                if (!stopped_ && !paused_)
                {
                    TimeSpan prev = ticks_;
                    ticks_ += elapsed;
                    delta_ = ticks_ - prev;
                }
            }


            public bool Paused
            {
                get { return paused_; }
            }


            public bool Stopped
            {
                get { return stopped_; }
            }


            public TimeSpan Delta
            {
                get
                {
                    return delta_;
                }
            }


            public TimeSpan Ticks
            {
                get
                {
                    return ticks_;
                }
            }
        }
    }
}
