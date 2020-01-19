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
        public class DataCollectorConnector : DataCollector<IMyShipConnector>
        {
            public DataCollectorConnector(Configuration.Options options)
                : base("connector", "", options)
            {
            }

            protected override void update()
            {
                connected_ = 0;
                disconnected_ = 0;
                connectable_ = 0;

                foreach(var connector in Blocks)
                {
                    blocksOn_ += isOn(connector) ? 1 : 0;

                    connected_ += connector.Status == MyShipConnectorStatus.Connected ? 1 : 0;
                    disconnected_ += connector.Status == MyShipConnectorStatus.Unconnected ? 1 : 0;
                    connectable_ += connector.Status == MyShipConnectorStatus.Connectable ? 1 : 0;
                }

                UpdateFinished = true;
            }

            int connected_ = 0;
            int disconnected_ = 0;
            int connectable_ = 0;

            public override string getText(string data)
            {
                return base.getText(data)
                    .Replace("%connected%", connected_.ToString())
                    .Replace("%disconnected%", disconnected_.ToString())
                    .Replace("%connectable%", connectable_.ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                if (name.ToLower() == "status")
                    return new Status(this);
                return base.getDataAccessor(name);
            }

            class Status : DataAccessor
            {
                DataCollectorConnector dc_;
                public Status(DataCollectorConnector dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => (dc_.connected_ / (float)dc_.Blocks.Count) +
                    (dc_.connectable_ / (float)dc_.Blocks.Count) * 0.5f;
                public override ValueType min() => new ValueType(0);
                public override ValueType max() => new ValueType(dc_.Blocks.Count);
                public override ValueType value() => new ValueType(dc_.connected_ + (dc_.connectable_ * 0.5));

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var connector in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = connector.CustomName;
                        item.indicator = connector.Status == MyShipConnectorStatus.Connected ? 1 : (connector.Status == MyShipConnectorStatus.Connectable ? 0.5 : 0);
                        item.min = new ValueType(0);
                        item.max = new ValueType(1);
                        item.value = new ValueType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
