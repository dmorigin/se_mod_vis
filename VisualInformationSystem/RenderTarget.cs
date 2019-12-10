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
        public class RenderTarget : VISObject
        {
            IMyTextSurface surface_ = null;

            Vector2 renderPosition_ = new Vector2();
            public Vector2 Position
            {
                get { return renderPosition_; }
            }

            Vector2 renderSize_ = new Vector2();
            public Vector2 Size
            {
                get { return renderSize_; }
            }

            Vector2 fontSize_ = new Vector2();
            public Vector2 FontSize
            {
                get { return fontSize_; }
            }


            Vector2I coordinate_ = new Vector2I();
            public Vector2I Coordinate
            {
                get { return coordinate_; }
            }


            Vector2 displayOffset_ = new Vector2();
            public Vector2 DisplayOffset
            {
                get { return displayOffset_; }
            }


            public Color BackgroundColor
            {
                get { return surface_.ScriptBackgroundColor; }
                set { surface_.ScriptBackgroundColor = value; }
            }

/*
            Display display_ = null;
            public Display Display2
            {
                get { return display_; }
            }


            public RenderTarget(Display display, Vector2I coordinate)
                : base(display.App)
            {
                display_ = display;
                coordinate_ = coordinate;
            }
*/

            public RenderTarget(Vector2I coordinate)
            {
                coordinate_ = coordinate;
            }

            public void setupSurface(IMyTextSurface surface)
            {
                releaseSurface();

                surface_ = surface;
                surface_.WriteText("");
                surface_.Script = "";
                surface_.ContentType = ContentType.SCRIPT;
                surface_.Font = Program.Default.Font;
                surface_.FontSize = 1f;
                surface_.TextPadding = 0f;

                surface_.BackgroundColor = Color.Black;
                surface_.ScriptBackgroundColor = Color.Black;

                renderSize_ = surface_.SurfaceSize;
                renderPosition_ = (surface_.TextureSize - renderSize_) * 0.5f;

                displayOffset_ = renderSize_ * coordinate_ + renderPosition_;

                adjustFontSize(Program.Default.Font, 1f);
                drawInitScreen();
            }


            public Vector2 adjustFontSize(string font, float fontSize)
            {
                //fontSize_ = surface_.MeasureStringInPixels(new StringBuilder("M"), font, fontSize);
                fontSize_ = new Vector2(21f, 22f) * fontSize;
                return fontSize_;
            }


            public void releaseSurface()
            {
                if (surface_ == null)
                    return;

                surface_.WriteText("");
                surface_.Script = "";
                surface_.ContentType = ContentType.NONE;
                surface_.TextPadding = 2f;
                surface_.Font = "DEBUG";
                surface_.FontSize = 1f;
                surface_.BackgroundColor = Color.Black;
                surface_.FontColor = Color.White;
                surface_.Alignment = TextAlignment.LEFT;

                surface_ = null;
            }


            public MySpriteDrawFrame getRenderFrame()
            {
                return surface_.DrawFrame();
            }


            public bool spriteExist(string name)
            {
                List<string> sprites = new List<string>();
                surface_.GetSprites(sprites);
                return sprites.Exists(x => x == name);
            }


            void drawInitScreen()
            {
                using (MySpriteDrawFrame frame = getRenderFrame())
                {
                    MySprite textInit = MySprite.CreateText("Initilize screen", "debug", Color.LawnGreen, 1f, TextAlignment.LEFT);
                    textInit.Position = new Vector2(5f, fontSize_.Y) + renderPosition_;
                    frame.Add(textInit);
                }
            }
        }
    }
}
