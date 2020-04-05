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
        public class GraphicTest : Graphic
        {
            public GraphicTest(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            public override Graphic clone()
            {
                GraphicTest gfx = new GraphicTest(Template, Options);

                gfx.DataCollector = DataCollector;
                gfx.DataAccessor = gfx.DataCollector.getDataAccessor(DataAccessorName);
                gfx.DataAccessorName = DataAccessorName;
                gfx.Position = Position;
                gfx.PositionType = PositionType;
                gfx.Size = Size;
                gfx.SizeType = SizeType;
                gfx.VisibleThresholdA = VisibleThresholdA;
                gfx.VisibleOperatorA = VisibleOperatorA;
                gfx.VisibleThresholdB = VisibleThresholdB;
                gfx.VisibleOperatorB = VisibleOperatorB;
                gfx.VisibleCondition = VisibleCondition;

                foreach (var color in Gradient)
                    gfx.addGradientColor(color.Key, color.Value);

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            protected override bool supportCheck(string name) => false;

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                // render info text
                string text = "Size=" + rt.Size.X.ToString("0000.00") + ";" + rt.Size.Y.ToString("0000.00");
                var fontSize = display.measureLineInPixels(text, "debug", 1f);
                float fontScale = Math.Min((rt.Size.X * 0.8f) / fontSize.X, (rt.Size.Y * 0.8f) / fontSize.Y);

                var font = MySprite.CreateText(text, "debug", Color.White, fontScale);
                font.Position = new Vector2(rt.Size.X * 0.5f, fontSize.Y * fontScale + 10.0f) + rt.DisplayOffset;
                addSprite(font);

                // render edges
                Vector2 blockSize = new Vector2(20f, 20f);

                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, (rt.Size / 2f) + rt.DisplayOffset, blockSize, Color.White));
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, (blockSize / 2f) + rt.DisplayOffset, blockSize, Color.Red));
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, new Vector2(rt.Size.X - blockSize.X / 2f, blockSize.Y / 2f) + rt.DisplayOffset, blockSize, Color.Green));
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, new Vector2(blockSize.X / 2f, rt.Size.Y - blockSize.Y / 2f) + rt.DisplayOffset, blockSize, Color.Blue));
                addSprite(new MySprite(SpriteType.TEXTURE, IconNameSquareSimple, (rt.Size - (blockSize / 2f)) + rt.DisplayOffset, blockSize, Color.Yellow));
            }
        }
    }
}
