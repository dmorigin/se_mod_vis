﻿using Sandbox.Game.EntityComponents;
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
                groupId_ = groupId;
            }


            public override bool construct()
            {
                if (Manager.JobManager.registerTimedJob(this))
                {
                    log(Console.LogType.Info, $"Display constructed");
                    return true;
                }

                log(Console.LogType.Error, "Failed to register display as job");
                return false;
            }


            public override void tick(TimeSpan delta)
            {
                if (template_ != null)
                {
                    // queue gather jobs
                    foreach (var graphic in template_.getGraphics())
                    {
                        if (graphic.DataCollector != null)
                            JobManager.queueJob(graphic.DataCollector as Job);
                    }

                    // queue render job
                    JobManager.queueJob(new RenderJob(this));
                }
            }


            public override TimeSpan Interval
            {
                get
                {
                    if (template_ != null)
                        return template_.UpdateInterval;
                    return base.Interval;
                }

                set { base.Interval = value; }
            }


            string groupId_ = "";
            public string GroupID
            {
                get { return groupId_; }
            }


            Template template_ = null;
            public Template Template
            {
                get { return template_; }
                set { template_ = value; }
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

            public Vector2 FontSize
            {
                get
                {
                    if (reference_ != null)
                        return reference_.FontSize;
                    return new Vector2(0f, 0f);
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


            RenderTarget getReferenceRT()
            {
                if (renderTargets_.Count == 1)
                    return renderTargets_[0];

                return getRenderTarget(Program.Default.DisplayCoordinate);
            }


            public void render()
            {
                foreach (var rt in renderTargets_)
                {
                    using (var frame = rt.getRenderFrame())
                    {
                        foreach (var graphic in template_.getGraphics())
                            graphic.getSprite(this, rt, sprite => frame.Add(sprite));
                    }
                }
            }
            #endregion // Rendering
        }
    }
}
