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
        public class GraphicText : Graphic
        {
            public GraphicText(Template template, Configuration.Options options)
                : base(template, options)
            {
            }


            public override Graphic clone()
            {
                GraphicText gfx = new GraphicText(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataRetriever = DataRetriever;
                gfx.DataRetrieverName = DataRetrieverName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.useDefaultFont_ = useDefaultFont_;
                gfx.useFontSize_ = useFontSize_;
                gfx.useDefaultAlignment_ = useDefaultAlignment_;
                gfx.font_ = font_;
                gfx.alignment_ = alignment_;
                gfx.text_.AddList(text_);

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }


            bool useDefaultFont_ = true;
            bool useFontSize_ = false;
            float fontSize_ = Program.Default.FontSize;
            public float FontSize
            {
                get { return useDefaultFont_ ? Template.FontSize : fontSize_; }
            }


            string font_ = Program.Default.Font;
            public string Font
            {
                get { return useDefaultFont_ ? Template.Font : font_; }
            }


            public Color FontColor
            {
                get { return useDefaultFont_ ? Template.FontColor : Color; }
            }


            bool useDefaultAlignment_ = true;
            TextAlignment alignment_ = Program.Default.FontAlignment;
            public TextAlignment TextAlignment
            {
                get { return useDefaultAlignment_ ? Template.TextAlignment : alignment_; }
            }


            List<string> text_ = new List<string>();


            #region Configuration
            bool configFont(string key, string value, Configuration.Options options)
            {
                font_ = value != string.Empty ? value : Program.Default.Font;
                fontSize_ = options.asFloat(0, 0f);
                if (fontSize_ == 0f)
                    useFontSize_ = false;
                else
                    useFontSize_ = true;

                Color = options.asColor(1, Program.Default.FontColor);
                useDefaultFont_ = false;
                return true;
            }

            bool configText(string key, string value, Configuration.Options options)
            {
                text_.Add(value);
                return true;
            }

            bool configAlignment(string key, string value, Configuration.Options options)
            {
                string data = value.ToLower();
                switch (data)
                {
                    case "center":
                    case "c":
                        alignment_ = TextAlignment.CENTER;
                        break;
                    case "left":
                    case "l":
                        alignment_ = TextAlignment.LEFT;
                        break;
                    case "right":
                    case "r":
                        alignment_ = TextAlignment.RIGHT;
                        break;
                    default:
                        return false;
                }

                useDefaultAlignment_ = false;
                return true;
            }

            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler handler = base.getConfigHandler();
                handler.add("font", configFont);
                handler.add("text", configText);
                handler.add("alignment", configAlignment);

                return handler;
            }
            #endregion // Configuration


            protected override bool supportCheck(string name)
            {
                return true;
            }


            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                float fontSize = FontSize;
                int maxLength = 0;
                foreach(string text in text_)
                {
                    if (text.Length > maxLength)
                        maxLength = text.Length;
                }

                if (!useFontSize_)
                {
                    Vector2 size = SizeType == Graphic.ValueType.Relative ? Size * display.RenderArea.Size : Size;

                    // scale font over size
                    float length = display.FontSize.X * maxLength;
                    float height = display.FontSize.Y * text_.Count;

                    fontSize = Math.Min(size.X / length, size.Y / height);
                }

                Vector2 renderPosition = PositionType == Graphic.ValueType.Relative ? Position * display.RenderArea.Size : Position;
                float positionY = renderPosition.Y - ((rt.FontSize.Y * (text_.Count - 1)) * 0.5f);

                for (int c = 0; c < text_.Count; c++)
                {
                    Graphic.renderTextLine(display, rt, addSprite, Font, fontSize, new Vector2(renderPosition.X, positionY + (c * fontSize)), 
                        FontColor, DataCollector != null ? DataCollector.getText(text_[c]) : text_[c], TextAlignment);
                }
            }
        }
    }
}
