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
            public static float FontSize = 0.8f;
            public static Color FontColor = new Color(179, 237, 255);
            public static TextAlignment FontAlignment = TextAlignment.LEFT;
            public static string StringFormat = "#0.00#";
            public static Graphic.ValueType ValueType = Graphic.ValueType.Relative;

            public static Color Color = Color.White;
            public static Color BackgroundColor = new Color(0, 88, 151);
            public static Vector2 Position = new Vector2(0.5f, 0.5f);
            public static Graphic.ValueType PositionType = ValueType;
            public static Vector2 Size = new Vector2(1f, 1f);
            public static Graphic.ValueType SizeType = ValueType;
            public static float Thickness = 0.01f;

            public static Color BarColor = new Color(179, 237, 255);
            public static Color BarBackgroundColor = new Color(179, 237, 255, 50);
            public static Color BarBorderColor = new Color(179, 237, 255);
            public static float BarBorderSize = 0f;
            public static float BarTileSpace = 0.04f;
            public static Graphic.ValueType BarTileSpaceType = ValueType;
            public static int BarTileCount = 10;
            public static string BarTileIcon = IconNameSquareSimple;
            public static float BarRotation = 0f;

            public static float ListBarThickness = 1f;
            public static Graphic.ValueType ListBarThicknessType = ValueType;
            public static float MaxShipSpeed = 100.0f;

            public static string DisplayNameTag = "[VIS]";
            public static Vector2I DisplayCoordinate = new Vector2I(0, 0);
            public static int DisplayID = 0;
            public static string EmptyDisplayGroupID = "";
            public static string MyObjectBuilder = "MyObjectBuilder";
            public static string Component = $"{MyObjectBuilder}_Component";

            public static Dictionary<RenderTargetID, RectangleF> RenderTargetFixSize = new Dictionary<RenderTargetID, RectangleF>();

            public static float RefreshInSec = 5.0f;
            public static TimeSpan Refresh = TimeSpan.FromSeconds(RefreshInSec);
            public static float ReconstructIntervalInSec = 30f;
            public static TimeSpan ReconstructInterval = TimeSpan.FromSeconds(ReconstructIntervalInSec);
            public static TimeSpan WatchConnectorInterval = TimeSpan.FromSeconds(2.0);//TimeSpan.FromSeconds(1.0);
            public static float DCRefreshInSec = 5.0f;
            public static TimeSpan DCRefresh = TimeSpan.FromSeconds(DCRefreshInSec);
            public static int MaxInstructionCount = 7000;
            public static int ExceptionRetry = 3;

            public static long MaxAmountItems = 1000;
            public static Dictionary<VISItemType, long> AmountItems = new Dictionary<VISItemType, long>()
            {
                // individual amount values
                { $"{MyObjectBuilder}_Ore/Ice", 200000 },
                { $"{MyObjectBuilder}_Ore/Stone", 20000 },
                { $"{MyObjectBuilder}_Ore/Iron", 200000 },

                { $"{Component}/SteelPlate", 10000 },
                { $"{Component}/Medical", 500 },
                { $"{Component}/Motor", 6000 },
                { $"{Component}/InteriorPlate", 10000 },
                { $"{Component}/Construction", 10000 },
                { $"{Component}/ZoneChip", 20 },

                { $"{MyObjectBuilder}_OxygenContainerObject/OxygenBottle", 5 },
                { $"{MyObjectBuilder}_GasContainerObject/HydrogenBottle", 5 },

                // amount values for type groups
                { $"{MyObjectBuilder}_AmmoMagazine/", 1000 },
                { $"{MyObjectBuilder}_Component/", 4000 },
                { $"{MyObjectBuilder}_PhysicalGunObject/", 20 },
                { $"{MyObjectBuilder}_Ore/", 200000 },
                { $"{MyObjectBuilder}_Ingot/", 40000 },
                { $"{MyObjectBuilder}_ConsumableItem/", 100 },
                { $"{MyObjectBuilder}_PhysicalObject/", 2000 },
                { $"{MyObjectBuilder}_Datapad/", 30 },
                { $"{MyObjectBuilder}_Package/", 100 }
            };

            public static Dictionary<string, VISItemType> ItemTypeMap = new Dictionary<string, VISItemType>()
            {
                { "ammo", $"{MyObjectBuilder}_AmmoMagazine/" },
                { "component", $"{MyObjectBuilder}_Component/" },
                { "handtool", $"{MyObjectBuilder}_PhysicalGunObject/" },
                { "ore", $"{MyObjectBuilder}_Ore/" },
                { "ingot", $"{MyObjectBuilder}_Ingot/" },
                { "consumable", $"{MyObjectBuilder}_ConsumableItem/" },
                { "ice", $"{MyObjectBuilder}_Ore/Ice" },
                { "uranium", $"{MyObjectBuilder}_Ingot/Uranium" }
            };

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
