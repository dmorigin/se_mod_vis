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
        public class DataCollectorConnector : DataCollectorBase<IMyShipConnector>
        {
            public DataCollectorConnector(Configuration.Options options, string connector)
                : base("connector", "", options, connector)
            {
            }

            protected override void update()
            {
                connected_ = 0;
                unconnected_ = 0;
                connectable_ = 0;

                foreach(var connector in Blocks)
                {
                    blocksOn_ += isOn(connector) ? 1 : 0;
                    blocksFunctional_ += connector.IsFunctional ? 1 : 0;

                    connected_ += connector.Status == MyShipConnectorStatus.Connected ? 1 : 0;
                    unconnected_ += connector.Status == MyShipConnectorStatus.Unconnected ? 1 : 0;
                    connectable_ += connector.Status == MyShipConnectorStatus.Connectable ? 1 : 0;
                }

                UpdateFinished = true;
            }

            int connected_ = 0;
            int unconnected_ = 0;
            int connectable_ = 0;

            public override string getVariable(string data)
            {
                return base.getVariable(data)
                    .Replace("%connected%", connected_.ToString())
                    .Replace("%disconnected%", unconnected_.ToString())
                    .Replace("%connectable%", connectable_.ToString());
            }

            #region Data Accessor
            public override DataAccessor getDataAccessor(string name)
            {
                switch(name.ToLower())
                {
                    case "status":
                        return new Status(this);
                    case "connected":
                        return new Connected(this);
                    case "unconnected":
                        return new Unconnected(this);
                }

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
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.connected_ + (dc_.connectable_ * 0.5));

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var connector in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = connector.CustomName;
                        item.indicator = connector.Status == MyShipConnectorStatus.Connected ? 1 : (connector.Status == MyShipConnectorStatus.Connectable ? 0.5 : 0);
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);
                        item.value = new VISUnitType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Connected : DataAccessor
            {
                DataCollectorConnector dc_;
                public Connected(DataCollectorConnector dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => (dc_.connected_ / (float)dc_.Blocks.Count);
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.connected_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var connector in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = connector.CustomName;
                        item.indicator = connector.Status == MyShipConnectorStatus.Connected ? 1 : 0;
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);
                        item.value = new VISUnitType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }

            class Unconnected : DataAccessor
            {
                DataCollectorConnector dc_;
                public Unconnected(DataCollectorConnector dc)
                {
                    dc_ = dc;
                }

                public override double indicator() => (dc_.unconnected_ / (float)dc_.Blocks.Count);
                public override VISUnitType min() => new VISUnitType(0);
                public override VISUnitType max() => new VISUnitType(dc_.Blocks.Count);
                public override VISUnitType value() => new VISUnitType(dc_.connected_);

                public override void list(out List<ListContainer> container, Func<ListContainer, bool> filter = null)
                {
                    container = new List<ListContainer>();
                    foreach (var connector in dc_.Blocks)
                    {
                        ListContainer item = new ListContainer();
                        item.name = connector.CustomName;
                        item.indicator = connector.Status == MyShipConnectorStatus.Unconnected ? 1 : 0;
                        item.min = new VISUnitType(0);
                        item.max = new VISUnitType(1);
                        item.value = new VISUnitType(item.indicator);

                        if (filter == null || (filter != null && filter(item)))
                            container.Add(item);
                    }
                }
            }
            #endregion // Data Accessor
        }
    }
}
