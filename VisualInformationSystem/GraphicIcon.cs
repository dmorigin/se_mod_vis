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
        public class GraphicIcon : Graphic
        {
            public GraphicIcon(Template template, Configuration.Options options)
                : base(template, options)
            {
            }

            public override bool construct()
            {
                return base.construct();
            }

            public override Graphic clone()
            {
                GraphicIcon gfx = new GraphicIcon(Template, Options);

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

                gfx.iconName_ = iconName_;
                gfx.icon_ = icon_;
                gfx.blink_ = blink_;
                gfx.rotation_ = rotation_;

                Manager.JobManager.queueJob(gfx.getConstructionJob());
                return gfx;
            }

            protected override bool supportCheck(string name)
            {
                return true;
            }

            bool toggleShow_ = true; // render data only
            public override void prepareRendering(Display display)
            {
                toggleShow_ = !toggleShow_;
            }

            public override void getSprite(Display display, RenderTarget rt, AddSpriteDelegate addSprite)
            {
                bool visible = true;
                if (DataAccessor != null)
                    visible = VisibleOperator(DataAccessor.indicator(), VisibleThreshold);

                if (!visible || (!toggleShow_ && blink_))
                    return;

                Vector2 position = PositionType == ValueType.Relative ? Position * display.RenderArea.Size : Position;
                Vector2 size = SizeType == ValueType.Relative ? Size * display.RenderArea.Size : Size;

                icon_(addSprite, rt, iconName_, position, size, rotation_, Color);
            }

            #region Configuration
            public override ConfigHandler getConfigHandler()
            {
                var config = base.getConfigHandler();
                config.add("icon", configIcon);
                config.add("blink", configBlink);
                config.add("rotation", configRotation);

                return config;
            }

            string iconName_ = "";
            Icon.Render icon_ = null;
            bool configIcon(string key, string value, Configuration.Options options)
            {
                iconName_ = value;
                icon_ = Icon.getIcon(value);
                if (icon_ == null)
                {
                    log(Console.LogType.Error, $"Invalid icon name:{value}");
                    return false;
                }
                return true;
            }

            bool blink_ = false;
            bool configBlink(string key, string value, Configuration.Options options)
            {
                blink_ = Configuration.asBoolean(value, false);
                return true;
            }

            float rotation_ = 0f;
            bool configRotation(string key, string value, Configuration.Options options)
            {
                // 360° == 2*pi
                rotation_ = (float)((Configuration.asFloat(value, 0f) / 180f) * Math.PI);
                return true;
            }
            #endregion // Configuration
        }
    }
}
