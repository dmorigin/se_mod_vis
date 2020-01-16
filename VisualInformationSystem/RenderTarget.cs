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

            public Vector2 Position
            {
                get;
                private set;
            }

            public Vector2 Size
            {
                get;
                private set;
            }

            public Vector2I Coordinate
            {
                get;
                private set;
            }

            public Vector2 DisplayOffset
            {
                get;
                private set;
            }

            public Color BackgroundColor
            {
                get { return surface_.ScriptBackgroundColor; }
                set { surface_.ScriptBackgroundColor = value; }
            }

            public RenderTarget(Vector2I coordinate)
            {
                Coordinate = coordinate;
            }

            public void setupSurface(IMyTextSurface surface)
            {
                releaseSurface();

                surface_ = surface;
                surface_.WriteText("");
                surface_.Script = "";
                surface_.ContentType = ContentType.SCRIPT;
                surface_.Font = Default.Font;
                surface_.FontSize = 1f;
                surface_.TextPadding = 0f;

                surface_.BackgroundColor = Color.Black;
                surface_.ScriptBackgroundColor = Color.Black;

                Size = surface_.SurfaceSize;
                Position = (surface_.TextureSize - Size) * 0.5f;

                DisplayOffset = -(Size * Coordinate) + Position;

                if (RenderTarget.sprites_.Count == 0)
                    surface_.GetSprites(RenderTarget.sprites_);

                drawInitScreen();
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

            static List<string> sprites_ = new List<string>();
            public static bool spriteExist(string name)
            {
                return RenderTarget.sprites_.Exists(x => x == name);
            }

            void drawInitScreen()
            {
                using (MySpriteDrawFrame frame = getRenderFrame())
                {
                    MySprite textInit = MySprite.CreateText("Initilize screen", "debug", Color.LawnGreen, 1f, TextAlignment.LEFT);
                    textInit.Position = new Vector2(5f, Default.CharHeight) + Position;
                    frame.Add(textInit);
                }
            }
        }
    }
}
