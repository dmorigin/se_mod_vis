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
        public class Configuration
        {
            #region String to Type conversion
            public static bool asBoolean(string data, bool defaultValue = true)
            {
                data = data.ToLower();
                if (data == "true")
                    return true;
                if (data == "false")
                    return false;

                return defaultValue;
            }


            public static int asInteger(string data, int defaultValue = 0)
            {
                int value;
                if (!int.TryParse(data, out value))
                    return defaultValue;
                return value;
            }


            public static float asFloat(string data, float defaultValue = 0f)
            {
                float value;
                if (!float.TryParse(data, out value))
                    return defaultValue;
                return value;
            }


            public static Color asColor(string data, Color defaultValue = new Color())
            {
                int count = data.Count(x => x == ',');
                if (count == 2)
                {
                    Vector3I vec = asVector(data, new Vector3I(defaultValue.R, defaultValue.G, defaultValue.B));
                    return new Color(vec.X, vec.Y, vec.Z);
                }

                if (count == 3)
                {
                    Vector4I vec = asVector(data, new Vector4I(defaultValue.R, defaultValue.G, defaultValue.B, defaultValue.A));
                    return new Color(vec.X, vec.Y, vec.Z, vec.W);
                }

                return defaultValue;
            }


            public static Vector2 asVector(string data, Vector2 defaultValue = new Vector2())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 2)
                    {
                        float x, y;
                        float.TryParse(parts[0], out x);
                        float.TryParse(parts[1], out y);

                        return new Vector2(x, y);
                    }
                }

                return defaultValue;
            }


            public static Vector3 asVector(string data, Vector3 defaultValue = new Vector3())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 3)
                    {
                        float x, y, z;
                        float.TryParse(parts[0], out x);
                        float.TryParse(parts[1], out y);
                        float.TryParse(parts[2], out z);

                        return new Vector3(x, y, z);
                    }
                }

                return defaultValue;
            }


            public static Vector4 asVector(string data, Vector4 defaultValue = new Vector4())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 4)
                    {
                        float x, y, z, w;
                        float.TryParse(parts[0], out x);
                        float.TryParse(parts[1], out y);
                        float.TryParse(parts[2], out z);
                        float.TryParse(parts[3], out w);

                        return new Vector4(x, y, z, w);
                    }
                }

                return defaultValue;
            }


            public static Vector2I asVector(string data, Vector2I defaultValue = new Vector2I())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 2)
                    {
                        int x, y;
                        int.TryParse(parts[0], out x);
                        int.TryParse(parts[1], out y);

                        return new Vector2I(x, y);
                    }
                }

                return defaultValue;
            }


            public static Vector3I asVector(string data, Vector3I defaultValue = new Vector3I())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 3)
                    {
                        int x, y, z;
                        int.TryParse(parts[0], out x);
                        int.TryParse(parts[1], out y);
                        int.TryParse(parts[2], out z);

                        return new Vector3I(x, y, z);
                    }
                }

                return defaultValue;
            }


            public static Vector4I asVector(string data, Vector4I defaultValue = new Vector4I())
            {
                if (data != string.Empty)
                {
                    string[] parts = data.Split(',');
                    if (parts.Length == 4)
                    {
                        int x, y, z, w;
                        int.TryParse(parts[0], out x);
                        int.TryParse(parts[1], out y);
                        int.TryParse(parts[2], out z);
                        int.TryParse(parts[3], out w);

                        return new Vector4I(x, y, z, w);
                    }
                }

                return defaultValue;
            }
            #endregion


            public class Options
            {
                List<string> options_ = null;


                public Options(List<string> options)
                {
                    options_ = options;
                }


                public Options(Options other)
                {
                    options_ = new List<string>();
                    options_.AddList(other.options_);
                }


                public Options clone()
                {
                    List<string> data = new List<string>();
                    data.AddList(options_);
                    return new Options(data);
                }


                public bool equals(Options other) => options_.SequenceEqual(other.options_);
                public int Count => options_.Count;


                public string this[int index]
                {
                    get
                    {
                        if (index < options_.Count && index >= 0)
                            return options_[index].Trim();
                        return string.Empty;
                    }
                }


                public bool asBoolean(int index, bool defaultValue = true) => Configuration.asBoolean(this[index], defaultValue);
                public int asInteger(int index, int defaultValue = 0) => Configuration.asInteger(this[index], defaultValue);
                public float asFloat(int index, float defaultValue = 0f) => Configuration.asFloat(this[index], defaultValue);
                public Color asColor(int index, Color defaultValue = new Color()) => Configuration.asColor(this[index], defaultValue);
                public Vector2 asVector(int index, Vector2 defaultValue = new Vector2()) => Configuration.asVector(this[index], defaultValue);
                public Vector3 asVector(int index, Vector3 defaultValue = new Vector3()) => Configuration.asVector(this[index], defaultValue);
                public Vector4 asVector(int index, Vector4 defaultValue = new Vector4()) => Configuration.asVector(this[index], defaultValue);
                public Vector2I asVector(int index, Vector2I defaultValue = new Vector2I()) => Configuration.asVector(this[index], defaultValue);
                public Vector3I asVector(int index, Vector3I defaultValue = new Vector3I()) => Configuration.asVector(this[index], defaultValue);
                public Vector4I asVector(int index, Vector4I defaultValue = new Vector4I()) => Configuration.asVector(this[index], defaultValue);
            }


            public class Handler
            {
                public delegate bool KeyHandler(string key, string value, Options options);
                Dictionary<string, KeyHandler> handlers_ = new Dictionary<string, KeyHandler>();
                Handler sub_ = null;
                Handler parent_ = null;

                public void setSubHandler(Handler subHandler)
                {
                    sub_ = subHandler;
                    sub_.parent_ = this;
                }

                public void add(string key, KeyHandler handler)
                {
                    if (handlers_.ContainsKey(key))
                        return;

                    handlers_[key] = handler;
                }

                public void remove(string key)
                {
                    handlers_.Remove(key);
                }

                public bool exists(string key) => handlers_.ContainsKey(key);
                Handler last() => sub_ != null ? sub_.last() : this;

                KeyHandler get(Handler use, string key)
                {
                    if (use.handlers_.ContainsKey(key))
                        return use.handlers_[key];

                    if (use.parent_ != null)
                    {
                        Handler parent = use.parent_;
                        use.parent_ = null;
                        parent.sub_ = null;
                        return get(parent, key);
                    }

                    return handlers_["*"];
                }

                public KeyHandler this[string key] => get(last(), key);

                bool wildcardHandler(string key, string value, Options options) => false;

                public Handler()
                {
                    add("*", wildcardHandler);
                }
            }


            static bool processConfig(Handler handler, string config, Func<string, string, List<string>, bool> errorHandler)
            {
                // convert to line list
                List<string> lines = config.Trim().Split('\n').ToList();
                foreach (var line in lines)
                {
                    string trim = line.Trim();
                    if (trim.Length == 0 || trim[0] == '#')
                        continue;

                    // split in segments
                    List<string> segments = trim.Split(':').ToList();
                    if (segments.Count > 0)
                    {
                        string key = segments[0].Trim().ToLower();
                        string value = segments.Count >= 2 ? segments[1].Trim() : "";
                        List<string> options = segments.Count > 2 ? segments.GetRange(2, segments.Count - 2) : new List<string>();

                        if (key.Length <= 0)
                            continue;

                        if (!handler[key](key, value, new Options(options)))
                            return errorHandler(key, value, options);
                    }
                }

                return true;
            }


            public static bool Process(Handler handler, string config, Func<string, string, List<string>, bool> errorHandler = null)
            {
                return processConfig(handler, config, errorHandler != null ? errorHandler : (key, value, options) =>
                {
                    return false;
                });
            }


            public static bool Process(Handler handler, IMyTerminalBlock block, Func<string, string, List<string>, bool> errorHandler = null)
            {
                return processConfig(handler, block.CustomData, errorHandler != null ? errorHandler : (key, value, options) =>
                {
                    return false;
                });
            }
        }
    }
}
