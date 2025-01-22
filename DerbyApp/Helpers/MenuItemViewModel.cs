using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DerbyApp.Helpers
{
    public class MenuItemViewModel : INotifyPropertyChanged
    {
        private readonly ICommand _command;
        private bool _isChecked;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<string> SelectionChanged;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        public string Header { get; set; }
        public TrulyObservableCollection<MenuItemViewModel> ParentList;

        public TrulyObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        public bool IsChecked
        {
            set
            {
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
            get
            {
                return _isChecked;
            }
        }

        private void Execute()
        {
            foreach (var item in ParentList)
            {
                if (item.Header != Header)
                {
                    if (item.IsChecked) item.IsChecked = false;
                }
            }
            SelectionChanged?.Invoke(this, Header);
        }
    }
}
