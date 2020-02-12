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
        public class RenderTargetID : IEquatable<RenderTargetID>
        {
            public RenderTargetID(string typeId, string subtypeId, int index)
            {
                typeId_ = typeId;
                subtypeId_ = subtypeId;
                index_ = index;
            }

            string typeId_ = "";
            string subtypeId_ = "";
            int index_ = 0;

            public static RenderTargetID Invalid = new RenderTargetID("", "", -1);

            public static RenderTargetID fromString(string str)
            {
                int slash = str.IndexOf('/');
                int colon = str.LastIndexOf(':');

                int index = 0;
                if (slash < 0 || colon < slash || !int.TryParse(str.Substring(colon + 1), out index))
                    return Invalid;

                return new RenderTargetID(str.Substring(0, slash), str.Substring(slash + 1, colon - slash - 1), index);
            }

            public static RenderTargetID fromSurfaceProvider(IMyTextSurfaceProvider provider, int index)
            {
                IMyTerminalBlock block = provider as IMyTerminalBlock;
                if (block != null)
                    return new RenderTargetID(block.BlockDefinition.TypeIdString, block.BlockDefinition.SubtypeId, index);

                return Invalid;
            }

            public static bool tryGetFixed(RenderTargetID id, out Vector2 size)
            {
                var list = Default.RenderTargetFixSize.ToList();
                int index = list.FindIndex((pair) => pair.Key.Equals(id));
                if (index >= 0)
                {
                    size = list[index].Value;
                    return true;
                }

                size = new Vector2();
                return false;
            }

            public static implicit operator RenderTargetID(string str) => fromString(str);
            public bool Equals(RenderTargetID other) => typeId_ == other.typeId_ && subtypeId_ == other.subtypeId_ && index_ == other.index_;
            public override string ToString() => $"{typeId_}/{subtypeId_}:{index_}";
        }
    }
}
