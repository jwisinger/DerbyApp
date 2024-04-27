using System.Windows.Forms;

namespace DerbyApp.Helpers
{
    public partial class UserInputBox: Form
    {
        public string Input = "";

        public UserInputBox(string title)
        {
            InitializeComponent();
            Text = title;
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            Input = textBox1.Text;
        }
    }
}
