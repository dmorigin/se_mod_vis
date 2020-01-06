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
        public class Console : VISObject
        {
            public Console()
            {
            }


            public override bool construct()
            {
                renderTarget_ = new RenderTarget(new Vector2I(0, 0));
                renderTarget_.setupSurface(App.Me.GetSurface(0));
                renderTarget_.BackgroundColor = Color.Black;

                float lineCount = (renderTarget_.Size.Y / (Default.CharHeight * fontSize_));
                lineHeight_ = renderTarget_.Size.Y / (int)lineCount;
                lineCorrection_ = (lineCount - (int)lineCount) * (int)(lineCount + 1);

                log(LogType.Info, "Console System constructed");
                Constructed = true;
                return true;
            }


            int maxStoreLines_ = 30;
            public int MaxStoreLines
            {
                get { return maxStoreLines_; }
                set
                {
                    if (value < maxStoreLines_)
                    {
                        while (value == messages_.Count)
                            messages_.Dequeue();
                    }
                    maxStoreLines_ = value;
                }
            }


            string font_ = "DEBUG";
            float fontSize_ = 0.5f;
            float lineHeight_ = 0f;
            float lineCorrection_ = 0f;
            Queue<KeyValuePair<LogType, string>> messages_ = new Queue<KeyValuePair<LogType, string>>();
            RenderTarget renderTarget_ = null;
            bool newMesssages_ = false;


            public enum LogType
            {
                Info,
                Warning,
                Error,
                Debug
            }

            // workaround for pack code
            string logTypeToString(LogType type)
            {
                switch (type)
                {
                    case LogType.Info:
                        return "Info";
                    case LogType.Warning:
                        return "Warning";
                    case LogType.Error:
                        return "Error";
                    case LogType.Debug:
                        return "Debug";
                }

                return "Invalid";
            }

            Color getFontColor(LogType logType)
            {
                switch (logType)
                {
                    case LogType.Warning:
                        return Color.YellowGreen;
                    case LogType.Error:
                        return Color.Red;
                    case LogType.Debug:
                        return Color.DarkOliveGreen;
                }

                return Color.LightGreen;
            }

            public new void log(LogType logType, string message)
            {
                messages_.Enqueue(new KeyValuePair<LogType, string>(logType, $"[{logTypeToString(logType)}]: {message}"));
                if (messages_.Count > maxStoreLines_)
                    messages_.Dequeue();
                newMesssages_ = true;
            }


            public void flush()
            {
                if (renderTarget_ != null && newMesssages_ == true)
                {
                    float offset = (messages_.Count * lineHeight_ + lineCorrection_) - (renderTarget_.Size.Y + renderTarget_.Position.Y);
                    int lineCount = 0;
                    newMesssages_ = false;

                    using (MySpriteDrawFrame frame = renderTarget_.getRenderFrame())
                    {
                        // flush message lines
                        foreach (var message in messages_)
                        {
                            MySprite line = MySprite.CreateText(message.Value, font_, getFontColor(message.Key), fontSize_, TextAlignment.LEFT);
                            line.Position = new Vector2(renderTarget_.Position.X, lineCount++ * lineHeight_ - offset);
                            frame.Add(line);
                        }
                    }
                }
            }
        }
    }
}
