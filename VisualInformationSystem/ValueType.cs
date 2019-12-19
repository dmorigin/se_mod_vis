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
            g,
            Percent
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

            public static implicit operator int(ValueType vt) => (int)vt.value_;
            public static implicit operator long(ValueType vt) => (long)vt.value_;
            public static implicit operator double(ValueType vt) => vt.value_;
            public static implicit operator float(ValueType vt) => (float)vt.value_;

            public static bool operator >(ValueType vt, double d) => vt.value_ > d;
            public static bool operator <(ValueType vt, double d) => vt.value_ < d;
            public static bool operator >=(ValueType vt, double d) => vt.value_ >= d;
            public static bool operator <=(ValueType vt, double d) => vt.value_ <= d;

            public override string ToString()
            {
                string unit = UnitToString(unit_);
                string multiplier = MultiplierToString(multiplier_);
                double value = value_;

                // fix g
                if (unit_ == Unit.g)
                {
                    switch (multiplier_)
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
                else if (unit_ == Unit.Percent)
                    value *= 100.0;

                return $"{value.ToString(Program.Default.StringFormat)}{multiplier}{unit}";
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
                        return "L";
                    case Unit.g:
                        return "g";
                    case Unit.Percent:
                        return "%";
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

                while (value >= 1000.0 && multiplier != Multiplier.P)
                {
                    value /= 1000.0;
                    multiplier = getNextHeigher(multiplier);
                }

                while (value < 0.0 && multiplier != Multiplier.None)
                {
                    value *= 1000.0;
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
