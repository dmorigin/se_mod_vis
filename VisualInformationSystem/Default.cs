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
        public class Default
        {
            public static string Font = "DEBUG";
            public static float FontSize = 1.0f;
            public static Color FontColor = Color.LawnGreen;
            public static TextAlignment FontAlignment = TextAlignment.LEFT;
            public static string StringFormat = "#0.000";

            public static Color Color = Color.White;
            public static Color BackgroundColor = Color.Black;
            public static Vector2 Position = new Vector2(0.5f, 0.5f);
            public static Graphic.ValueType PositionType = Graphic.ValueType.Relative;
            public static Vector2 Size = new Vector2(1f, 1f);
            public static Graphic.ValueType SizeType = Graphic.ValueType.Relative;

            public static string DisplayNameTag = "[VIS]";
            public static Vector2I DisplayCoordinate = new Vector2I(0, 0);
            public static int DisplayID = 0;
            public static string EmptyDisplayGroupID = "";

            public static TimeSpan UpdateInterval = TimeSpan.FromSeconds(5.0);
            public static float UpdateIntervalInSec = 5.0f;
            public static TimeSpan ReconstructInterval = TimeSpan.FromSeconds(5.0);
        }
    }
}
