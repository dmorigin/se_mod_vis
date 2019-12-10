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
        public abstract class Graphic : VISObject
        {
            public Graphic(Template template, Configuration.Options options)
            {
                options_ = options;
                template_ = template;
            }


            public override bool construct()
            {
                if (Options.Count >= 1)
                {
                    dataRetrieverName_ = Options[0];
                    return true;
                }

                log(Console.LogType.Error, $"Missing data retriever");
                return false;
            }


            protected abstract bool supportCheck(string name);


            #region Configuration
            public class ConfigHandler : Configuration.Handler
            {
                public ConfigHandler(Graphic graphic)
                {
                    graphic_ = graphic;

                    add("position", configPosition);
                    add("size", configSize);
                    add("color", configColor);
                    add("check", configCheck);
                }


                Graphic graphic_ = null;


                protected virtual bool configPosition(string key, string value, Configuration.Options options)
                {
                    graphic_.Position = Configuration.asVector(value, Program.Default.Position);
                    if (options.Count > 0)
                    {
                        string type = options[0].ToLower();
                        if (type == "relative")
                            graphic_.PositionType = ValueType.Relative;
                        else
                            graphic_.PositionType = ValueType.Absolut;
                    }

                    return true;
                }

                protected virtual bool configSize(string key, string value, Configuration.Options options)
                {
                    graphic_.Size = Configuration.asVector(value, Program.Default.Size);
                    if (options.Count > 0)
                    {
                        string type = options[0].ToLower();
                        if (type == "relative")
                            graphic_.SizeType = ValueType.Relative;
                        else
                            graphic_.SizeType = ValueType.Absolut;
                    }

                    return true;
                }

                /*!
                 * Set a color. This color is used for all sprites. The alpha value is
                 * optional.
                 * 
                 * Syntax: color:r,g,b(,a)
                 */
                protected virtual bool configColor(string key, string value, Configuration.Options options)
                {
                    //graphic_.Color = Configuration.asColor(value, Program.Default.Color);
                    graphic_.addGradientColor(1f, Configuration.asColor(value, Program.Default.Color));
                    return true;
                }

                /*!
                 * Set a color for a gradient. This colors are used if a graphic has a data collector. The alpha
                 * value is optional. The indicator is set as floating point value. The interpretation is
                 * from 0 to n
                 * 
                 * Syntax: gradient:indicator:r,g,b(,a)
                 */
                protected virtual bool configGradient(string key, string value, Configuration.Options options)
                {
                    if (options.Count == 1)
                    {
                        float indicator = Configuration.asFloat(value, 0f);
                        Color color = options.getAsColor(0, Program.Default.Color);
                        graphic_.addGradientColor(indicator, color);
                    }
                    return false;
                }

                /*!
                 * Syntax: check:type:options
                 */
                bool configCheck(string key, string value, Configuration.Options options)
                {
                    string name = value.ToLower();
                    if (graphic_.supportCheck(name))
                    {
                        IDataCollector dataCollector = graphic_.CollectorManager.getDataCollector(name, options);
                        if (dataCollector != null)
                        {
                            graphic_.DataCollector = dataCollector;
                            DataRetriever retriever = dataCollector.getDataRetriever(graphic_.DataRetrieverName);
                            if (retriever != null)
                            {
                                graphic_.DataRetriever = retriever;
                                return true;
                            }
                        }
                    }
                    else
                        graphic_.log(Console.LogType.Error, $"Check type {value} isn't supported");

                    return false;
                }
            }

            public virtual ConfigHandler getConfigHandler()
            {
                return new ConfigHandler(this);
            }
            #endregion // Configuration


            public enum ValueType
            {
                Absolut,
                Relative
            }

            #region Properties
            Configuration.Options options_ = null;
            protected Configuration.Options Options
            {
                get { return options_; }
            }

            protected DataCollectorManager CollectorManager
            {
                get { return Manager.CollectorManager; }
            }

            IDataCollector dataCollector_ = null;
            public IDataCollector DataCollector
            {
                get { return dataCollector_; }
                protected set { dataCollector_ = value; }
            }

            string dataRetrieverName_ = "";
            protected string DataRetrieverName
            {
                get { return dataRetrieverName_; }
                set { dataRetrieverName_ = value; }
            }

            DataRetriever dataRetriever_ = null;
            protected DataRetriever DataRetriever
            {
                get { return dataRetriever_; }
                set { dataRetriever_ = value; }
            }

            Template template_ = null;
            public Template Template
            {
                get { return template_; }
            }

            Vector2 position_ = Program.Default.Position;
            public Vector2 Position
            {
                get { return position_; }
                protected set { position_ = value; }
            }


            ValueType positionType_ = Program.Default.PositionType;
            public ValueType PositionType
            {
                get { return positionType_; }
                protected set { positionType_ = value; }
            }


            Vector2 size_ = Program.Default.Size;
            public Vector2 Size
            {
                get { return size_; }
                protected set { size_ = value; }
            }

            ValueType sizeType_ = Program.Default.SizeType;
            public ValueType SizeType
            {
                get { return sizeType_; }
                protected set { sizeType_ = value; }
            }

            public Color Color
            {
                get { return getGradientColor(1f); }
                protected set { addGradientColor(1f, value); }
            }

            Dictionary<float, Color> colorGradient_ = new Dictionary<float, Color>();
            public Color getGradientColor(float indicator)
            {
                foreach (var pair in colorGradient_)
                {
                    if (pair.Key <= indicator)
                        return pair.Value;
                }

                return Program.Default.Color;
            }

            public void addGradientColor(float indicator, Color color)
            {
                if (colorGradient_.ContainsKey(indicator))
                    colorGradient_[indicator] = color;
                else
                {
                    colorGradient_.Add(indicator, color);
                    colorGradient_ = colorGradient_.OrderBy(x => x.Key).ToDictionary(a => a.Key, b => b.Value);
                }
            }

            protected Dictionary<float, Color> Gradient
            {
                get { return colorGradient_; }
            }
            #endregion // Properties

            public delegate void AddSpriteDelegate(MySprite sprite);
            public abstract void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite);

            #region Render Helper
            protected static void renderTextLine(RenderTarget rt, AddSpriteDelegate addSprite, string font, float fontSize, Vector2 position, 
                Color fontColor, string textLine, TextAlignment alignment)
            {
                // fix font position
                Vector2 offset = new Vector2(rt.DisplayOffset.X, rt.DisplayOffset.Y - (0.75f * (fontSize * rt.FontSize.Y)));
                MySprite sprite = MySprite.CreateText(textLine, font, fontColor, fontSize, alignment);
                sprite.Position = position + offset;
                addSprite(sprite);
            }

            protected static void renderBar(RenderTarget rt, AddSpriteDelegate addSprite, Vector2 position, Vector2 size, bool vertical, float ration,
                Dictionary<float, Color> gradient, Color backgroundColor)
            {
                // draw background
                if (backgroundColor.A > 0)
                {
                    MySprite bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", position, size, backgroundColor);
                    addSprite(bg);
                }

                float startRatio = 0f;
                KeyValuePair<float, Color>[] colors = gradient.ToArray();
                for (int s = 0; s < colors.Length && startRatio <= ration; ++s)
                {
                    float curRatio = Math.Min(colors[s].Key, ration) - startRatio;
                    Vector2 barSize;
                    Vector2 barPosition;

                    if (vertical)
                    {
                        barSize = new Vector2(size.X, size.Y * curRatio);
                        barPosition = new Vector2(position.X, position.Y + ((size.Y - barSize.Y) * 0.5f) - (size.Y * startRatio));
                    }
                    else
                    {
                        barSize = new Vector2(size.X * curRatio, size.Y);
                        barPosition = new Vector2(position.X - ((size.X - barSize.X) * 0.5f) + (size.X * startRatio), position.Y);
                    }

                    MySprite bar = new MySprite(SpriteType.TEXTURE, "SquareSimple", barPosition, barSize, colors[s].Value);
                    addSprite(bar);

                    startRatio += curRatio;
                }
            }
            #endregion // Render Helper
        }
    }
}
