using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

#warning VULNERABILITY: Block user from picking the same name twice
#warning FEATURE: Load and save race to database instead of file?
#warning BUG: Need to add a 13th racer field
#warning FEATURE: Add a way to start a race directly from the main screen

namespace DerbyApp
{
    public partial class NewRace : Form
    {
        private readonly Database _db;
        public Race Race = new Race();
        private readonly HeatList _raceHeatList = RaceResults.ThirteenCarsFourLanes;

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
        }

        private void ButtonLoadRacers_Click(object sender, EventArgs e)
        {
            foreach (Control c in tlpRacer.Controls)
            {
                if (c is ComboBox cb)
                {
                    cb.Items.Clear();
                    foreach (Racer r in _db.GetRacerData())
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

        private void TextBox1_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                e.Cancel = true;
                tbName.Focus();
                errorProvider1.SetError(tbName, "Name should not be left blank!");
            }
            else
            {
                e.Cancel = false;
                errorProvider1.SetError(tbName, "");
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
                                SelectedRacers.AddRange(_db.GetRacerData().Where(x => i == x.Number).ToArray());
                            }
                        }
                    }
                }
            }
            if(SelectedRacers.Count > 0)
            {
                Race = new Race(tbName.Text, SelectedRacers, _raceHeatList);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false))
                {
                    sw.WriteLine(tbName.Text);
                    foreach (Control c in tlpLevel.Controls)
                    {
                        if (c is CheckBox box)
                        {
                            sw.WriteLine(box.Checked);
                        }
                    }
                    foreach (Control c in tlpRacer.Controls)
                    {
                        if (c is ComboBox box)
                        {
                            sw.WriteLine(box.Text);
                        }
                    }
                }
            }
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    tbName.Text = sr.ReadLine();
                    foreach (Control c in tlpLevel.Controls)
                    {
                        if (c is CheckBox box)
                        {
                            if (bool.TryParse(sr.ReadLine(), out bool b)) box.Checked = b;
                        }
                    }
                    ButtonLoadRacers_Click(null, null);
                    foreach (Control c in tlpRacer.Controls)
                    {
                        if (c is ComboBox box)
                        {
                            box.Text = sr.ReadLine();
                        }
                    }
                }
            }
        }
    }
}
