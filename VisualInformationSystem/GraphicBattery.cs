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
                if (DataCollector != null)
                {
                    DataCollectorBattery batteries = DataCollector as DataCollectorBattery;
                    if (batteries == null)
                        return;

                    List<DataRetriever.ListContainer> capacityList;
                    List<DataRetriever.ListContainer> inoutList;
                    batteries.getDataRetriever("capacity").getList(out capacityList);
                    batteries.getDataRetriever("inout").getList(out inoutList);

                    // calculate rows and cols size
                    int rows = rows_;
                    int cols = cols_;
                    float scale = 0f;
                    Vector2 size = new Vector2();

                    // full automatic
                    if (rows_ == 0 && cols_ == 0)
                    {
                        cols = capacityList.Count;
                        while(cols > 0)
                        {
                            // padding == 2pixel
                            Vector2 currentSize = new Vector2((Size.X - ((cols + 1) * 2)) / cols, (Size.Y - ((rows + 1) * 2)) / rows);
                            float currentScale = Math.Min(batterySize.X / size.X, batterySize.Y / size.Y);

                            if (currentScale < scale)
                                break;

                            scale = currentScale;
                            size = currentSize;
                            rows = (int)(capacityList.Count / --cols);
                        }
                    }
                    else
                    {
                        // calculate rows
                        if (rows == 0)
                            cols = (int)(capacityList.Count / rows);
                        // calculate cols
                        else
                            cols = (int)(capacityList.Count / cols);

                        size = new Vector2((Size.X - ((cols + 1) * 2)) / cols, (Size.Y - ((rows + 1) * 2)) / rows);
                        scale = Math.Min(batterySize.X / size.X, batterySize.Y / size.Y);
                    }

                    float positionX = Position.X + rt.DisplayOffset.X + 2f - (Size.X * 0.5f) + (size.X * 0.5f);
                    float positionY = Position.Y + rt.DisplayOffset.Y + 2f - (Size.Y * 0.5f) + (size.Y * 0.5f);
                    float offsetX = size.X + 2f;
                    float offsetY = size.Y + 2f;

                    // draw batteries
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            int index = c * (r + 1);
                            if (index >= capacityList.Count)
                                break;

                            Vector2 position = new Vector2(positionX + (offsetX * cols), positionY + (offsetY * rows));
                            drawSingleBattery(size, position, scale, (float)capacityList[index].indicator, (float)inoutList[index].indicator, true, addSprite);
                        }
                    }
                }
            }

            Vector2 batterySize = new Vector2(60f, 120f);
            int capacitySegments = 6;

            void drawSingleBattery(Vector2 size, Vector2 position, float scale, float capacity, float load, bool onoff, AddSpriteDelegate addSprite)
            {
                float borderSize = 8f * scale;
                float capacityBorder = borderSize * 0.5f;

                Vector2 poleSize = new Vector2(batterySize.X * 0.5f, 10f) * scale;
                Vector2 backgroundSize = new Vector2(batterySize.X * scale, (batterySize.Y * scale) - poleSize.Y);
                Vector2 InnerSectionSize = backgroundSize - borderSize;

                Color borderColor = load <= 0f || onoff == false ? Color.Red : Color.Green;

                // draw plus pole
                MySprite sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y - backgroundSize.Y * 0.5f),
                    poleSize, Color.White);
                addSprite(sprite);

                // draw background
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple",
                    new Vector2(position.X, position.Y + poleSize.Y * 0.5f),
                    backgroundSize, Color.White);
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
                        (InnerSectionSize.Y - (capacityBorder * (capacitySegments + 1f))) / capacitySegments);
                    float capacityYOffset = capacitySize.Y + capacityBorder;
                    float capacityYPosition = position.Y + (poleSize.Y + InnerSectionSize.Y - capacitySize.Y) * 0.5f - capacityBorder;

                    for (int s = 0; s < 6; s++)
                    {
                        float lerp = (1f / capacitySegments) * s;
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
