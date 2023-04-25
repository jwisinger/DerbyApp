using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DerbyApp.RacerDatabase
{
    public partial class DatabaseCreator : Form
    {
        public string DatabaseFile = "";
        public DatabaseCreator()
        {
            InitializeComponent();
        }

        public DatabaseCreator(string title, string textbox)
        {
            InitializeComponent();
            textBox1.Text = textbox;
            Text = title;
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            DatabaseFile = (sender as OpenFileDialog).FileName;
            DialogResult = DialogResult.OK;
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }
    }
}
