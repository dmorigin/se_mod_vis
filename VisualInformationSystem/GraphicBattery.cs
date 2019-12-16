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
                gfx.construct();

                gfx.DataCollector = DataCollector;
                gfx.DataRetriever = DataRetriever;
                gfx.DataRetrieverName = DataRetrieverName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                gfx.rows_ = rows_;
                gfx.cols_ = cols_;

                return gfx;
            }


            #region Configuration
            int cols_ = 0;
            int rows_ = 0;

            bool configColumns(string key, string value, Configuration.Options options)
            {
                cols_ = Configuration.asInteger(value, 0);
                return true;
            }

            bool configRows(string key, string value, Configuration.Options options)
            {
                rows_ = Configuration.asInteger(value, 0);
                return true;
            }

            public override ConfigHandler getConfigHandler()
            {
                ConfigHandler config = base.getConfigHandler();
                config.add("cols", configColumns);
                config.add("rows", configRows);
                return config;
            }
            #endregion // Configuration


            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                DataCollectorBattery batteries = DataCollector as DataCollectorBattery;
                if (batteries == null)
                    return;

                Vector2 renderSize = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;
                Vector2 renderPosition = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;

                List<DataRetriever.ListContainer> capacityList;
                List<DataRetriever.ListContainer> inoutList;
                batteries.getDataRetriever("capacity").list(out capacityList);
                batteries.getDataRetriever("inout").list(out inoutList);
                    
                // calculate rows and cols size
                int rows = rows_;
                int cols = cols_;
                float scale = 0.0f;
                Vector2 size = new Vector2();

                // full automatic
                if (rows_ <= 0 && cols_ <= 0)
                {
                    for(int curCols = capacityList.Count; curCols > 0; curCols--)
                    {
                        // padding == 2pixel
                        int curRows = (int)Math.Ceiling((double)capacityList.Count / curCols);
                        Vector2 curSize = new Vector2((renderSize.X - (curCols * 2)) / curCols, (renderSize.Y - (curRows * 2)) / curRows);
                        float curScale = Math.Min(curSize.X / batterySize_.X, curSize.Y / batterySize_.Y);

                        if (curScale < scale)
                            break;

                        scale = curScale;
                        size = curSize;
                        rows = curRows;
                        cols = curCols;
                    }
                }
                else
                {
                    // calculate rows
                    if (rows <= 0)
                        rows = (int)Math.Ceiling((double)capacityList.Count / cols);
                    // calculate cols
                    else if (cols <= 0)
                        cols = (int)Math.Ceiling((double)capacityList.Count / rows);

                    size = new Vector2((renderSize.X - (cols * 2)) / cols, (renderSize.Y - (rows * 2)) / rows);
                    scale = Math.Min(size.X / batterySize_.X, size.Y / batterySize_.Y);
                }

                float positionX = renderPosition.X + rt.DisplayOffset.X + 2f - (renderSize.X * 0.5f) + (size.X * 0.5f);
                float positionY = renderPosition.Y + rt.DisplayOffset.Y + 2f - (renderSize.Y * 0.5f) + (size.Y * 0.5f);
                float offsetX = size.X + 2f;
                float offsetY = size.Y + 2f;

                // draw batteries
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int index = (cols * r) + c;
                        if (index >= capacityList.Count)
                            break;

                        Vector2 position = new Vector2(positionX + (offsetX * c), positionY + (offsetY * r));
                        drawSingleBattery(position, scale, (float)capacityList[index].indicator,
                            (float)inoutList[index].indicator, inoutList[index].onoff, addSprite);
                    }
                }
            }

            Vector2 batterySize_ = new Vector2(60f, 120f);
            int capacitySegments_ = 6;

            void drawSingleBattery(Vector2 position, float scale, float capacity, float load, bool onoff, AddSpriteDelegate addSprite)
            {
                float borderSize = 8f * scale;
                float capacityBorder = borderSize * 0.5f;

                Vector2 poleSize = new Vector2(batterySize_.X * 0.5f, 10f) * scale;
                Vector2 backgroundSize = new Vector2(batterySize_.X * scale, (batterySize_.Y * scale) - poleSize.Y);
                Vector2 InnerSectionSize = backgroundSize - borderSize;

                Color borderColor = load <= 0f || onoff == false ? Color.Red : Color.Green;

                // draw plus pole
                MySprite sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y - backgroundSize.Y * 0.5f),
                    poleSize, borderColor);
                addSprite(sprite);

                // draw background
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    backgroundSize, borderColor);
                addSprite(sprite);

                // draw inner section
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    InnerSectionSize, Template.BackgroundColor);
                addSprite(sprite);

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

                        sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                            new Vector2(position.X, capacityYPosition - (capacityYOffset * s)),
                            capacitySize, Color.Lerp(Color.Red, Color.Green, lerp));
                        addSprite(sprite);
                    }
                }
                else
                {
                    sprite = new MySprite(SpriteType.TEXTURE, "Cross",
                        new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                        new Vector2(InnerSectionSize.X, InnerSectionSize.X) * 0.9f,
                        Color.Red);
                    addSprite(sprite);
                }
            }
        }
    }
}
