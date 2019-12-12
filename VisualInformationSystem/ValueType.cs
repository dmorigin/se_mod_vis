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
        public enum Unit
        {
            None,
            W,
            Wh,
            l,
            g
        }

        public enum Multiplier
        {
            None,
            K, // Kilo
            M, // Million
            G, // Giga
            T, // Tera
            P // Peta
        }


        public struct ValueType
        {
            public ValueType(double value, Multiplier multiplier = Multiplier.None, Unit unit = Unit.None)
            {
                value_ = value;
                multiplier_ = multiplier;
                unit_ = unit;
            }

            double value_;
            public double Value
            {
                get { return value_; }
                set { value_ = value; }
            }

            Multiplier multiplier_;
            public Multiplier Multiplier
            {
                get { return multiplier_; }
                set { multiplier_ = value; }
            }

            Unit unit_;
            public Unit Unit
            {
                get { return unit_; }
                set { unit_ = value; }
            }

            public static implicit operator long(ValueType vt) => (long)vt.value_;
            public static implicit operator double(ValueType vt) => vt.value_;
            public static implicit operator float(ValueType vt) => (float)vt.value_;

            public override string ToString()
            {
                string unit = UnitToString(unit_);
                string multiplier = MultiplierToString(multiplier_);

                // fix g
                if (unit_ == Unit.g)
                {
                    switch(multiplier_)
                    {
                        case Multiplier.M:
                            unit = "T";
                            multiplier = "";
                            break;
                        case Multiplier.G:
                            unit = "T";
                            multiplier = "M";
                            break;
                        case Multiplier.T:
                            unit = "T";
                            multiplier = "G";
                            break;
                    }
                }

                return $"{value_.ToString(Program.Default.StringFormat)}{multiplier}{unit}";
            }

            public string UnitToString(Unit unit)
            {
                switch(unit)
                {
                    case Unit.W:
                        return "W";
                    case Unit.Wh:
                        return "Wh";
                    case Unit.l:
                        return "l";
                    case Unit.g:
                        return "g";
                }

                return "";
            }

            public string MultiplierToString(Multiplier multiplier)
            {
                switch(multiplier)
                {
                    case Multiplier.K:
                        return "k";
                    case Multiplier.M:
                        return "M";
                    case Multiplier.G:
                        return "G";
                    case Multiplier.T:
                        return "T";
                    case Multiplier.P:
                        return "P";
                }

                return "";
            }

            public ValueType pack()
            {
                double value = value_;
                Multiplier multiplier = multiplier_;

                while (value >= 1000 && multiplier != Multiplier.P)
                {
                    value /= 1000;
                    multiplier = getNextHeigher(multiplier);
                }

                while (value < 0 && multiplier != Multiplier.None)
                {
                    value *= 1000;
                    multiplier = getNextLower(multiplier);
                }

                return new ValueType(value, multiplier, unit_);
            }

            private Multiplier getNextHeigher(Multiplier multiplier)
            {
                switch (multiplier)
                {
                    case Multiplier.None:
                        return Multiplier.K;
                    case Multiplier.K:
                        return Multiplier.M;
                    case Multiplier.M:
                        return Multiplier.G;
                    case Multiplier.G:
                        return Multiplier.T;
                    case Multiplier.T:
                        return Multiplier.P;
                }

                return multiplier;
            }

            private Multiplier getNextLower(Multiplier multiplier)
            {
                switch (multiplier)
                {
                    case Multiplier.P:
                        return Multiplier.T;
                    case Multiplier.T:
                        return Multiplier.G;
                    case Multiplier.G:
                        return Multiplier.M;
                    case Multiplier.M:
                        return Multiplier.K;
                    case Multiplier.K:
                        return Multiplier.None;
                }

                return multiplier;
            }
        }
    }
}
