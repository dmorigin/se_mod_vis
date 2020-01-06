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
        public class Default
        {
            public static string Font = "DEBUG";
            public static float FontSize = 0.8f;
            public static Color FontColor = new Color(179, 237, 255);
            public static TextAlignment FontAlignment = TextAlignment.LEFT;
            public static string StringFormat = "#0.00#";

            public static Color Color = Color.White;
            public static Color BackgroundColor = new Color(0, 88, 151);
            public static int ZPosition = 0;
            public static Vector2 Position = new Vector2(0.5f, 0.5f);
            public static Graphic.ValueType PositionType = Graphic.ValueType.Relative;
            public static Vector2 Size = new Vector2(1f, 1f);
            public static Graphic.ValueType SizeType = Graphic.ValueType.Relative;

            public static long AmountItems = 20000;
            public static Color BarBackgroundColor = new Color(179, 237, 255, 50);
            public static bool BarVertical = true;

            public static string DisplayNameTag = "[VIS]";
            public static Vector2I DisplayCoordinate = new Vector2I(0, 0);
            public static int DisplayID = 0;
            public static string EmptyDisplayGroupID = "";

            public static float RefreshInSec = 5.0f;
            public static TimeSpan Refresh = TimeSpan.FromSeconds(RefreshInSec);
            public static TimeSpan ReconstructInterval = TimeSpan.FromSeconds(30.0);
            public static float DCRefreshInSec = 5.0f;
            public static TimeSpan DCRefresh = TimeSpan.FromSeconds(DCRefreshInSec);
            public static int MaxInstructionCount = 7000;

            public static int CharHeight = 29;
            public static int CharWidthMonospace = 24 + 1;
            public static int CharSpaceWidth = 1;
            public static Dictionary<char, int> CharWidths = new Dictionary<char, int>()
            {
                { '.', 9 },
                { '!', 8 },
                { '?', 18 },
                { ',', 9 },
                { ':', 9 },
                { ';', 9 },
                { '"', 10 },
                { '\'', 6 },
                { '+', 18 },
                { '-', 10 },
                { ' ', 14 },

                { '(', 9 },
                { ')', 9 },
                { '[', 9 },
                { ']', 9 },
                { '{', 9 },
                { '}', 9 },

                { '\\', 12 },
                { '/', 14 },
                { '_', 15 },
                { '|', 6 },

                { '~', 18 },
                { '<', 18 },
                { '>', 18 },
                { '=', 18 },

                { '0', 19 },
                { '1', 9 },
                { '2', 19 },
                { '3', 17 },
                { '4', 19 },
                { '5', 19 },
                { '6', 19 },
                { '7', 16 },
                { '8', 19 },
                { '9', 19 },

                { 'A', 21 },
                { 'B', 21 },
                { 'C', 19 },
                { 'D', 21 },
                { 'E', 18 },
                { 'F', 17 },
                { 'G', 20 },
                { 'H', 20 },
                { 'I', 8 },
                { 'J', 16 },
                { 'K', 17 },
                { 'L', 15 },
                { 'M', 26 },
                { 'N', 21 },
                { 'O', 21 },
                { 'P', 20 },
                { 'Q', 21 },
                { 'R', 21 },
                { 'S', 21 },
                { 'T', 17 },
                { 'U', 20 },
                { 'V', 20 },
                { 'W', 31 },
                { 'X', 19 },
                { 'Y', 20 },
                { 'Z', 19 },

                { 'a', 17 },
                { 'b', 17 },
                { 'c', 16 },
                { 'd', 17 },
                { 'e', 17 },
                { 'f', 9 },
                { 'g', 17 },
                { 'h', 17 },
                { 'i', 8 },
                { 'j', 8 },
                { 'k', 17 },
                { 'l', 8 },
                { 'm', 27 },
                { 'n', 17 },
                { 'o', 17 },
                { 'p', 17 },
                { 'q', 17 },
                { 'r', 10 },
                { 's', 17 },
                { 't', 9 },
                { 'u', 17 },
                { 'v', 15 },
                { 'w', 27 },
                { 'x', 15 },
                { 'y', 17 },
                { 'z', 16 }
            };
        }
    }
}
