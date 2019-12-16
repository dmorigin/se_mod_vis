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
        public class Statistics
        {
            char[] runSymbol_ = { '-', '\\', '|', '/' };
            int runSymbolIndex_ = 0;
            char getRunSymbol()
            {
                char sym = runSymbol_[runSymbolIndex_++];
                if (runSymbolIndex_ >= 4)
                    runSymbolIndex_ = 0;
                return sym;
            }

            string stateToString(VISManager.State state)
            {
                switch (state)
                {
                    case VISManager.State.Run:
                        return "Run";
                    case VISManager.State.Init:
                        return "Init";
                    case VISManager.State.Shutdown:
                        return "Shutdown";
                    case VISManager.State.Stopped:
                        return "Stopped";
                    case VISManager.State.Error:
                        return "Error";
                }

                return "";
            }

            string exception_ = "";
            public void registerException(Exception exp)
            {
                exception_ = exp.ToString();
            }

            TimeSpan nextUpdate_ = new TimeSpan(0);
            TimeSpan updateInterval_ = TimeSpan.FromSeconds(2.0);
            TimeSpan ticks_ = new TimeSpan(0);

            long instructionCountLastUpdate_ = 0;
            long ticksSinceLastUpdate_ = 0;
            double timeSinceLastUpdate_ = 0.0;

            public void tick(Program app, VISManager vis)
            {
                ticks_ += app.Runtime.TimeSinceLastRun;

                // update
                instructionCountLastUpdate_ += app.Runtime.CurrentInstructionCount;
                timeSinceLastUpdate_ += app.Runtime.LastRunTimeMs;
                ticksSinceLastUpdate_++;

                if (nextUpdate_ <= ticks_)
                {
                    int jobQueuedExec = 0;
                    int jobQueued = 0;
                    int jobTimed = 0;
                    vis.JobManager.getStatistic(ref jobTimed, ref jobQueued, ref jobQueuedExec);

                    // print statistic
                    string msg = "Visual Information System\n=============================\n";
                    msg += $"Running: {getRunSymbol()}\n";
                    msg += $"Statistic Interval: {updateInterval_.Seconds}s\n";
                    msg += $"VIS State: {stateToString(vis.CurrentState)}\n";
                    msg += $"Time: {ticks_}\n";
                    msg += $"Ticks: {ticksSinceLastUpdate_}\n";
                    msg += $"Avg Time/tick: {(timeSinceLastUpdate_ / ticksSinceLastUpdate_).ToString("#0.0#####")}ms\n";
                    msg += $"Avg Inst/tick: {(instructionCountLastUpdate_ / (double)ticksSinceLastUpdate_).ToString("#0.0###")}\n";
                    msg += $"Job (Timed): {jobTimed}\n";
                    msg += $"Job (Queue/Exec): {jobQueued}/{jobQueuedExec}\n";

                    if (exception_ != "")
                        msg += $"\nException:\n{exception_}\n";

                    app.Echo(msg);

                    nextUpdate_ = ticks_ + updateInterval_;
                    instructionCountLastUpdate_ = 0;
                    ticksSinceLastUpdate_ = 0;
                    timeSinceLastUpdate_ = 0.0;
                }
            }
        }
    }
}
