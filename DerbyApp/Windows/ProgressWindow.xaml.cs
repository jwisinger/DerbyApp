using System.Windows;

namespace DerbyApp.Windows
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public double TableProgressValue
        {
            set => TableProgressBarControl.Value = value;
        }

        public double ProgressValue
        {
            set => ProgressBarControl.Value = value;
        }
    }
}