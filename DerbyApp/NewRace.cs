using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;

#warning VULNERABILITY: Block user from picking the same name twice
#warning DATABASE: Allow modification of database on "create new race"

namespace DerbyApp
{
    public partial class NewRace : Form
    {
        private readonly Database _db;
        public RaceResults Race = new RaceResults();
        private readonly RaceHeat _raceHeatList = RaceHeats.ThirteenCarsFourLanes;

        public NewRace(Database db)
        {
            _db = db;
            InitializeComponent();
            for (int i = 0; i < _raceHeatList.RacerCount; i++)
            {
                tlpRacer.RowCount++;
                tlpRacer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpRacer.Controls.Add(new Label() { Text = "Racer in Position " + (i + 1), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, i + 1);
                tlpRacer.Controls.Add(new ComboBox() { Text = "", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }, 1, i + 1);
            }
            tlpLevel.Controls.Add(new TextBox() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Text = "Choose the levels you want to include racers from and click \"Load Racers\", then choose the desired racer name for each position and click \"OK\"." });
            foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                tlpLevel.RowCount++;
                tlpLevel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpLevel.Controls.Add(new CheckBox() { Text = s, Dock = DockStyle.Fill, Checked = true, Name="cb" + s }, 0, tlpLevel.RowCount - 1);
            }
            tlpLevel.RowCount++;
            tlpLevel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            Button b = new Button() { Text = "Load Racers", Name = "buttonLoadRacers" };
            b.Click += new EventHandler(ButtonLoadRacers_Click);
            tlpLevel.Controls.Add(b, 0, tlpLevel.RowCount - 1);

            cbName.Items.AddRange(db.GetListOfRaces().ToArray());
        }

        private void ButtonLoadRacers_Click(object sender, EventArgs e)
        {
            foreach (Control c in tlpRacer.Controls)
            {
                (c as ComboBox).Items.Clear();
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
            if(SelectedRacers.Count > 0)
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

        private void CbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = 0;
            List<string> racers = _db.GetRacerNames(cbName.Text);

            foreach (Control c in tlpLevel.Controls) if (c is CheckBox box) box.Checked = true;
            ButtonLoadRacers_Click(null, null);

            foreach (Control c in tlpRacer.Controls)
            {
                if (i >= racers.Count) break;
                if (c is ComboBox box)
                {
                    if (box.Name != "cbName") box.SelectedItem = racers[i++];
                }
            }
        }
    }
}
