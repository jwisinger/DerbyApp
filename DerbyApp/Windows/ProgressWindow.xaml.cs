using System.Windows;

namespace DerbyApp.Windows
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public double ProgressValue
        {
            set => ProgressBarControl.Value = value;
        }
    }
}