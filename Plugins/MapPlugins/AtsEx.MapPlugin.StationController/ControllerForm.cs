﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Automatic9045.AtsEx.PluginHost;
using Automatic9045.AtsEx.PluginHost.ClassWrappers;

namespace Automatic9045.MapPlugins.StationController
{
    public partial class ControllerForm : Form
    {
        public ControllerForm()
        {
            InitializeComponent();

            if (AtsExPlugin.BveHacker.HasScenarioProviderCreated)
            {
                ResetInput();
            }
            else
            {
                AtsExPlugin.BveHacker.ScenarioProviderCreated += ScenarioProviderCreated;
            }

            void ScenarioProviderCreated(ScenarioProviderCreatedEventArgs e)
            {
                AtsExPlugin.BveHacker.ScenarioProviderCreated -= ScenarioProviderCreated;
                ResetInput(e.ScenarioProvider);
            }
        }

        private void ResetInput(ScenarioProvider scenarioProvider = null)
        {
            scenarioProvider = scenarioProvider ?? AtsExPlugin.BveHacker.CurrentScenarioProvider;

            StationList stations = scenarioProvider.Route.Stations;
            Station lastStation = stations.Count == 0 ? null : stations[stations.Count - 1] as Station;
            
            LocationValue.Text = (lastStation is null ? 0 : lastStation.Location + 500).ToString();
            int arrivalTime = lastStation is null ? 10 * 60 * 60 * 1000 : lastStation.DefaultTime + 2 * 60 * 1000;
            ArrivalTimeValue.Text = arrivalTime.ToTimeText();
            DepertureTimeValue.Text = (arrivalTime + 30 * 1000).ToTimeText();
        }

        private void AddButtonClicked(object sender, EventArgs e)
        {
            StationList stations = AtsExPlugin.BveHacker.CurrentScenarioProvider.Route.Stations;

            try
            {
                Station newStation = new Station(NameValue.Text)
                {
                    Location = int.Parse(LocationValue.Text),
                    DefaultTime = ArrivalTimeValue.Text.ToTimeMilliseconds(),
                    ArrivalTime = ArrivalTimeValue.Text.ToTimeMilliseconds(),
                    DepertureTime = Pass.Checked ? int.MaxValue : DepertureTimeValue.Text.ToTimeMilliseconds(),
                    Pass = Pass.Checked,
                    IsTerminal = IsTerminal.Checked,
                };
                stations.Add(newStation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生したため駅を追加できませんでした。\n\n詳細：\n\n" + ex.ToString(), "駅追加エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetInput();
                return;
            }

            UpdateStationList();
            ResetInput();
        }

        private void RemoveButtonClicked(object sender, EventArgs e)
        {
            StationList stations = AtsExPlugin.BveHacker.CurrentScenarioProvider.Route.Stations;
            if (stations.Count == 0) return;
            stations.RemoveAt(stations.Count - 1);

            if (stations.Count == 0)
            {
                RemoveButton.Enabled = false;
            }
            else
            {
                Station lastStation = stations.Last() as Station;
            }

            UpdateStationList();
        }

        private void UpdateStationList()
        {
            StationList stations = AtsExPlugin.BveHacker.CurrentScenarioProvider.Route.Stations;
            TimeTable timeTable = AtsExPlugin.BveHacker.CurrentScenarioProvider.TimeTable;
            timeTable.NameTexts = new string[stations.Count + 1];
            timeTable.NameTextWidths = new int[stations.Count + 1];
            timeTable.ArrivalTimeTexts = new string[stations.Count + 1];
            timeTable.ArrivalTimeTextWidths = new int[stations.Count + 1];
            timeTable.DepertureTimeTexts = new string[stations.Count + 1];
            timeTable.DepertureTimeTextWidths = new int[stations.Count + 1];
            timeTable.Update();

            AtsExPlugin.BveHacker.UpdateDiagram();
        }
    }
}