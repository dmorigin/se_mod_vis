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
        public class VISManager
        {
            public VISManager()
            {
                config_ = new ConfigHandler(this);

                console_ = new Console();
                collectorManager_ = new DataCollectorManager();
                jobManager_ = new JobManager();
                templateManager_ = new TemplateManager();
                displayManager_ = new DisplayManager();
            }


            public bool construct()
            {
                if (console_.construct())
                {
                    // setup state handler
                    stateHandlers_[State.Run] = handleRunState;
                    stateHandlers_[State.Stopped] = handleStoppedState;
                    stateHandlers_[State.Init] = handleInitState;
                    stateHandlers_[State.Shutdown] = handleShutdownState;
                    stateHandlers_[State.Error] = handleErrorState;

                    // setup configuration
                    config_.add("displaytag", config_.configDisplayNameTag);
                    config_.add("template", config_.configTemplate);

                    if (collectorManager_.construct())
                    {
                        if (templateManager_.construct())
                        {
                            // construct job manager
                            if (jobManager_.construct())
                            {
                                // construct display manager
                                if (displayManager_.construct())
                                {
                                    log(Console.LogType.Info, "VIS Manager constructed");
                                    timer_.start();
                                    return true;
                                }
                                else
                                    log(Console.LogType.Error, "Failed to construct display manager");
                            }
                            else
                                log(Console.LogType.Error, "Failed to construct template manager");
                        }
                        else
                            log(Console.LogType.Error, "Failed to construct job manager");
                    }
                    else
                        log(Console.LogType.Error, "Failed to construct data collector manager");
                }

                return false;
            }

            JobManager jobManager_ = null;
            public JobManager JobManager
            {
                get { return jobManager_; }
            }

            TemplateManager templateManager_ = null;
            public TemplateManager TemplateManager
            {
                get { return templateManager_; }
            }

            DisplayManager displayManager_ = null;
            public DisplayManager DisplayManager
            {
                get { return displayManager_; }
            }

            DataCollectorManager collectorManager_ = null;
            public DataCollectorManager CollectorManager
            {
                get { return collectorManager_; }
            }

            Timer timer_ = new Timer();
            public Timer Timer
            {
                get { return timer_; }
            }

            List<DisplayProvider> displayProviders_ = new List<DisplayProvider>();

            #region Command line
            CommandLine cmdLine_ = new CommandLine();
            public CommandLine CommandLine
            {
                get { return cmdLine_; }
            }
            #endregion // Command line

            #region Console
            Console console_ = null;
            public Console Console
            {
                get { return console_; }
            }

            private void log(Console.LogType logType, string messsage)
            {
                console_.log(logType, messsage);
            }
            #endregion // Console

            #region Configuration
            ConfigHandler config_ = null;

            class ConfigHandler : Configuration.Handler
            {
                // default configuration
                public string vDisplayNameTag_ = Default.DisplayNameTag;


                VISManager manager_ = null;
                public ConfigHandler(VISManager manager)
                {
                    manager_ = manager;
                }


                public bool configDisplayNameTag(string key, string value, Configuration.Options options)
                {
                    if (value == "")
                        return false;

                    vDisplayNameTag_ = value;
                    return true;
                }


                public bool configTemplate(string key, string value, Configuration.Options options)
                {
                    // create new template

                    return true;
                }
            }
            #endregion // Configuration

            #region Application State System
            public enum State
            {
                Init,
                Run,
                Shutdown,
                Stopped,
                Error
            }

            public string stateToString(State state)
            {
                switch (state)
                {
                    case State.Run:
                        return "Run";
                    case State.Init:
                        return "Init";
                    case State.Shutdown:
                        return "Shutdown";
                    case State.Stopped:
                        return "Stopped";
                    case State.Error:
                        return "Error";
                }

                return "";
            }

            public UpdateFrequency UpdateFrequency
            {
                get { return App.Runtime.UpdateFrequency; }
                private set
                {
                    App.statistics_.setSensitivity(value);
                    App.Runtime.UpdateFrequency = value;
                }
            }

            #region State Handler
            delegate void StateDelegate();

            void handleStoppedState()
            {
                initStateStage_ = 0;
                dpIndex_ = 0;
                waitAfterInit_ = 6;

                if (reboot_)
                    switchState(State.Init);
                else
                    UpdateFrequency = UpdateFrequency.Update100;
            }

            int initStateStage_ = 0;
            int dpIndex_ = 0;
            int waitAfterInit_ = 6;
            void handleInitState()
            {
                if (initStateStage_ == 0)
                {
                    log(Console.LogType.Info, "Init system");
                    UpdateFrequency = UpdateFrequency.Update10;

                    // read configuration
                    bool readConfigFailed = false;
                    Configuration.Process(config_, App.Me, (key, value, options) =>
                    {
                        log(Console.LogType.Error, $"Read config: \"{key}\", \"{value}\"");
                        readConfigFailed = true;
                        return false;
                    });

                    if (!readConfigFailed)
                    {
                        log(Console.LogType.Info, "Init stage 1 successful");
                        initStateStage_ = 1;
                    }
                    else
                    {
                        log(Console.LogType.Error, "Failed to read configuration");
                        initStateStage_ = 99; // >98 is error state
                    }
                }
                else if (initStateStage_ == 1)
                {
                    // aquire displays
                    App.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(null, (block) =>
                    {
                        if (!block.IsSameConstructAs(App.Me) ||
                            !block.CustomName.Contains(config_.vDisplayNameTag_))
                            return false;

                        IMyTextSurfaceProvider provider = block as IMyTextSurfaceProvider;
                        if (provider != null)
                        {
                            // create display provider
                            DisplayProvider displayProvider = new DisplayProvider(block.CustomName, provider);

                            // add to list
                            displayProviders_.Add(displayProvider);
                        }

                        return false;
                    });

                    log(Console.LogType.Info, "Init stage 2 successful");
                    initStateStage_ = 2;
                    dpIndex_ = 0;
                }
                else if (initStateStage_ == 2)
                {
                    if (dpIndex_ >= displayProviders_.Count)
                    {
                        log(Console.LogType.Info, "Init stage 3 successful");
                        displayProviders_.Clear();
                        initStateStage_ = 98; // 98 is success state
                        return;
                    }

                    // init display providers
                    var provider = displayProviders_[dpIndex_];
                    if (!provider.Constructed)
                    {
                        if (!provider.construct())
                        {
                            log(Console.LogType.Error, $"Failed to construct provider {provider.Name}");
                            initStateStage_ = 99; // >98 is error state
                        }

                        return;
                    }
                    else
                        dpIndex_++;
                }
                else if (initStateStage_ == 98)
                {
                    if (--waitAfterInit_ == 0)
                    {
                        log(Console.LogType.Info, "VIS Manager initiated");
                        UpdateFrequency = UpdateFrequency.Update1;
                        switchState(State.Run);
                    }
                }
                else
                {
                    log(Console.LogType.Error, "Init runs into an error state");
                    switchState(State.Error);
                }
            }

            void handleRunState()
            {
                // process job manager
                jobManager_.tick(timer_.Delta);
            }

            void handleShutdownState()
            {
                log(Console.LogType.Info, "Shutdown system");
                UpdateFrequency = UpdateFrequency.Update100;

                // switch to stopped state
                switchState(State.Stopped);
            }

            void handleErrorState()
            {
                reboot_ = false;
                switchState(State.Shutdown);
            }
            #endregion // State Handler


            State currentState_ = State.Stopped;
            public State CurrentState
            {
                get { return currentState_; }
            }

            Dictionary<State, StateDelegate> stateHandlers_ = new Dictionary<State, StateDelegate>();
            bool reboot_ = true;


            public void switchState(State nextState)
            {
                log(Console.LogType.Info, $"Switch to state:{stateToString(nextState)}");
                currentState_ = nextState;
            }
            #endregion // Application State System


            public void onSave()
            {
            }


            public void onTick(string args, UpdateType updateSource)
            {
                try
                {
                    // update timer
                    timer_.update(App.Runtime.TimeSinceLastRun);

                    if ((updateSource & UpdateType.Terminal) != 0 ||
                        (updateSource & UpdateType.Trigger) != 0)
                    {
                        // process command line
                        //cmdLine_.process(args);
                    }

                    // process state
                    stateHandlers_[currentState_]();

                    // process console
                    console_.flush();
                }
                catch (Exception exp)
                {
                    log(Console.LogType.Error, "VIS run into an exception -> shutdown");
                    App.registerException(exp);
                    UpdateFrequency = UpdateFrequency.Update100;
                    switchState(State.Error);
                }
            }
        }
    }
}
