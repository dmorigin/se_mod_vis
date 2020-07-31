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
        public class GraphicBattery : Graphic
        {
            public GraphicBattery(ContentContainer template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name) => name == "battery";

            #region Configuration
            int cols_ = 0;
            bool configColumns(string key, string value, Configuration.Options options)
            {
                cols_ = Configuration.asInteger(value, 0);
                return true;
            }

            int rows_ = 0;
            bool configRows(string key, string value, Configuration.Options options)
            {
                rows_ = Configuration.asInteger(value, 0);
                return true;
            }

            float margin_ = 4f;
            bool configMargin(string key, string value, Configuration.Options options)
            {
                margin_ = Configuration.asFloat(value, 4f);
                return true;
            }

            Color[] barColors_ =
            {
                Color.Red,          // 0% capacity
                Color.Green,        // 100% capacity
            };

            Color[] borderColors_ =
            {
                Color.Red,              // off
                Color.Red,              // discharging (load)
                Color.Green,            // charging (load)
                new Color(254, 69, 7),  // charge mode == Recharge
                Color.Blue,             // charge mode == Discharge
            };
            bool configBatteryColors(string key, string value, Configuration.Options options)
            {
                switch (value.ToLower())
                {
                    case "onoff":
                        borderColors_[0] = options.asColor(0, Color.Red);
                        break;
                    case "load":
                        borderColors_[1] = options.asColor(0, Color.Red);
                        borderColors_[2] = options.asColor(1, Color.Green);
                        break;
                    case "mode":
                        borderColors_[3] = options.asColor(0, new Color(254, 69, 7));
                        borderColors_[4] = options.asColor(1, Color.Blue);
                        break;
                    case "bar":
                        barColors_[0] = options.asColor(0, Color.Red);
                        barColors_[1] = options.asColor(1, Color.Green);
                        break;
                    default:
                        return false;
                }

                return true;
            }

            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler config = base.getConfigHandler();
                config.add("cols", configColumns);
                config.add("rows", configRows);
                config.add("margin", configMargin);
                config.add("batterycolors", configBatteryColors);
                return config;
            }
            #endregion // Configuration

            #region Rendering
            class RenderDataBattery : RenderDataBase
            {
                public int Rows;
                public int Cols;

                public float Scale;
                public Vector2 Size;

                //public Vector2 renderPosition;
                //public Vector2 renderSize;

                public List<IMyBatteryBlock> Batteries;
            }

            protected override RenderDataBase createRenderDataObj() => new RenderDataBattery();

            //RenderData renderData_ = new RenderData();
            Vector2 batterySize_ = new Vector2(60f, 120f);
            int capacitySegments_ = 6;

            public override void prepareRendering(Display display)
            {
                base.prepareRendering(display);

                RenderDataBattery renderData = RenderData as RenderDataBattery;
                DataCollectorBattery dcBattery = DataCollector as DataCollectorBattery;
                if (dcBattery == null)
                {
                    renderData.Batteries = new List<IMyBatteryBlock>();
                    return;
                }

                renderData.Batteries = dcBattery.Batteries;
                //renderData.renderSize = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                //renderData.renderPosition = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                // calculate rows and cols size
                Vector2 batterySize = batterySize_ + margin_;

                // full automatic
                if (rows_ <= 0 && cols_ <= 0)
                {
                    for (int curCols = renderData.Batteries.Count; curCols > 0; curCols--)
                    {
                        // padding == 2pixel
                        int curRows = (int)Math.Ceiling((double)renderData.Batteries.Count / curCols);
                        Vector2 curSize = new Vector2(renderData.InnerSize.X / curCols, renderData.InnerSize.Y / curRows);
                        float curScale = Math.Min(curSize.X / batterySize.X, curSize.Y / batterySize.Y);

                        if (curScale < renderData.Scale)
                            break;

                        renderData.Scale = curScale;
                        renderData.Size = curSize;
                        renderData.Rows = curRows;
                        renderData.Cols = curCols;
                    }
                }
                else
                {
                    // calculate rows
                    if (rows_ <= 0)
                        renderData.Rows = (int)Math.Ceiling((double)renderData.Batteries.Count / cols_);
                    // calculate cols
                    else if (cols_ <= 0)
                        renderData.Cols = (int)Math.Ceiling((double)renderData.Batteries.Count / rows_);

                    renderData.Size = new Vector2(renderData.InnerSize.X / renderData.Cols, renderData.InnerSize.Y / renderData.Rows);
                    renderData.Scale = Math.Min(renderData.Size.X / batterySize.X, renderData.Size.Y / batterySize.Y);
                }
            }

            public override void render(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                base.render(display, rt, addSprite);

                RenderDataBattery renderData = RenderData as RenderDataBattery;

                float positionX = renderData.Position.X + rt.DisplayOffset.X - (renderData.InnerSize.X * 0.5f) + (renderData.Size.X * 0.5f);
                float positionY = renderData.Position.Y + rt.DisplayOffset.Y - (renderData.InnerSize.Y * 0.5f) + (renderData.Size.Y * 0.5f);
                float offsetX = renderData.Size.X;
                float offsetY = renderData.Size.Y;

                // draw batteries
                for (int r = 0; r < renderData.Rows; r++)
                {
                    for (int c = 0; c < renderData.Cols; c++)
                    {
                        int index = (renderData.Cols * r) + c;
                        if (index >= renderData.Batteries.Count)
                            break;

                        IMyBatteryBlock battery = renderData.Batteries[index];
                        Vector2 position = new Vector2(positionX + (offsetX * c), positionY + (offsetY * r));
                        drawSingleBattery(position, renderData.Scale,
                            battery.CurrentStoredPower / battery.MaxStoredPower,
                            (battery.CurrentInput / battery.MaxInput) - (battery.CurrentOutput / battery.MaxOutput),
                            DataCollectorBase<IMyBatteryBlock>.isOn(battery),
                            battery.ChargeMode, addSprite);
                    }
                }
            }

            void drawSingleBattery(Vector2 position, float scale, float capacity, float load, bool onoff, ChargeMode chargeMode, AddSpriteDelegate addSprite)
            {
                float borderSize = 8f * scale;
                float capacityBorder = borderSize * 0.5f;

                Vector2 poleSize = new Vector2(batterySize_.X * 0.5f, 10f) * scale;
                Vector2 backgroundSize = new Vector2(batterySize_.X * scale, (batterySize_.Y * scale) - poleSize.Y);
                Vector2 InnerSectionSize = backgroundSize - borderSize;

                Color borderColor = onoff == false ? borderColors_[0] : 
                    (chargeMode == ChargeMode.Recharge ? borderColors_[3] : 
                    (chargeMode == ChargeMode.Discharge ? borderColors_[4] :
                    (load <= 0f ? borderColors_[1] : borderColors_[2])));

                // draw plus pole
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                    new Vector2(position.X, position.Y - backgroundSize.Y * 0.5f),
                    poleSize, borderColor));

                // draw background
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    backgroundSize, borderColor));

                // draw inner section
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    InnerSectionSize, Template.BackgroundColor));

                // react on on/off state
                if (onoff == true)
                {
                    // draw capacity marker
                    Vector2 capacitySize = new Vector2(InnerSectionSize.X - capacityBorder * 2f,
                        (InnerSectionSize.Y - (capacityBorder * (capacitySegments_ + 1f))) / capacitySegments_);
                    float capacityYOffset = capacitySize.Y + capacityBorder;
                    float capacityYPosition = position.Y + (poleSize.Y + InnerSectionSize.Y - capacitySize.Y) * 0.5f - capacityBorder;

                    for (int s = 0; s < 6; s++)
                    {
                        float lerp = (1f / capacitySegments_) * s;
                        if (capacity <= lerp)
                            break;

                        addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple,
                            new Vector2(position.X, capacityYPosition - (capacityYOffset * s)),
                            capacitySize, Color.Lerp(barColors_[0], barColors_[1], lerp)));
                    }

                    if (chargeMode == ChargeMode.Recharge)
                        drawChargeModeIndicator(addSprite, new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                            InnerSectionSize.X * 1.3f, (float)(Math.PI * 1.5), borderColor);
                    else if (chargeMode == ChargeMode.Discharge)
                        drawChargeModeIndicator(addSprite, new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                            InnerSectionSize.X * 1.3f, (float)(Math.PI * 0.5), borderColor);
                }
                else
                {
                    addSprite(new MySprite(SpriteType.TEXTURE, "Cross",
                        new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                        new Vector2(InnerSectionSize.X, InnerSectionSize.X) * 0.9f,
                        Color.Red));
                }
            }

            void drawChargeModeIndicator(AddSpriteDelegate addSprite, Vector2 position, float size, float rotation, Color color)
            {
                addSprite(new MySprite(SpriteType.TEXTURE, "AH_BoreSight",
                    new Vector2(position.X, position.Y - size * 0.2f),
                    new Vector2(size, size), color, rotation: rotation));

                addSprite(new MySprite(SpriteType.TEXTURE, "AH_BoreSight",
                    new Vector2(position.X, position.Y + size * 0.2f),
                    new Vector2(size, size), color, rotation: rotation));
            }
            #endregion // Rendering
        }
    }
}
