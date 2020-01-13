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
        public class VISItemType : IEquatable<VISItemType>, IEquatable<MyItemType>
        {
            public static MyItemType EmptyType = new MyItemType();
            public static VISItemType Empty = new VISItemType();

            public MyItemType Type { get; protected set; }
            public bool Valid { get; set; }

            private VISItemType()
            {
                Type = EmptyType;
                Valid = false;
            }

            public VISItemType(MyItemType type)
            {
                Type = type;
                Valid = type != EmptyType;
            }

            public VISItemType(string typeId, string subtypeId)
            {
                try
                {
                    Type = new MyItemType(typeId, subtypeId);
                    Valid = true;
                }
                catch (Exception)
                {
                    Valid = false;
                    Type = new MyItemType();
                }
            }

            public VISItemType(string type)
            {
                try
                {
                    Type = MyItemType.Parse(type);
                    Valid = true;
                }
                catch (Exception)
                {
                    Valid = false;
                    Type = EmptyType;
                }
            }

            public static bool compareItemTypes(MyItemType a, MyItemType b)
            {
                if (a.SubtypeId != "" && b.SubtypeId != "")
                    return a.TypeId == b.TypeId && a.SubtypeId == b.SubtypeId;
                return a.TypeId == b.TypeId;
            }

            public bool Equals(VISItemType other) => VISItemType.compareItemTypes(this.Type, other.Type);
            public bool Equals(MyItemType other) => VISItemType.compareItemTypes(this.Type, other);
            public override bool Equals(object obj) => Equals(obj as VISItemType);
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => $"{Type.TypeId}/{Type.SubtypeId}";

            public static implicit operator VISItemType(MyItemType type) => new VISItemType(type);
            public static implicit operator VISItemType(string type) => new VISItemType(type);
            public static implicit operator MyItemType(VISItemType type) => type.Type;
            public static implicit operator bool(VISItemType type) => type.Valid;
            public static implicit operator string(VISItemType type) => type.ToString();

            public static bool operator ==(VISItemType a, VISItemType b) => a.Equals(b);
            public static bool operator !=(VISItemType a, VISItemType b) => !a.Equals(b);
        }
    }
}
