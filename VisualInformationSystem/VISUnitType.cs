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
            Watt,
            WattHour,
            Liter,
            Gram,
            Percent,
            Gravity,
            Speed,
            Meter
        }

        public enum Multiplier
        {
            None,
            m, // milli
            K, // Kilo
            M, // Million
            G, // Giga
            T, // Tera
            P // Peta
        }


        public struct VISUnitType
        {
            public VISUnitType(double value, Multiplier multiplier = Multiplier.None, Unit unit = Unit.None)
            {
                Value = value;
                Multiplier = multiplier;
                Unit = unit;
            }

            public double Value
            {
                get;
                set;
            }

            public Multiplier Multiplier
            {
                get;
                set;
            }

            public Unit Unit
            {
                get;
                set;
            }

            public static implicit operator int(VISUnitType ut) => (int)ut.Value;
            public static implicit operator long(VISUnitType ut) => (long)ut.Value;
            public static implicit operator double(VISUnitType ut) => ut.Value;
            public static implicit operator float(VISUnitType ut) => (float)ut.Value;

            public static implicit operator string(VISUnitType ut) => ut.ToString();

            public static bool operator >(VISUnitType ut, double d) => ut.Value > d;
            public static bool operator <(VISUnitType ut, double d) => ut.Value < d;
            public static bool operator >=(VISUnitType ut, double d) => ut.Value >= d;
            public static bool operator <=(VISUnitType ut, double d) => ut.Value <= d;

            public override string ToString()
            {
                string unit = UnitToString(Unit);
                string multiplier = MultiplierToString(Multiplier);
                double value = Value;

                // fix g
                if (Unit == Unit.Gram)
                {
                    switch (Multiplier)
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
                else if (Unit == Unit.Percent)
                    value *= 100.0;

                return $"{value.ToString(Default.StringFormat)}{multiplier}{unit}";
            }

            public string UnitToString(Unit unit)
            {
                switch(unit)
                {
                    case Unit.Watt:
                        return "W";
                    case Unit.WattHour:
                        return "Wh";
                    case Unit.Liter:
                        return "L";
                    case Unit.Gram:
                        return "g";
                    case Unit.Percent:
                        return "%";
                    case Unit.Gravity:
                        return "G";
                    case Unit.Speed:
                        return "m/s";
                    case Unit.Meter:
                        return "m";
                }

                return "";
            }

            public string MultiplierToString(Multiplier multiplier)
            {
                switch(multiplier)
                {
                    case Multiplier.m:
                        return "m";
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

            public VISUnitType pack()
            {
                double value = Value;
                Multiplier multiplier = Multiplier;

                while (value >= 1000.0 && multiplier != Multiplier.P)
                {
                    value /= 1000.0;
                    multiplier = getNextHeigher(multiplier);
                }

                while (value < 0.1 && multiplier != Multiplier.m)
                {
                    value *= 1000.0;
                    multiplier = getNextLower(multiplier);
                }

                return new VISUnitType(value, multiplier, Unit);
            }

            private Multiplier getNextHeigher(Multiplier multiplier)
            {
                switch (multiplier)
                {
                    case Multiplier.m:
                        return Multiplier.None;
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
                    case Multiplier.None:
                        return Multiplier.m;
                }

                return multiplier;
            }
        }
    }
}
