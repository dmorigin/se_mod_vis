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
                : base($"Display:{groupId}")
            {
                GroupId = groupId;
                PanelConnector = null;
                RenderingRetry = Default.ExceptionRetry;
                ContentContainer = null;
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

            bool renderProcessStarted_ = false;
            public override void tick(TimeSpan delta)
            {
                if (ContentContainer != null && !renderProcessStarted_)
                {
                    renderProcessStarted_ = true;

                    // queue gather jobs
                    foreach (var graphic in ContentContainer.getGraphics())
                    {
                        if (graphic.DataCollector != null)
                            graphic.DataCollector.queueJob();
                    }

                    // queue render job
                    JobManager.queueJob(new RenderJob(this));
                }
            }

            public override TimeSpan Interval
            {
                get
                {
                    if (ContentContainer != null)
                        return ContentContainer.Refresh;
                    return base.Interval;
                }

                set { base.Interval = value; }
            }

            public string GroupId
            {
                get;
                private set;
            }

            public ContentContainer ContentContainer
            {
                get;
                set;
            }

            public PanelConnectorObj PanelConnector
            {
                get;
                set;
            }

            public string Text => PanelConnector != null ? PanelConnector.Text : "";
            public string Title => PanelConnector != null ? PanelConnector.Title : "";

            public class PanelConnectorObj
            {
                public PanelConnectorObj(IMyTextPanel panel)
                {
                    Panel = panel;
                }

                public IMyTextPanel Panel
                {
                    get;
                    private set;
                }

                public string Text => Panel.GetText();
                public string Title => Panel.GetPublicTitle();
            }

            #region Rendering
            int RenderingRetry
            {
                get;
                set;
            }

            class RenderJob : Job
            {
                public RenderJob(Display display)
                    : base($"Render[{display.Name}]")
                {
                    display_ = display;
                }

                Display display_ = null;

                public override void finalizeJob()
                {
                    display_.renderProcessStarted_ = false;
                }

                public override void tick(TimeSpan delta)
                {
                    // start rendering
                    display_.render();
                    display_.RenderingRetry = Default.ExceptionRetry;
                }

                public override bool handleException()
                {
                    log(Console.LogType.Error, $"Rendering failed: {display_.Name}:{display_.Id} => {display_.RenderingRetry}");
                    return display_.RenderingRetry-- >= 0;
                }
            }

            RectangleF renderArea_ = new RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            public RectangleF RenderArea => renderArea_;

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

            public bool addRenderTarget(IMyTextSurface surface, RenderTargetID id, Vector2I coordinate)
            {
                if (getRenderTarget(coordinate) == null && surface != null)
                {
                    RenderTarget rt = new RenderTarget(id, coordinate);
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

            bool drawEmpty_ = false;
            public void render()
            {
                foreach (var graphic in ContentContainer.getGraphics())
                    graphic.prepareRendering(this);

                foreach (var rt in renderTargets_)
                {
                    rt.BackgroundColor = ContentContainer.BackgroundColor;

                    using (var frame = rt.getRenderFrame())
                    {
                        // Workaround to fix the MP Sprite issue
                        if (drawEmpty_)
                            frame.Add(new MySprite());
                        drawEmpty_ = !drawEmpty_;

                        rt.clearDrawArea(sprite => frame.Add(sprite));

                        foreach (var graphic in ContentContainer.getGraphics())
                            graphic.render(this, rt, sprite => frame.Add(sprite));
                    }
                }
            }
            #endregion // Rendering

            #region Management
            static List<Display> displays_ = new List<Display>();
            static int genericDisplayGroupId_ = 0;

            public static Display createDisplay(string groupId)
            {
                Display display;

                if (groupId == Default.EmptyDisplayGroupID)
                {
                    groupId = $"genericDisplayGroup_{++genericDisplayGroupId_}";
                    display = new Display(groupId);
                }
                else if ((display = getDisplayGroup(groupId)) != null)
                    return display;
                else
                    display = new Display(groupId);

                display.log(Console.LogType.Info, $"Create new display: group({groupId})");
                display.Manager.JobManager.queueJob(display.getConstructionJob());
                displays_.Add(display);
                return display;
            }

            static Display getDisplayGroup(string id) => displays_.Find((display) => display.GroupId == id);
            #endregion // Management
        }
    }
}
