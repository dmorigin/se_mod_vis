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
        public class Display : JobTimed
        {
            public Display(string groupId)
            {
                GroupId = groupId;
            }

            public override bool construct()
            {
                if (Manager.JobManager.registerTimedJob(this))
                {
                    log(Console.LogType.Info, $"Display {GroupId} constructed");
                    Constructed = true;
                    return true;
                }

                log(Console.LogType.Error, "Failed to register display as job");
                return false;
            }

            public Vector2 measureLineInPixels(string line, string font, float fontSize)
            {
                if (line.Length == 0)
                    return new Vector2(0f, 0f);

                int width = 0;
                if (font.ToLower() != "monospace")
                {
                    foreach (char c in line)
                    {
                        int value;
                        if (!Default.CharWidths.TryGetValue(c, out value))
                            value = Default.CharWidthMonospace;
                        width += value;
                    }
                }
                else
                    width = line.Length * Default.CharWidthMonospace;

                width += Default.CharSpaceWidth * (line.Length - 1);
                return new Vector2(width * fontSize, Default.CharHeight * fontSize);
            }

            public override void tick(TimeSpan delta)
            {
                if (Template != null)
                {
                    // queue gather jobs
                    foreach (var graphic in Template.getGraphics())
                    {
                        if (graphic.DataCollector != null)
                            JobManager.queueJob(graphic.DataCollector.getUpdateJob());
                    }

                    // queue render job
                    JobManager.queueJob(new RenderJob(this));
                }
            }

            public override TimeSpan Interval
            {
                get
                {
                    if (Template != null)
                        return Template.Refresh;
                    return base.Interval;
                }

                set { base.Interval = value; }
            }

            public string GroupId
            {
                get;
                private set;
            }

            public Template Template
            {
                get;
                set;
            }


            class RenderJob : Job
            {
                public RenderJob(Display display)
                {
                    display_ = display;
                }


                Display display_ = null;


                public override void tick(TimeSpan delta)
                {
                    // start rendering
                    display_.render();
                }
            }

            #region Rendering
            RectangleF renderArea_ = new RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            public RectangleF RenderArea
            {
                get { return renderArea_; }
            }

            public Color BackgroundColor
            {
                get
                {
                    if (reference_ != null)
                        return reference_.BackgroundColor;
                    return Color.Black;
                }
            }

            List<RenderTarget> renderTargets_ = new List<RenderTarget>();
            RenderTarget reference_ = null;

            public bool addRenderTarget(IMyTextSurface surface, Vector2I coordinate)
            {
                if (getRenderTarget(coordinate) == null && surface != null)
                {
                    RenderTarget rt = new RenderTarget(coordinate);
                    rt.construct();
                    rt.setupSurface(surface);

                    // calculate surface offset
                    Vector2 offset = new Vector2(coordinate.X * rt.Size.X, coordinate.Y * rt.Size.Y);

                    // expand vd area
                    renderArea_.X = Math.Min(renderArea_.X, offset.X);
                    renderArea_.Y = Math.Min(renderArea_.Y, offset.Y);
                    renderArea_.Width = Math.Max(renderArea_.Width, (coordinate.X + 1) * rt.Size.X);
                    renderArea_.Height = Math.Max(renderArea_.Height, (coordinate.Y + 1) * rt.Size.Y);

                    // add pd to list
                    renderTargets_.Add(rt);
                    reference_ = getReferenceRT();
                    return true;
                }

                return false;
            }

            public RenderTarget getRenderTarget(Vector2I coordinate)
            {
                foreach (var rt in renderTargets_)
                {
                    if (rt.Coordinate == coordinate)
                        return rt;
                }

                return null;
            }

            public RenderTarget getReferenceRT()
            {
                if (renderTargets_.Count == 1)
                    return renderTargets_[0];

                return getRenderTarget(Default.DisplayCoordinate);
            }

            public void render()
            {
                foreach (var graphic in Template.getGraphics())
                    graphic.prepareRendering(this);

                foreach (var rt in renderTargets_)
                {
                    rt.BackgroundColor = Template.BackgroundColor;

                    using (var frame = rt.getRenderFrame())
                    {
                        foreach (var graphic in Template.getGraphics())
                            graphic.getSprite(this, rt, sprite => frame.Add(sprite));
                    }
                }
            }
            #endregion // Rendering
        }
    }
}
