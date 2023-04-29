using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

#warning TODO: Add back level filters
#warning TODO: Show race position
#warning TODO: Allow adding up to full "13"
#warning TODO: Allow creation of new race
#warning DATABASE: store changes to database

namespace DerbyApp
{
    /// <summary>
    /// Interaction logic for EditRace.xaml
    /// </summary>
    public partial class EditRace : Page
    {
        private readonly Database _db;
        private bool _handleSelection = true;
        public List<string> Races;
        public ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
        public ObservableCollection<Racer> AllRacers = new ObservableCollection<Racer>();


        public EditRace(Database db)
        {
            _db = db;
            InitializeComponent();
            Races = _db.GetListOfRaces();
            cbName.DataContext = Races;
            AllRacers = _db.GetAllRacers();
            cbColumn.ItemsSource = AllRacers;
            dataGridRacers.DataContext = Racers;
            /*for (int i = 0; i < _raceHeatList.RacerCount; i++)
            {
                tlpRacer.RowCount++;
                tlpRacer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpRacer.Controls.Add(new Label() { Text = "Racer in Position " + (i + 1), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, i + 1);
                tlpRacer.Controls.Add(new ComboBox() { Text = "", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }, 1, i + 1);
            }*/

            /*tlpLevel.Controls.Add(new TextBox() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Text = "Choose the levels you want to include racers from and click \"Load Racers\", then choose the desired racer name for each position and click \"OK\"." });
            foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                tlpLevel.RowCount++;
                tlpLevel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpLevel.Controls.Add(new CheckBox() { Text = s, Dock = DockStyle.Fill, Checked = true, Name = "cb" + s }, 0, tlpLevel.RowCount - 1);
            }
            tlpLevel.RowCount++;
            tlpLevel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));*/
        }

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Racers = _db.GetRacers((string)e.AddedItems[0], Racers);
        }

        private void DataGridComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int oldRacer = Racers.IndexOf(Racers.Where(x => x.Number == (e.RemovedItems[0] as Racer).Number).FirstOrDefault());
                int newRacer = Racers.IndexOf(Racers.Where(x => x.Number == (e.AddedItems[0] as Racer).Number).FirstOrDefault());
                if (oldRacer >= 0 && newRacer == -1) Racers[oldRacer] = (e.AddedItems[0] as Racer);
                else
                {
                    if (_handleSelection)
                    {
                        _handleSelection = false;
                        (sender as ComboBox).SelectedItem = e.RemovedItems[0];
                        _handleSelection = true;
                    }
                }
            }
            catch { }

            //Racers[sender. e.Row.GetIndex()] = AllRacers.Where(racer => racer.RacerName == racerName).FirstOrDefault();
            //_db.AddRacerToRacerTable(Racers[e.Row.GetIndex()]);
        }

        /*
            public RaceResults Race = new RaceResults();
            private readonly RaceHeat _raceHeatList = RaceHeats.ThirteenCarsFourLanes;

            private void ButtonLoadRacers_Click(object sender, EventArgs e)
            {
                foreach (Control c in tlpRacer.Controls)
                {
                    if (c is ComboBox cb)
                    {
                        if (cb.Name != "cbName")
                        {
                            cb.Items.Clear();
                            foreach (Racer r in _db.GetAllRacers())
                            {
                                Control cntrl = tlpLevel.Controls.Find("cb" + r.Level, true).FirstOrDefault();
                                if (cntrl != null)
                                {
                                    if ((cntrl as CheckBox).Checked)
                                    {
                                        cb.Items.Add(r.RacerName + "; " + r.Number);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void ComboBox1_Validating(object sender, CancelEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(cbName.Text))
                {
                    e.Cancel = true;
                    cbName.Focus();
                    errorProvider1.SetError(cbName, "Name should not be left blank!");
                }
                else
                {
                    e.Cancel = false;
                    errorProvider1.SetError(cbName, "");
                }
            }

            private void NewRace_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (DialogResult == DialogResult.OK)
                {
                    if (!ValidateChildren())
                    {
                        e.Cancel = true;
                    }
                }
            }

            private void ButtonOK_Click(object sender, EventArgs e)
            {
                List<Racer> SelectedRacers = new List<Racer>();
                foreach (Control c in tlpRacer.Controls)
                {
                    if (c is ComboBox cb)
                    {
                        if (cb.SelectedItem is string s)
                        {
                            string[] txt = s.Split(';');
                            if (txt.Length == 2)
                            {
                                if (Int64.TryParse(txt[1], out Int64 i))
                                {
                                    SelectedRacers.AddRange(_db.GetAllRacers().Where(x => i == x.Number).ToArray());
                                }
                            }
                        }
                    }
                }
                if (SelectedRacers.Count > 0)
                {
                    Race = new RaceResults(cbName.Text, SelectedRacers, _raceHeatList.HeatCount);
                }
            }

            private void CbName_KeyPress(object sender, KeyPressEventArgs e)
            {
                var regex = new Regex(@"[^a-zA-Z0-9\s]");
                if (regex.IsMatch(e.KeyChar.ToString()))
                {
                    e.Handled = true;
                }
            }
        }*/
    }
}
