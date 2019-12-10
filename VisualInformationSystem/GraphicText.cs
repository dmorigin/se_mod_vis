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
        public class GraphicText : Graphic
        {
            public GraphicText(Template template, Configuration.Options options)
                : base(template, options)
            {
            }


            bool useDefaultFont_ = true;
            public bool UseDefaultFont
            {
                get { return useDefaultFont_; }
            }

            bool useFontSize_ = false;
            public bool UseFontSize
            {
                get { return useFontSize_; }
            }

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


            string text_ = "";
            public string Text
            {
                get { return text_; }
            }


            #region Configuration
            /*!
             * Setting the font with size and color. The size is a floating point value. Colors
             * are always rgb integer values between 0 ... 255. The alpha value is optional. The
             * values of size and color are optional. If you don't set this values size will be
             * set to 0.0f and color to white. If size is zero the font will scale by size.
             * 
             * Syntax: font:name:size:r,g,b(,a)
             */
            bool configFont(string key, string value, Configuration.Options options)
            {
                font_ = value != string.Empty ? value : Program.Default.Font;
                fontSize_ = options.getAsFloat(0, 0f);
                if (fontSize_ == 0f)
                    useFontSize_ = false;
                else
                    useFontSize_ = true;

                Color = options.getAsColor(1, Program.Default.FontColor);
                useDefaultFont_ = false;
                return true;
            }


            /*!
             * Set the text that you want to display.
             * 
             * Syntax: text:string
             */
            bool configText(string key, string value, Configuration.Options options)
            {
                text_ += $"{value}\n";
                return true;
            }


            /*!
             * Set the alignment of a text.
             * 
             * Syntax: alignment:c|center|l|left|r|right
             */
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
                int lines = Text.Count<char>(c => c == '\n') + 1;

                if (!UseFontSize)
                {
                    Vector2 size = SizeType == Graphic.ValueType.Relative ? Size * display.RenderArea.Size : Size;

                    // scale font over size
                    float length = display.FontSize.X * Text.Length;
                    float height = display.FontSize.Y * lines;

                    fontSize = Math.Min(size.X / length, size.Y / height);
                }

                // fix font position
                //Vector2 offset = new Vector2(rt.DisplayOffset.X, rt.DisplayOffset.Y - (0.75f * (fontSize * rt.FontSize.Y) * lines));
                Vector2 position = PositionType == Graphic.ValueType.Relative ? Position * display.RenderArea.Size : Position;

                /*
                MySprite sprite = MySprite.CreateText(DataCollector != null ? DataCollector.getText(Text) : Text, Font, FontColor, fontSize, TextAlignment);
                sprite.Position = position + offset;
                addSprite(sprite);
                */
                Graphic.renderTextLine(rt, addSprite, Font, fontSize, position, FontColor, DataCollector != null ? DataCollector.getText(Text) : Text, TextAlignment);
            }
        }
    }
}
