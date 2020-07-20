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
            static int GraphicIDCounter = 0;
            public Graphic(ContentContainer template, Configuration.Options options)
                : base($"GraphicObject:{GraphicIDCounter++}")
            {
                Options = options;
                Template = template;
                DataAccessorName = "";
                RenderData = createRenderDataObj();

                // defaults
                MaxDCUpdateInterval = Default.DCRefresh;
                Position = Default.Position;
                PositionType = Default.PositionType;
                Size = Default.Size;
                SizeType = Default.SizeType;
                VisibleThresholdA = -1.0;
                VisibleOperatorA = greaterequal;
                VisibleThresholdB = -1.0;
                VisibleOperatorB = val_true;
                VisibleCondition = cond_and;

                RenderData.BackgroundColor = new Color(0, 0, 0, 0);
                RenderData.BackgroundIconName = IconNameSquareSimple;
                RenderData.BackgroundRotation = 0f;

                RenderData.BorderColor = new Color(0, 0, 0, 0);
                RenderData.BorderThickness = 0f;
                RenderData.BorderSpacing = 0f;
                RenderData.BorderName = "Simple";
            }

            public override bool construct()
            {
                if (Options.Count >= 1)
                    DataAccessorName = Options[0];
                else
                    log(Console.LogType.Warning, $"No data accessor defined");

                return true;
            }

            //public abstract Graphic clone();
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
                    add("gradient", configGradient);
                    add("border", configBorder);
                    add("background", configBackground);
                    add("check", configCheck);
                    add("checkremote", configCheckRemote);
                    add("dcrefresh", configDCRefresh);
                    add("visibility", configVisibility);
                }

                Graphic graphic_ = null;

                protected virtual bool configPosition(string key, string value, Configuration.Options options)
                {
                    graphic_.Position = Configuration.asVector(value, Default.Position);
                    if (options.Count > 0)
                    {
                        ValueType vt;
                        if (!toValueType(options[0], out vt, Default.PositionType))
                            return false;
                        graphic_.PositionType = vt;
                    }

                    return true;
                }

                protected virtual bool configSize(string key, string value, Configuration.Options options)
                {
                    graphic_.Size = Configuration.asVector(value, Default.Size);
                    if (options.Count > 0)
                    {
                        ValueType vt;
                        if (!toValueType(options[0], out vt, Default.SizeType))
                            return false;
                        graphic_.SizeType = vt;
                    }

                    return true;
                }

                protected virtual bool configColor(string key, string value, Configuration.Options options)
                {
                    graphic_.colorGradient_.Clear();
                    graphic_.addGradientColor(0f, Configuration.asColor(value, Default.Color));
                    return true;
                }

                protected virtual bool configGradient(string key, string value, Configuration.Options options)
                {
                    if (options.Count == 1)
                    {
                        float threshold = Configuration.asFloat(value, 0f);
                        Color color = options.asColor(0, Default.Color);
                        graphic_.addGradientColor(threshold, color);
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
                            graphic_.DataAccessor = dataCollector.getDataAccessor(graphic_.DataAccessorName);
                            return true;
                        }
                    }
                    else
                        graphic_.log(Console.LogType.Error, $"Check type {value} isn't supported");

                    return false;
                }

                bool configCheckRemote(string key, string value, Configuration.Options options)
                {
                    string name = value.ToLower();
                    if (graphic_.supportCheck(name))
                    {
                        IDataCollector dataCollector = graphic_.CollectorManager.getDataCollector(name, options, options[0]);
                        if (dataCollector != null)
                        {
                            graphic_.DataCollector = dataCollector;
                            graphic_.DataCollector.MaxUpdateInterval = graphic_.MaxDCUpdateInterval;
                            graphic_.DataAccessor = dataCollector.getDataAccessor(graphic_.DataAccessorName);
                            return true;
                        }
                    }
                    else
                        graphic_.log(Console.LogType.Error, $"Check type {value} isn't supported");

                    return false;
                }

                bool configDCRefresh(string key, string value, Configuration.Options options)
                {
                    float interval = Configuration.asFloat(value, Default.DCRefreshInSec);
                    graphic_.MaxDCUpdateInterval = TimeSpan.FromSeconds(interval);
                    if (graphic_.DataCollector != null)
                        graphic_.DataCollector.MaxUpdateInterval = graphic_.MaxDCUpdateInterval;
                    return true;
                }

                bool configVisibility(string key, string value, Configuration.Options options)
                {
                    Func<string, OperatorDelegate> func = (op) =>
                    {
                        switch (op)
                        {
                            case "equal":
                            case "==":
                                return equal;
                            case "unequal":
                            case "!=":
                                return unequal;
                            case "less":
                            case "<":
                                return less;
                            case "greater":
                            case ">":
                                return greater;
                            case "lessequal":
                            case "<=":
                                return lessequal;
                            case "greaterequal":
                            case ">=":
                                return greaterequal;
                            default:
                                return val_false;
                        }
                    };

                    graphic_.VisibleOperatorA = func(value.ToLower());
                    graphic_.VisibleThresholdA = options.asFloat(0, 0f);

                    if (options.Count >= 4)
                    {
                        switch (options[1].ToLower())
                        {
                            case "||":
                            case "or":
                                graphic_.VisibleCondition = cond_or;
                                break;
                            case "&&":
                            case "and":
                                graphic_.VisibleCondition = cond_and;
                                break;
                            default:
                                return false;
                        }
                        graphic_.VisibleOperatorB = func(options[2].ToLower());
                        graphic_.VisibleThresholdB = options.asFloat(3, 0f);
                    }

                    return true;
                }

                bool configBackground(string key, string value, Configuration.Options options)
                {
                    // background:color:icon:rotation:thickness:type

                    graphic_.RenderData.BackgroundColor = Configuration.asColor(value, Default.GfxBackgroundColor);

                    if (options.Count > 0)
                    {
                        graphic_.RenderData.BackgroundIconName = options[0] == "" ? IconNameSquareSimple : options[0];
                        graphic_.RenderData.BackgroundIcon = Icon.getIcon(options[0]);
                        if (graphic_.RenderData.BackgroundIcon == null)
                        {
                            graphic_.log(Console.LogType.Error, $"Invalid background icon '{options[0]}'");
                            return false;
                        }

                        graphic_.BackgroundRotation = (options.asFloat(1, 0f) / 180f) * (float)Math.PI;

                        graphic_.BackgroundThickness = options.asFloat(2, 0f);
                        ValueType vt;
                        if (!toValueType(options[3], out vt, Default.ValueType))
                            return false;

                        graphic_.BackgroundThicknessType = vt;
                    }

                    return true;
                }

                bool configBorder(string key, string value, Configuration.Options options)
                {
                    // border:style:size:spacing:type:color

                    //string prefix = "VIS_Icon_Border";
                    graphic_.RenderData.BorderIcon = Icon.getIcon($"VIS_Icon_Border{value}");
                    if (graphic_.RenderData.BorderIcon == null)
                    {
                        graphic_.log(Console.LogType.Error, $"Invalid border icon '{value}'");
                        return false;
                    }
                    graphic_.RenderData.BorderName = value;

                    graphic_.BorderSize = options.asFloat(0, Default.BorderSize);
                    graphic_.BorderSpacing = options.asFloat(1, Default.BorderSpacing);

                    ValueType vt;
                    if (!toValueType(options[2], out vt, Default.ValueType))
                        return false;
                    graphic_.BorderSizeType = vt;

                    graphic_.RenderData.BorderColor = options.asColor(3, Default.BorderColor);
                    return true;
                }
            }

            public virtual ConfigHandler getConfigHandler()
            {
                return new ConfigHandler(this);
            }
            #endregion // Configuration

            #region Condition
            // indicator is greater then threshold
            // example indicator > 0.1
            // visibility:less:0.1
            protected delegate bool OperatorDelegate(double a, double b);
            protected delegate bool ConditionDelegate(OperatorDelegate a, OperatorDelegate b, double indicator, double thresholdA, double thresholdB);

            protected static bool val_true(double a, double b) => true;
            protected static bool val_false(double a, double b) => false;
            protected static bool equal(double a, double b) => a == b;
            protected static bool unequal(double a, double b) => a != b;
            protected static bool less(double a, double b) => a < b;
            protected static bool greater(double a, double b) => a > b;
            protected static bool lessequal(double a, double b) => a <= b;
            protected static bool greaterequal(double a, double b) => a >= b;

            static bool cond_and(OperatorDelegate a, OperatorDelegate b, double indicator, double thresholdA, double thresholdB) => a(indicator, thresholdA) && b(indicator, thresholdB);
            static bool cond_or(OperatorDelegate a, OperatorDelegate b, double indicator, double thresholdA, double thresholdB) => a(indicator, thresholdA) || b(indicator, thresholdB);

            /*protected double VisibleThreshold
            {
                get;
                set;
            }

            protected OperatorDelegate VisibleOperator
            {
                get;
                set;
            }*/

            protected double VisibleThresholdA
            {
                get;
                set;
            }

            protected double VisibleThresholdB
            {
                get;
                set;
            }

            protected OperatorDelegate VisibleOperatorA
            {
                get;
                set;
            }

            protected OperatorDelegate VisibleOperatorB
            {
                get;
                set;
            }

            protected ConditionDelegate VisibleCondition
            {
                get;
                set;
            }

            protected bool isVisible(double indicator) => VisibleCondition(VisibleOperatorA, VisibleOperatorB, indicator, VisibleThresholdA, VisibleThresholdB);
            #endregion // Condition

            public enum ValueType
            {
                Absolute,
                Relative
            }

            protected static bool toValueType(string value, out ValueType vt, ValueType defaultValue)
            {
                switch (value.ToLower())
                {
                    case "":
                        vt = defaultValue;
                        return true;
                    case "r":
                    case "relative":
                        vt = ValueType.Relative;
                        return true;
                    case "a":
                    case "absolute":
                        vt = ValueType.Absolute;
                        return true;
                }

                vt = Default.ValueType;
                Program.App.Manager.log(Console.LogType.Error, $"Invalid value type:{value}");
                return false;
            }

            #region Properties
            public ContentContainer Template
            {
                get;
                protected set;
            }

            protected Configuration.Options Options
            {
                get;
                set;
            }

            protected DataCollectorManager CollectorManager => Manager.CollectorManager;

            public IDataCollector DataCollector
            {
                get;
                protected set;
            }

            protected string DataAccessorName
            {
                get;
                set;
            }

            protected DataAccessor DataAccessor
            {
                get;
                set;
            }

            public TimeSpan MaxDCUpdateInterval
            {
                get;
                protected set;
            }

            public Vector2 Position
            {
                get;
                protected set;
            }

            public ValueType PositionType
            {
                get;
                protected set;
            }

            public Vector2 Size
            {
                get;
                protected set;
            }

            public ValueType SizeType
            {
                get;
                protected set;
            }

            public Color Color
            {
                get { return getGradientColor(0f); }
                protected set { addGradientColor(0f, value); }
            }

            Dictionary<float, Color> colorGradient_ = new Dictionary<float, Color>();
            protected Dictionary<float, Color> Gradient => colorGradient_;

            public Color getGradientColor(float threshold)
            {
                Color color;
                if (getGradientColor(threshold, colorGradient_, out color))
                    return color;
                else
                    return Template.FontColor;
            }

            public static bool getGradientColor(float threshold, Dictionary<float, Color> gradient, out Color color)
            {
                color = Default.Color;
                foreach (var pair in gradient)
                {
                    if (pair.Key <= threshold)
                    {
                        color = pair.Value;
                        return true;
                    }
                }

                return false;
            }

            public static bool getGradientColorLerp(float threshold, Dictionary<float, Color> gradient, out Color color)
            {
                color = Default.Color;
                if (gradient.Count == 0)
                    return false;

                KeyValuePair<float, Color> lower = gradient.First();
                KeyValuePair<float, Color> upper = gradient.First();

                if (gradient.Count >= 2)
                {
                    KeyValuePair<float, Color> prev = upper;
                    foreach (var pair in gradient)
                    {
                        if (pair.Key <= threshold)
                        {
                            upper = prev;
                            lower = pair;
                            break;
                        }

                        prev = pair;
                    }
                }

                color = Color.Lerp(lower.Value, upper.Value, (threshold - lower.Key) / (upper.Key - lower.Key));
                return true;
            }

            public void addGradientColor(float threshold, Color color)
            {
                if (colorGradient_.ContainsKey(threshold))
                    colorGradient_[threshold] = color;
                else
                {
                    colorGradient_.Add(threshold, color);
                    colorGradient_ = colorGradient_.OrderByDescending(x => x.Key).ToDictionary(a => a.Key, b => b.Value);
                }
            }

            public float BorderSize
            {
                get;
                set;
            }

            public float BorderSpacing
            {
                get;
                set;
            }

            public ValueType BorderSizeType
            {
                get;
                set;
            }

            public float BackgroundThickness
            {
                get;
                set;
            }

            public ValueType BackgroundThicknessType
            {
                get;
                set;
            }

            public float BackgroundRotation
            {
                get;
                set;
            }
            #endregion // Properties

            #region Rendering
            public delegate void AddSpriteDelegate(MySprite sprite);

            protected class RenderDataBase
            {
                // render area
                public Vector2 Position;
                public Vector2 OuterSize;
                public Vector2 InnerSize;

                // border
                public Color BorderColor;
                public float BorderThickness;
                public float BorderRotation;
                public float BorderSpacing;
                public string BorderName;
                public Icon.Render BorderIcon;

                // background
                public Vector2 BackgroundSize;
                public Color BackgroundColor;
                public string BackgroundIconName;
                public Icon.Render BackgroundIcon;
                public float BackgroundRotation;
                public float BackgroundThickness;
            }

            protected RenderDataBase RenderData
            {
                get;
                set;
            }

            protected virtual RenderDataBase createRenderDataObj() => new RenderDataBase();

            public virtual void prepareRendering(Display display)
            {
                RenderData.Position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                RenderData.OuterSize = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                var min = RenderData.OuterSize.X < RenderData.OuterSize.Y ? RenderData.OuterSize.X : RenderData.OuterSize.Y;

                // calculate border
                RenderData.BorderThickness = BorderSizeType == ValueType.Relative ? BorderSize * min : BorderSize;
                RenderData.BorderSpacing = BorderSizeType == ValueType.Relative ? BorderSpacing * min : BorderSpacing;

                // calculate background
                RenderData.BackgroundSize = RenderData.OuterSize - (2 * RenderData.BorderThickness);
                RenderData.BackgroundThickness = BackgroundThicknessType == ValueType.Relative ? BackgroundThickness * min : BackgroundThickness;
                RenderData.BackgroundRotation = BackgroundRotation;

                // calculate inner size
                RenderData.InnerSize = RenderData.BackgroundSize - (2 * RenderData.BorderSpacing);
            }

            public virtual void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                // Render Background
                if (RenderData.BackgroundColor.A > 0 && RenderData.BackgroundIcon != null)
                    RenderData.BackgroundIcon(addSprite, rt, RenderData.BackgroundIconName, RenderData.Position, RenderData.BackgroundSize,
                        RenderData.BackgroundThickness, RenderData.BackgroundRotation, RenderData.BackgroundColor);

                // Render border
                if (RenderData.BorderThickness > 0 && RenderData.BorderIcon != null)
                    RenderData.BorderIcon(addSprite, rt, RenderData.BorderName, RenderData.Position, RenderData.OuterSize,
                        RenderData.BorderThickness, RenderData.BorderRotation, RenderData.BorderColor);
            }
            #endregion // Rendering

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

            #region Render Bars
            public delegate void RenderStyledBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size,
                float rotation, bool doubleSided, int tiles, float tileSpace, string tileName, float ratio, Dictionary<float, Color> gradient,
                float borderSize, Color borderColor, Color backgroundColor);

            protected static void renderEllipseBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size,
                float degreeStart, float degreeEnd, float blockThickness, float ratio,
                Dictionary<float, Color> gradient, bool lerp, Color backgroundColor)
            {
                Vector2 rtPosition = position + rt.DisplayOffset;

                Vector2 halfSize = size * 0.5f;
                float range = Math.Abs(degreeEnd - degreeStart);
                float length = (float)(Math.PI * Math.Sqrt(2f * (halfSize.X * halfSize.X + halfSize.Y * halfSize.Y)));
                float thickness = Math.Max(size.X, size.Y) * ((blockThickness * 0.01f) * (range / 360f));
                float step = range / (length / thickness);
                float max = range * ratio;

                // render marker
                for (float c = 0, degree = degreeStart + step; degree < degreeEnd; degree += step, c += step)
                {
                    if (c >= max)
                        break;

                    float t = (degree / 180f) * (float)Math.PI + (degree < 0f ? (float)Math.PI * 2f : 0f);
                    Vector2 point = new Vector2(
                        -halfSize.X * (float)Math.Cos(t),
                        -halfSize.Y * (float)Math.Sin(t));

                    // axis vector
                    Vector2 axis;
                    if (degree >= 0f && degree < 180f)
                        axis = new Vector2(-1f, 0f);
                    else// if (degree >= 180f && degree < 360f)
                        axis = new Vector2(1f, 0f);

                    float delta = (float)Math.Acos(Vector2.Dot(Vector2.Normalize(point), axis));

                    Color color;
                    if (lerp)
                        getGradientColorLerp(c / range, gradient, out color);
                    else
                        getGradientColor(c / range, gradient, out color);

                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                        (point * 0.5f) + rtPosition,
                        new Vector2(point.Length(), thickness),
                        color, rotation: delta));
                }

                addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle, rtPosition, size * 0.5f, backgroundColor));
                addSprite(new MySprite(SpriteType.TEXTURE, "CircleHollow", rtPosition, size * 1.06f, backgroundColor));
            }

            protected static void renderSimpleBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size, 
                float rotation, bool doubleSided, int tiles, float tileSpace, string tileName, float ratio, Dictionary<float, Color> gradient,
                float borderSize, Color borderColor, Color backgroundColor)
            {
                Vector2 innerSize = size - (borderSize * 2f);
                Vector2 rtPosition = position + rt.DisplayOffset;
                /*
                if (borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, size, borderColor, rotation: rotation));
                if (backgroundColor.A > 0 || borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, innerSize, backgroundColor, rotation: rotation));
                */
                Vector2 barSize;
                Vector2 barPosition;

                if (doubleSided)
                {
                    barSize = new Vector2(innerSize.X, innerSize.Y * ratio * 0.5f);
                    barPosition = new Vector2(0f, -barSize.Y * 0.5f);
                }
                else
                {
                    barSize = new Vector2(innerSize.X, innerSize.Y * ratio);
                    barPosition = new Vector2(0f, (innerSize.Y * 0.5f) - barSize.Y * 0.5f);
                }

                barPosition.Rotate(rotation);

                Color color;
                getGradientColorLerp(ratio, gradient, out color);
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, barPosition + rtPosition, barSize, color, rotation: rotation));
            }

            protected static void renderSegmentedBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size,
                float rotation, bool doubleSided, int tiles, float tileSpace, string tileName, float ratio, Dictionary<float, Color> gradient,
                float borderSize, Color borderColor, Color backgroundColor)
            {
                Vector2 innerSize = size - (borderSize * 2f);
                Vector2 rtPosition = position + rt.DisplayOffset;
                /*
                if (borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, size, borderColor, rotation: rotation));
                if (backgroundColor.A > 0 || borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, innerSize, backgroundColor, rotation: rotation));
                */
                float startRatio = ratio;
                float innerSizeOffset = 0f;
                if (doubleSided)
                {
                    startRatio = ratio >= 0f ? ratio : 0f;
                    innerSizeOffset = -innerSize.Y * 0.25f;
                    innerSize.Y *= 0.5f;
                }

                KeyValuePair<float, Color>[] colors = gradient.ToArray();
                for (int c = 0; c < colors.Length && startRatio > 0f; ++c)
                {
                    if (colors[c].Key > startRatio)
                        continue;

                    float curRatio = startRatio - colors[c].Key;

                    Vector2 barSize = new Vector2(innerSize.X, innerSize.Y * curRatio);
                    Vector2 barPosition = new Vector2(0f, -(colors[c].Key * innerSize.Y) + ((innerSize.Y - barSize.Y) * 0.5f) + innerSizeOffset);
                    barPosition.Rotate(rotation);

                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, barPosition + rtPosition, barSize, colors[c].Value, rotation: rotation));
                    startRatio -= curRatio;
                }

                for (int d = 0; d < colors.Length && startRatio > ratio; d++)
                {
                    if (colors[d].Key >= 0f)
                        continue;

                    float curRatio = (colors[d].Key < ratio ? ratio : colors[d].Key) - startRatio;

                    Vector2 barSize = new Vector2(innerSize.X, innerSize.Y * curRatio);
                    Vector2 barPosition = new Vector2(0f, -(startRatio * innerSize.Y) + ((innerSize.Y - barSize.Y) * 0.5f) + innerSizeOffset);
                    barPosition.Rotate(rotation);

                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, barPosition + rtPosition, barSize, colors[d].Value, rotation: rotation));
                    startRatio += curRatio;
                }
            }

            protected static void renderTiledBar(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size,
                float rotation, bool doubleSided, int tiles, float tileSpace, string tileName, float ratio, Dictionary<float, Color> gradient,
                float borderSize, Color borderColor, Color backgroundColor)
            {
                Vector2 innerSize = size - (borderSize * 2f);
                Vector2 rtPosition = position + rt.DisplayOffset;
                /*
                if (borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, size, borderColor, rotation: rotation));
                if (backgroundColor.A > 0 || borderSize > 0f)
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, rtPosition, innerSize, backgroundColor, rotation: rotation));
                */
                if (doubleSided) innerSize.Y *= 0.5f;
                Vector2 tileSize = new Vector2(innerSize.X - (borderSize > 0f ? tileSpace * 2f : 0f),
                    (innerSize.Y - (tileSpace * (tiles + (borderSize > 0f ? 1 : 0)))) / tiles);

                float yOffset = 0f;
                if (doubleSided)
                    yOffset = -(tileSize.Y * 0.5f + tileSpace);
                else
                    yOffset = innerSize.Y * 0.5f - tileSize.Y * 0.5f - tileSpace;

                float tileStep = tileSize.Y + tileSpace;

                float step = 1f / tiles;
                Color color;
                if (ratio >= 0f)
                {
                    for (int t = 0; t < Math.Round(ratio / step); t++)
                    {
                        Vector2 tilePosition = new Vector2(0f, -(t * tileStep) + yOffset);
                        tilePosition.Rotate(rotation);

                        getGradientColorLerp(t * step, gradient, out color);
                        addSprite(new MySprite(SpriteType.TEXTURE, tileName, tilePosition + rtPosition, tileSize, color, rotation: rotation));
                    }
                }
                else
                {
                    for (int t = 1; t <= -Math.Round(ratio / step); t++)
                    {
                        Vector2 tilePosition = new Vector2(0f, (t * tileStep) + yOffset);
                        tilePosition.Rotate(rotation);

                        getGradientColorLerp(-t * step, gradient, out color);
                        addSprite(new MySprite(SpriteType.TEXTURE, tileName, tilePosition + rtPosition, tileSize, color, rotation: rotation));
                    }
                }
            }

            protected enum SliderOrientation
            {
                Left,
                Right,
                Top,
                Bottom
            };
            protected void renderSlider(AddSpriteDelegate addSprite, RenderTarget rt, Vector2 position, Vector2 size, bool doubleSided, 
                float ratio, Dictionary<float, Color> barGradient, SliderOrientation sliderOrientation, float sliderWidth, Color sliderColor)
            {
                ratio = !doubleSided ? ((ratio * 2f) - 1f) * 0.5f : ratio * 0.5f;

                bool vertical = sliderOrientation == SliderOrientation.Left || sliderOrientation == SliderOrientation.Right;
                float rotation = vertical ? 0f : (float)Math.PI * 0.5f;
                Vector2 rtPosition = position + rt.DisplayOffset;
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
                        sliderPosition = new Vector2(rtPosition.X - (size.X - sliderSize.X) * 0.5f, rtPosition.Y - ratio * barSize.Y);
                    }
                    else
                    {
                        barPosition = new Vector2(position.X - (size.X - barSize.X) * 0.5f, position.Y);
                        sliderPosition = new Vector2(rtPosition.X + (size.X - sliderSize.X) * 0.5f, rtPosition.Y - ratio * barSize.Y);
                    }
                }
                else
                {
                    sliderSize = new Vector2(sliderWidth * size.X, size.Y * sliderHeightFactor);
                    barSize = new Vector2(size.Y * barHeightFactor, size.X - sliderSize.X);

                    if (sliderOrientation == SliderOrientation.Top)
                    {
                        barPosition = new Vector2(position.X, position.Y + (size.Y - barSize.X) * 0.5f);
                        sliderPosition = new Vector2(rtPosition.X + ratio * barSize.Y, rtPosition.Y - (size.Y - sliderSize.Y) * 0.5f);
                    }
                    else
                    {
                        barPosition = new Vector2(position.X, position.Y - (size.Y - barSize.X) * 0.5f);
                        sliderPosition = new Vector2(rtPosition.X + ratio * barSize.Y, rtPosition.Y + (size.Y - sliderSize.Y) * 0.5f);
                    }
                }

                if (doubleSided == true)
                {
                    Dictionary<float, Color> clamped = new Dictionary<float, Color>();
                    foreach (var pair in barGradient)
                        clamped.Add((pair.Key * 0.5f) + 0.5f, pair.Value);

                    renderSegmentedBar(addSprite, rt, barPosition, barSize, rotation, false, 0, 0f, "", 1f, clamped, 0f, Color.White, new Color(0, 0, 0, 0));
                }
                else
                    renderSegmentedBar(addSprite, rt, barPosition, barSize, rotation, false, 0, 0f, "", 1f, barGradient, 0f, Color.White, new Color(0, 0, 0, 0));

                // draw slider
                if (vertical)
                {
                    if (sliderOrientation == SliderOrientation.Left)
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle,
                            new Vector2(sliderPosition.X - sliderSize.X * 0.5f + sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X + sliderSize.X * 0.5f - sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.Y, sliderSize.Y),
                            sliderColor, rotation:(float)(Math.PI * 0.5f)));
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                            new Vector2(sliderPosition.X - sliderSize.Y * 0.25f, sliderPosition.Y),
                            new Vector2(sliderSize.X - sliderSize.Y * 1.5f, sliderSize.Y),
                            sliderColor));
                    }
                    else
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle,
                            new Vector2(sliderPosition.X + sliderSize.X * 0.5f - sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X - sliderSize.X * 0.5f + sliderSize.Y * 0.5f, sliderPosition.Y),
                            new Vector2(sliderSize.Y, sliderSize.Y),
                            sliderColor, rotation: (float)(Math.PI * 1.5f)));
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                            new Vector2(sliderPosition.X + sliderSize.Y * 0.25f, sliderPosition.Y),
                            new Vector2(sliderSize.X - sliderSize.Y * 1.5f, sliderSize.Y),
                            sliderColor));
                    }
                }
                else
                {
                    if (sliderOrientation == SliderOrientation.Top)
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle,
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.Y * 0.5f + sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.Y * 0.5f - sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X, sliderSize.X),
                            sliderColor, rotation: (float)(Math.PI)));
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.X * 0.25f),
                            new Vector2(sliderSize.X, sliderSize.Y - sliderSize.X * 1.5f),
                            sliderColor));
                    }
                    else
                    {
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle,
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.Y * 0.5f - sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X), sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, "Triangle",
                            new Vector2(sliderPosition.X, sliderPosition.Y - sliderSize.Y * 0.5f + sliderSize.X * 0.5f),
                            new Vector2(sliderSize.X, sliderSize.X),
                            sliderColor));
                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                            new Vector2(sliderPosition.X, sliderPosition.Y + sliderSize.X * 0.25f),
                            new Vector2(sliderSize.X, sliderSize.Y - sliderSize.X * 1.5f),
                            sliderColor));
                    }
                }
            }
            #endregion // Render Bars

            protected class Icon
            {
                public delegate void Render(AddSpriteDelegate addSprite, RenderTarget rt, string name,
                    Vector2 position, Vector2 size, float thickness, float rotation, Color color);

                public static Render getIcon(string name)
                {
                    if (RenderTarget.spriteExist(name))
                        return renderSEIcon;

                    // custom icon
                    switch(name)
                    {
                        case "VIS_Icon_Delimiter":
                            return renderIconDelimiter;
                        case "VIS_Icon_Border":
                        case "VIS_Icon_BorderSimple":
                            return renderIconBorderSimple;
                    }

                    return null;
                }

                static void renderSEIcon(AddSpriteDelegate addSprite, RenderTarget rt, string name,
                    Vector2 position, Vector2 size, float thickness, float rotation, Color color)
                {
                    addSprite(new MySprite(SpriteType.TEXTURE, name, position + rt.DisplayOffset, size, color, rotation: rotation));
                }

                static void renderIconBorderSimple(AddSpriteDelegate addSprite, RenderTarget rt, string name,
                    Vector2 position, Vector2 size, float thickness, float rotation, Color color)
                {
                    Vector2 rtPosition = position + rt.DisplayOffset;
                    Vector2 halfSize = size * 0.5f;
                    float halfThickness = thickness * 0.5f;

                    // top
                    Vector2 pos = new Vector2(0f, -halfSize.Y + halfThickness);
                    pos.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                        pos + rtPosition,
                        new Vector2(size.X, thickness),
                        color, rotation:rotation));

                    // bottom
                    pos = new Vector2(0f, halfSize.Y - halfThickness);
                    pos.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                        pos + rtPosition,
                        new Vector2(size.X, thickness),
                        color, rotation:rotation));

                    // left
                    pos = new Vector2(-halfSize.X + halfThickness, 0f);
                    pos.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                        pos + rtPosition,
                        new Vector2(thickness, size.Y),
                        color, rotation: rotation));

                    // right
                    pos = new Vector2(halfSize.X - halfThickness, 0f);
                    pos.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                        pos + rtPosition,
                        new Vector2(thickness, size.Y),
                        color, rotation: rotation));
                }

                static void renderIconDelimiter(AddSpriteDelegate addSprite, RenderTarget rt, string name,
                    Vector2 position, Vector2 size, float thickness, float rotation, Color color)
                {
                    Vector2 rtPosition = position + rt.DisplayOffset;

                    Vector2 posBar = new Vector2(rtPosition.X + size.Y * 0.5f, rtPosition.Y);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, posBar, new Vector2(size.X - size.Y, size.Y), color, rotation:rotation));

                    Vector2 posEndLeft = new Vector2(-size.X * 0.5f + size.Y * 0.5f, 0f);
                    posEndLeft.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle, posEndLeft + rtPosition, new Vector2(size.Y, size.Y), color));

                    Vector2 posEndRight = new Vector2(size.X * 0.5f - size.Y * 0.5f, 0f);
                    posEndRight.Rotate(rotation);
                    addSprite(new MySprite(SpriteType.TEXTURE, IconNameCircle, posEndRight + rtPosition, new Vector2(size.Y, size.Y), color));
                }
            }
            #endregion // Render Helper
        }
    }
}
