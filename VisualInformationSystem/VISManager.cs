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
                CurrentState = State.Stopped;

                Console = new Console();
                Timer = new Timer();
                CollectorManager = new DataCollectorManager();
                JobManager = new JobManager();
                //TemplateManager = new TemplateManager();
            }


            public bool construct()
            {
                // setup state handler
                stateHandlers_[State.Run] = handleRunState;
                stateHandlers_[State.Stopped] = handleStoppedState;
                stateHandlers_[State.Init] = handleInitState;
                stateHandlers_[State.Shutdown] = handleShutdownState;
                stateHandlers_[State.Error] = handleErrorState;

                Console.construct();
                CollectorManager.construct();
                //TemplateManager.construct();

                // construct job manager
                if (JobManager.construct())
                {
                    Timer.start();
                    return true;
                }
                else
                    log(Console.LogType.Error, "Failed to construct job manager");

                return false;
            }

            public JobManager JobManager
            {
                get;
                private set;
            }

            /*public TemplateManager TemplateManager
            {
                get;
                private set;
            }*/

            public DataCollectorManager CollectorManager
            {
                get;
                private set;
            }

            public Timer Timer
            {
                get;
                private set;
            }

            List<DisplayProvider> displayProviders_ = new List<DisplayProvider>();

            #region Console
            public Console Console
            {
                get;
                set;
            }
            public void log(Console.LogType logType, string messsage)
            {
                Console.log(logType, messsage);
            }
            #endregion // Console

            #region Configuration
            ConfigHandler config_ = null;

            class ConfigHandler : Configuration.Handler
            {
                VISManager manager_ = null;
                public ConfigHandler(VISManager manager)
                {
                    manager_ = manager;

                    // setup configuration
                    add("displaytag", configDisplayNameTag);
                    //add("template", configTemplate);
                    add("console", configConsole);
                    add("rtfixsize", configRTFixSize);
                    add("maxspeed", configMaxSpeed);
                    add("itemamount", configMaxAmountItem);
                    add("recointerval", configSetRecInterval);
                    add("shareconfig", configShareConfig);

                    remove("*");
                    add("blockconfig", configBlockStart);
                    add("blockend", configBlockEnd);
                    add("*", configCaptureRaw);
                }

                public string vDisplayNameTag_ = Default.DisplayNameTag;
                bool configDisplayNameTag(string key, string value, Configuration.Options options)
                {
                    if (value == "")
                        return false;

                    vDisplayNameTag_ = value;
                    return true;
                }

                /*bool configTemplate(string key, string value, Configuration.Options options)
                {
                    // create new template
                    manager_.log(Console.LogType.Error, "The template configuration isnt' fully implemented yet");
                    return false;
                }*/

                bool configConsole(string key, string value, Configuration.Options options)
                {
                    var block = App.GridTerminalSystem.GetBlockWithName(value);
                    if (block != null)
                    {
                        manager_.log(Console.LogType.Info, $"Console redirected to {block.CustomName}");
                        return manager_.Console.redirectConsole(block as IMyTextSurfaceProvider, options.asInteger(0, 0));
                    }

                    return false;
                }

                bool configRTFixSize(string key, string value, Configuration.Options options)
                {
                    int index = options.asInteger(0, 0);
                    RenderTargetID id = $"{value}:{index}";

                    if (id == RenderTargetID.Invalid)
                    {
                        manager_.log(Console.LogType.Error, $"Invalid block id \"{value}\"");
                        return false;
                    }

                    RectangleF rect = new RectangleF(options.asVector(2, new Vector2()), options.asVector(1, new Vector2()));
                    if (Default.RenderTargetFixSize.ToList().Exists((pair) => pair.Key.Equals(id)))
                        Default.RenderTargetFixSize[id] = rect;
                    else
                        Default.RenderTargetFixSize.Add(id, rect);

                    return true;
                }

                bool configMaxSpeed(string key, string value, Configuration.Options options)
                {
                    Default.MaxShipSpeed = Configuration.asFloat(value, Default.MaxShipSpeed);
                    return true;
                }

                bool configMaxAmountItem(string key, string value, Configuration.Options options)
                {
                    long amount = options.asInteger(0, (int)Default.MaxAmountItems);
                    if (amount <= 0)
                        return false;

                    VISItemType item = $"{Default.MyObjectBuilder}_{value}";
                    if (item.Valid)
                    {
                        var list = Default.AmountItems.ToList();
                        var index = list.FindIndex(pair => pair.Key == item);
                        if (index >= 0)
                            list.RemoveAt(index);

                        if (!item.Group)
                            list.Insert(0, new KeyValuePair<VISItemType, long>(item, amount));
                        else
                            list.Add(new KeyValuePair<VISItemType, long>(item, amount));

                        Default.AmountItems = list.ToDictionary((ik) => ik.Key, (iv) => iv.Value);
                        return true;
                    }

                    manager_.log(Console.LogType.Error, $"Invalid item type \"{value}\"");
                    return false;
                }

                bool configSetRecInterval(string key, string value, Configuration.Options options)
                {
                    Default.ReconstructInterval = TimeSpan.FromSeconds(Configuration.asFloat(value, Default.ReconstructIntervalInSec));
                    return true;
                }

                bool captureRaw_ = false;
                StringBuilder rawConfig_ = new StringBuilder();
                IMyTerminalBlock currentBlock_ = null;
                bool configBlockStart(string key, string value, Configuration.Options options)
                {
                    if (currentBlock_ != null)
                        return false;

                    // search block
                    App.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(null, (block) =>
                    {
                        if (block.IsSameConstructAs(App.Me) &&
                            block is IMyTextSurfaceProvider &&
                            block.CustomName == value)
                        {
                            captureRaw_ = true;
                            currentBlock_ = block;
                        }
                        return false;
                    });

                    return currentBlock_ != null;
                }

                bool configBlockEnd(string key, string value, Configuration.Options options)
                {
                    if (currentBlock_ != null)
                    {
                        manager_.displayProviders_.Add(new DisplayProvider(
                            currentBlock_.CustomName,
                            currentBlock_ as IMyTextSurfaceProvider,
                            rawConfig_.ToString()));
                    }

                    captureRaw_ = false;
                    rawConfig_.Clear();
                    currentBlock_ = null;
                    return true;
                }

                bool configCaptureRaw(string key, string value, Configuration.Options options)
                {
                    if (captureRaw_)
                    {
                        rawConfig_.AppendLine($"{key}:{value}:{options}");
                        return true;
                    }

                    return false;
                }

                bool configShareConfig(string key, string value, Configuration.Options options)
                {
                    Default.ShareCustomData = Configuration.asBoolean(value, Default.ShareCustomData);
                    return true;
                }
            }
            #endregion // Configuration

            #region Application State System
            #region mdk preserve
            public enum State
            {
                Init,
                Run,
                Shutdown,
                Stopped,
                Error
            }
            #endregion // mdk preserve

            /*public string stateToString(State state)
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
            }*/

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
                    Configuration.Process(config_, App.Me, false, (key, value, options) =>
                    {
                        log(Console.LogType.Error, $"Read config: \"{key}\", \"{value}\"");
                        readConfigFailed = true;
                        return false;
                    });

                    if (!readConfigFailed)
                        initStateStage_ = 1;
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
                            displayProviders_.Add(new DisplayProvider(block.CustomName, provider));

                        return false;
                    });

                    initStateStage_ = 2;
                    dpIndex_ = 0;
                }
                else if (initStateStage_ == 2)
                {
                    if (dpIndex_ >= displayProviders_.Count)
                    {
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
                JobManager.tick(Timer.Delta);
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

            public State CurrentState
            {
                get;
                protected set;
            }
            public void switchState(State nextState) => CurrentState = nextState;

            Dictionary<State, StateDelegate> stateHandlers_ = new Dictionary<State, StateDelegate>();
            bool reboot_ = true;
            #endregion // Application State System

            public void onSave()
            {
            }

            public void onTick(string args, UpdateType updateSource)
            {
                try
                {
                    // update timer
                    Timer.update(App.Runtime.TimeSinceLastRun);

                    // process state
                    stateHandlers_[CurrentState]();

                    // process console
                    Console.flush();
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
