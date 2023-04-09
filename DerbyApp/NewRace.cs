using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DerbyApp
{
    public partial class NewRace : Form
    {
        private readonly Database _db;
        public Race Race = new();

        public NewRace(Database db)
        {
            _db = db;
            InitializeComponent();
#warning The number 13 is hardcoded into this form
            for (int i = 1; i < tableLayoutPanel1.RowCount; i++)
            {
                tableLayoutPanel1.Controls.Add(new Label() { Text = "Racer in Position " + i, Dock = DockStyle.Fill }, 0, i);
                tableLayoutPanel1.Controls.Add(new ComboBox() { Text = "", Dock = DockStyle.Fill }, 1, i);
            }
            tableLayoutPanel3.Controls.Add(new TextBox() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Text = "Choose the levels you want to include racers from and click \"Load Racers\", then choose the desired racer name for each position and click \"OK\"." });
            foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                tableLayoutPanel3.RowCount++;
                tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayoutPanel3.Controls.Add(new CheckBox() { Text = s, Dock = DockStyle.Fill, Checked = true }, 0, tableLayoutPanel3.RowCount - 1);
            }
            tableLayoutPanel3.RowCount++;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            Button b = new() { Text = "Load Racers", Name = "buttonLoadRacers" };
            b.Click += new EventHandler(ButtonLoadRacers_Click);
            tableLayoutPanel3.Controls.Add(b, 0, tableLayoutPanel3.RowCount - 1);
        }

        private void ButtonLoadRacers_Click(object? sender, EventArgs e)
        {
            foreach (Control c in tableLayoutPanel1.Controls)
            {
                if (c is ComboBox cb)
                {
                    cb.Items.Clear();
#warning Need to make filter checkboxes actually work
                    cb.Items.AddRange(_db.GetRacerData().Select(x=>x.RacerName + "; " + x.Number).ToArray());
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
            List<Racer> SelectedRacers = new();
            foreach (Control c in tableLayoutPanel1.Controls)
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
                Race = new(tbName.Text, SelectedRacers, false);
            }
        }
    }
}
