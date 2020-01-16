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
            public GraphicBattery(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            protected override bool supportCheck(string name)
            {
                if (name == "battery")
                    return true;
                return false;
            }

            public override Graphic clone()
            {
                GraphicBattery gfx = new GraphicBattery(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataAccessor = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;
                gfx.VisibleThreshold = VisibleThreshold;
                gfx.VisibleOperator = VisibleOperator;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.rows_ = rows_;
                gfx.cols_ = cols_;
                gfx.margin_ = margin_;

                gfx.barColors_[0] = barColors_[0];
                gfx.barColors_[1] = barColors_[1];

                gfx.borderColors_[0] = borderColors_[0];
                gfx.borderColors_[1] = borderColors_[1];
                gfx.borderColors_[2] = borderColors_[2];
                gfx.borderColors_[3] = borderColors_[3];
                gfx.borderColors_[4] = borderColors_[4];

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

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
            struct RenderData
            {
                public int rows;
                public int cols;

                public float scale;
                public Vector2 size;

                public Vector2 renderPosition;
                public Vector2 renderSize;

                public List<IMyBatteryBlock> batteries;
            }

            RenderData renderData_ = new RenderData();
            Vector2 batterySize_ = new Vector2(60f, 120f);
            int capacitySegments_ = 6;

            public override void prepareRendering(Display display)
            {
                DataCollectorBattery dcBattery = DataCollector as DataCollectorBattery;
                if (dcBattery == null)
                {
                    renderData_.batteries = new List<IMyBatteryBlock>();
                    return;
                }

                renderData_.batteries = dcBattery.Batteries;
                renderData_.renderSize = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                renderData_.renderPosition = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                // calculate rows and cols size
                Vector2 batterySize = batterySize_ + margin_;

                // full automatic
                if (rows_ <= 0 && cols_ <= 0)
                {
                    for (int curCols = renderData_.batteries.Count; curCols > 0; curCols--)
                    {
                        // padding == 2pixel
                        int curRows = (int)Math.Ceiling((double)renderData_.batteries.Count / curCols);
                        Vector2 curSize = new Vector2(renderData_.renderSize.X / curCols, renderData_.renderSize.Y / curRows);
                        float curScale = Math.Min(curSize.X / batterySize.X, curSize.Y / batterySize.Y);

                        if (curScale < renderData_.scale)
                            break;

                        renderData_.scale = curScale;
                        renderData_.size = curSize;
                        renderData_.rows = curRows;
                        renderData_.cols = curCols;
                    }
                }
                else
                {
                    // calculate rows
                    if (rows_ <= 0)
                        renderData_.rows = (int)Math.Ceiling((double)renderData_.batteries.Count / cols_);
                    // calculate cols
                    else if (cols_ <= 0)
                        renderData_.cols = (int)Math.Ceiling((double)renderData_.batteries.Count / rows_);

                    renderData_.size = new Vector2(renderData_.renderSize.X / renderData_.cols, renderData_.renderSize.Y / renderData_.rows);
                    renderData_.scale = Math.Min(renderData_.size.X / batterySize.X, renderData_.size.Y / batterySize.Y);
                }
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                float positionX = renderData_.renderPosition.X + rt.DisplayOffset.X - (renderData_.renderSize.X * 0.5f) + (renderData_.size.X * 0.5f);
                float positionY = renderData_.renderPosition.Y + rt.DisplayOffset.Y - (renderData_.renderSize.Y * 0.5f) + (renderData_.size.Y * 0.5f);
                float offsetX = renderData_.size.X;
                float offsetY = renderData_.size.Y;

                // draw batteries
                for (int r = 0; r < renderData_.rows; r++)
                {
                    for (int c = 0; c < renderData_.cols; c++)
                    {
                        int index = (renderData_.cols * r) + c;
                        if (index >= renderData_.batteries.Count)
                            break;

                        IMyBatteryBlock battery = renderData_.batteries[index];
                        Vector2 position = new Vector2(positionX + (offsetX * c), positionY + (offsetY * r));
                        drawSingleBattery(position, renderData_.scale,
                            battery.CurrentStoredPower / battery.MaxStoredPower,
                            (battery.CurrentInput / battery.MaxInput) - (battery.CurrentOutput / battery.MaxOutput),
                            DataCollector<IMyBatteryBlock>.isOn(battery),
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
                addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y - backgroundSize.Y * 0.5f),
                    poleSize, borderColor));

                // draw background
                addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    backgroundSize, borderColor));

                // draw inner section
                addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
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

                        addSprite(new MySprite(SpriteType.TEXTURE, "SquareSimple",
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
