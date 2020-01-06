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
                    dataRetrieverName_ = Options[0];
                else
                    log(Console.LogType.Warning, $"No data retriever defined");

                return true;
            }

            public abstract Graphic clone();
            protected abstract bool supportCheck(string name);

            #region Configuration
            public class ConfigHandler : Configuration.Handler
            {
                public ConfigHandler(Graphic graphic)
                {
                    graphic_ = graphic;

                    add("zposition", configZPosition);
                    add("position", configPosition);
                    add("size", configSize);
                    add("color", configColor);
                    add("gradient", configGradient);
                    add("check", configCheck);
                    add("dcrefresh", configDCRefresh);
                }

                Graphic graphic_ = null;

                protected virtual bool configZPosition(string key, string value, Configuration.Options options)
                {
                    graphic_.ZPosition = Configuration.asInteger(value, Program.Default.ZPosition);
                    return true;
                }

                protected virtual bool configPosition(string key, string value, Configuration.Options options)
                {
                    graphic_.Position = Configuration.asVector(value, Program.Default.Position);
                    if (options.Count == 0)
                    {
                        string type = options[0].ToLower();
                        if (type == "relative" || type == "r")
                            graphic_.PositionType = ValueType.Relative;
                        else if (type == "absolute" || type == "a")
                            graphic_.PositionType = ValueType.Absolute;
                        else
                        {
                            graphic_.log(Console.LogType.Error, $"Invalid position type:{type}");
                            return false;
                        }
                    }

                    return true;
                }

                protected virtual bool configSize(string key, string value, Configuration.Options options)
                {
                    graphic_.Size = Configuration.asVector(value, Program.Default.Size);
                    if (options.Count == 1)
                    {
                        string type = options[0].ToLower();
                        if (type == "relative" || type == "r")
                            graphic_.SizeType = ValueType.Relative;
                        else if (type == "absolute" || type == "a")
                            graphic_.SizeType = ValueType.Absolute;
                        else
                        {
                            graphic_.log(Console.LogType.Error, $"Invalid size type:{type}");
                            return false;
                        }
                    }

                    return true;
                }

                protected virtual bool configColor(string key, string value, Configuration.Options options)
                {
                    graphic_.colorGradient_.Clear();
                    graphic_.addGradientColor(0f, Configuration.asColor(value, Program.Default.Color));
                    return true;
                }

                protected virtual bool configGradient(string key, string value, Configuration.Options options)
                {
                    if (options.Count == 1)
                    {
                        float indicator = Configuration.asFloat(value, 0f);
                        Color color = options.asColor(0, Program.Default.Color);
                        graphic_.addGradientColor(indicator, color);
                        return true;
                    }
                    return false;
                }

                bool configCheck(string key, string value, Configuration.Options options)
                {
                    string name = value.ToLower();
                    if (graphic_.supportCheck(name))
                    {
                        IDataCollector dataCollector = graphic_.CollectorManager.getDataCollector(name, options);
                        if (dataCollector != null)
                        {
                            graphic_.DataCollector = dataCollector;
                            graphic_.DataCollector.MaxUpdateInterval = graphic_.MaxDCUpdateInterval;
                            DataRetriever retriever = dataCollector.getDataRetriever(graphic_.DataRetrieverName);
                            if (retriever != null)
                            {
                                graphic_.DataRetriever = retriever;
                                return true;
                            }
                            else
                                graphic_.log(Console.LogType.Error, $"Data retriever ${graphic_.DataRetrieverName} isn't supported");
                        }
                    }
                    else
                        graphic_.log(Console.LogType.Error, $"Check type {value} isn't supported");

                    return false;
                }

                bool configDCRefresh(string key, string value, Configuration.Options options)
                {
                    float interval = Configuration.asFloat(value, Program.Default.DCRefreshInSec);
                    graphic_.MaxDCUpdateInterval = TimeSpan.FromSeconds(interval);
                    if (graphic_.DataCollector != null)
                        graphic_.DataCollector.MaxUpdateInterval = graphic_.MaxDCUpdateInterval;
                    return true;
                }
            }

            public virtual ConfigHandler getConfigHandler()
            {
                return new ConfigHandler(this);
            }
            #endregion // Configuration

            public enum ValueType
            {
                Absolute,
                Relative
            }

            #region Properties
            Template template_ = null;
            public Template Template
            {
                get { return template_; }
            }

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

            TimeSpan maxDCUpdateInterval_ = Program.Default.DCRefresh;
            public TimeSpan MaxDCUpdateInterval
            {
                get { return maxDCUpdateInterval_; }
                protected set { maxDCUpdateInterval_ = value; }
            }

            int zPosition_ = Program.Default.ZPosition;
            public int ZPosition
            {
                get { return zPosition_; }
                protected set { zPosition_ = value; }
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
                get { return getGradientColor(0f); }
                protected set { addGradientColor(0f, value); }
            }

            Dictionary<float, Color> colorGradient_ = new Dictionary<float, Color>();
            public Color getGradientColor(float indicator)
            {
                return getGradientColor(indicator, colorGradient_);
            }

            public Color getGradientColor(float indicator, Dictionary<float, Color> gradient)
            {
                foreach (var pair in gradient)
                {
                    if (pair.Key <= indicator)
                        return pair.Value;
                }

                return Template.FontColor;
            }

            public void addGradientColor(float indicator, Color color)
            {
                if (colorGradient_.ContainsKey(indicator))
                    colorGradient_[indicator] = color;
                else
                {
                    colorGradient_.Add(indicator, color);
                    colorGradient_ = colorGradient_.OrderByDescending(x => x.Key).ToDictionary(a => a.Key, b => b.Value);
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
            protected static void renderTextLine(Display display, RenderTarget rt, AddSpriteDelegate addSprite, 
                string font, float fontSize, Vector2 position, Color fontColor, string textLine, TextAlignment alignment)
            {
                // fix font position
                Vector2 offset = new Vector2(rt.DisplayOffset.X, rt.DisplayOffset.Y - (0.61f * (fontSize * Default.CharHeight)));
                MySprite sprite = MySprite.CreateText(textLine, font, fontColor, fontSize, alignment);
                sprite.Position = position + offset;
                addSprite(sprite);
            }

            protected static void renderBar(AddSpriteDelegate addSprite, Vector2 position, Vector2 size, bool vertical, float ratio,
                Dictionary<float, Color> gradient, Color backgroundColor)
            {
                // draw background
                if (backgroundColor.A > 0)
                    addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple", position, size, backgroundColor));

                float startRatio = ratio;
                KeyValuePair<float, Color>[] colors = gradient.ToArray();
                for (int c = 0; c < colors.Length && startRatio > 0f; ++c)
                {
                    if (colors[c].Key > startRatio)
                        continue;

                    Vector2 barSize;
                    Vector2 barPosition;
                    float curRatio = startRatio - colors[c].Key;

                    if (vertical)
                    {
                        barSize = new Vector2(size.X, size.Y * curRatio);
                        barPosition = new Vector2(position.X, position.Y - (colors[c].Key * size.Y) + ((size.Y - barSize.Y) * 0.5f));
                    }
                    else
                    {
                        barSize = new Vector2(size.X * curRatio, size.Y);
                        barPosition = new Vector2(position.X + (colors[c].Key * size.X) - ((size.X - barSize.X) * 0.5f), position.Y);
                    }

                    MySprite bar = new MySprite(SpriteType.TEXTURE, "SquareSimple", barPosition, barSize, colors[c].Value);
                    addSprite(bar);
                    startRatio -= curRatio;
                }
            }

            protected enum SliderOrientation
            {
                Left,
                Right,
                Top,
                Bottom
            };
            protected void renderSlider(AddSpriteDelegate addSprite, Vector2 position, Vector2 size, bool doubleSided, float ratio, 
                Dictionary<float, Color> barGradient, SliderOrientation sliderOrientation, float sliderWidth, Color sliderColor)
            {
                ratio = !doubleSided ? ((ratio * 2f) - 1f) * 0.5f : ratio * 0.5f;

                bool vertical = sliderOrientation == SliderOrientation.Left || sliderOrientation == SliderOrientation.Right;
                Vector2 sliderSize;
                Vector2 sliderPosition;
                Vector2 barPosition;
                Vector2 barSize;

                const float barHeightFactor = 0.8f;
                const float sliderHeightFactor = 0.9f;

                if (vertical)
                {
                    sliderSize = new Vector2(size.X * sliderHeightFactor, sliderWidth * size.Y);
                    barSize = new Vector2(size.X * barHeightFactor, size.Y - sliderSize.Y);

                    if (sliderOrientation == SliderOrientation.Left)
                    {
                        barPosition = new Vector2(position.X + (size.X - barSize.X) * 0.5f, position.Y);
                        sliderPosition = new Vector2(position.X - (size.X - sliderSize.X) * 0.5f, position.Y - ratio * barSize.Y);
                    }
                    else
                    {
                        barPosition = new Vector2(position.X - (size.X - barSize.X) * 0.5f, position.Y);
                        sliderPosition = new Vector2(position.X + (size.X - sliderSize.X) * 0.5f, position.Y - ratio * barSize.Y);
                    }
                }
                else
                {
                    sliderSize = new Vector2(sliderWidth * size.X, size.Y * sliderHeightFactor);
                    barSize = new Vector2(size.X - sliderSize.X, size.Y * barHeightFactor);

                    if (sliderOrientation == SliderOrientation.Top)
                    {
                        barPosition = new Vector2(position.X, position.Y + (size.Y - barSize.Y) * 0.5f);
                        sliderPosition = new Vector2(position.X + ratio * barSize.X, position.Y - (size.Y - sliderSize.Y) * 0.5f);
                    }
                    else
                    {
                        barPosition = new Vector2(position.X, position.Y - (size.Y - barSize.Y) * 0.5f);
                        sliderPosition = new Vector2(position.X + ratio * barSize.X, position.Y + (size.Y - sliderSize.Y) * 0.5f);
                    }
                }

                if (doubleSided == true)
                {
                    Dictionary<float, Color> clamped = new Dictionary<float, Color>();
                    foreach (var pair in barGradient)
                        clamped.Add((pair.Key * 0.5f) + 0.5f, pair.Value);

                    renderBar(addSprite, barPosition, barSize, vertical, 1f, clamped, new Color(0, 0, 0, 0));
                }
                else
                    renderBar(addSprite, barPosition, barSize, vertical, 1f, barGradient, new Color(0, 0, 0, 0));

                // draw slider
                if (vertical)
                {
                    if (sliderOrientation == SliderOrientation.Left)
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, "Circle",
                            new Vector2(sliderPosition.X - sliderSize.X * 0.5f + sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X + sliderSize.X * 0.5f - sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.Y, sliderSize.Y),
                            sliderColor, rotation:(float)(Math.PI * 0.5f)));
                        addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                            new Vector2(sliderPosition.X - sliderSize.Y * 0.25f, sliderPosition.Y),
                            new Vector2(sliderSize.X - sliderSize.Y * 1.5f, sliderSize.Y),
                            sliderColor));
                    }
                    else
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, "Circle",
                            new Vector2(sliderPosition.X + sliderSize.X * 0.5f - sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X - sliderSize.X * 0.5f + sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.Y, sliderSize.Y),
                            sliderColor, rotation: (float)(Math.PI * 1.5f)));
                        addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                            new Vector2(sliderPosition.X + sliderSize.Y * 0.25f, sliderPosition.Y),
                            new Vector2(sliderSize.X - sliderSize.Y * 1.5f, sliderSize.Y),
                            sliderColor));
                    }
                }
                else
                {
                    if (sliderOrientation == SliderOrientation.Top)
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, "Circle",
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.Y * 0.5f + sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.Y * 0.5f - sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X, sliderSize.X),
                            sliderColor, rotation: (float)(Math.PI)));
                        addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.X * 0.25f),
                            new Vector2(sliderSize.X, sliderSize.Y - sliderSize.X * 1.5f),
                            sliderColor));
                    }
                    else
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, "Circle",
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.Y * 0.5f - sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.Y * 0.5f + sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X, sliderSize.X),
                            sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.X * 0.25f),
                            new Vector2(sliderSize.X, sliderSize.Y - sliderSize.X * 1.5f),
                            sliderColor));
                    }
                }
            }

            protected class Icon
            {
                public delegate void Render(AddSpriteDelegate addSprite, string name,
                    Vector2 position, Vector2 size, float rotation, Color color);

                public static Render getIcon(string name)
                {
                    if (RenderTarget.spriteExist(name))
                        return renderSEIcon;

                    // custom icon
                    switch(name)
                    {
                        case "Storage":
                            return renderStorageIcon;
                        case "Delimiter":
                            return renderDelimiter;
                    }

                    return null;
                }

                static void renderSEIcon(AddSpriteDelegate addSprite, string name,
                    Vector2 position, Vector2 size, float rotation, Color color)
                {
                    addSprite(new MySprite(SpriteType.TEXTURE, name, position, size, color, rotation: rotation));
                }

                static void renderDelimiter(AddSpriteDelegate addSprite, string name,
                    Vector2 position, Vector2 size, float rotation, Color color)
                {
                    Vector2 posBar = new Vector2(position.X + size.Y * 0.5f, position.Y);
                    addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple", posBar, new Vector2(size.X - size.Y, size.Y), color, rotation:rotation));

                    Vector2 posEndLeft = new Vector2(position.X - size.X * 0.5f + size.Y * 0.5f, position.Y);
                    posEndLeft.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, "Circle", posEndLeft, new Vector2(size.Y, size.Y), color));

                    Vector2 posEndRight = new Vector2(position.X + size.X * 0.5f - size.Y * 0.5f, position.Y);
                    posEndRight.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, "Circle", posEndRight, new Vector2(size.Y, size.Y), color));
                }

                static void renderStorageIcon(AddSpriteDelegate addSprite, string name,
                    Vector2 center, Vector2 size, float rotation, Color color)
                {
                    //size.Y = size.X;
                    Vector2 offset = new Vector2(0f, 0f);
                    Vector2 linesize = size;
                    linesize.X = size.X * 0.05f;
                    linesize.Y = size.Y * 0.5f;

                    MySprite sprite;
                    sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                        size: linesize,
                        color: color
                        );
                    offset.Y = linesize.Y / 2f;
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = offset.X + linesize.Y * (float)Math.Cos(Math.PI / 6);
                    offset.Y = offset.Y - linesize.Y * (float)Math.Sin(Math.PI / 6);
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = offset.X - linesize.Y * 2f * (float)Math.Cos(Math.PI / 6);
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = linesize.Y / 2f * (float)Math.Cos(Math.PI / 6);
                    offset.Y = linesize.Y * 3f / 2f * (float)Math.Sin(Math.PI / 6);
                    sprite.RotationOrScale = (float)Math.PI / 3f;
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.Y = offset.Y - linesize.Y * 2f * (float)Math.Sin(Math.PI / 6);
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = -linesize.Y / 2f * (float)Math.Cos(Math.PI / 6);
                    offset.Y = offset.Y - linesize.Y / 2f;
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = -linesize.Y / 2f * (float)Math.Cos(Math.PI / 6);
                    offset.Y = linesize.Y * 3f / 2f * (float)Math.Sin(Math.PI / 6);
                    sprite.RotationOrScale = -(float)Math.PI / 3f;
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.Y = offset.Y - linesize.Y * 2f * (float)Math.Sin(Math.PI / 6);
                    sprite.Position = center + offset;
                    addSprite(sprite);

                    offset.X = linesize.Y / 2f * (float)Math.Cos(Math.PI / 6);
                    offset.Y = offset.Y - linesize.Y / 2f;
                    sprite.Position = center + offset;
                    addSprite(sprite);
                }
            }
            #endregion // Render Helper
        }
    }
}
