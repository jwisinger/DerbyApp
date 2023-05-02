using System;
using System.Windows.Data;
using System.Windows;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;

namespace DerbyApp
{
    public class NameToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
#warning HIGHLIGHT: Delete this, or enable it
            /*string input = (string)value;
            switch (input)
            {
                case "John Wisinger":
                    return new SolidColorBrush(Colors.LightBlue);
                default:
                    return DependencyProperty.UnsetValue;
            }*/
            /*int input;
            try
            {
                DataGridCell dgc = (DataGridCell)value;
                System.Data.DataRowView rowView = (System.Data.DataRowView)dgc.DataContext;
                input = (int)rowView.Row.ItemArray[dgc.Column.DisplayIndex];
            }
            catch (InvalidCastException e)
            {
                return DependencyProperty.UnsetValue;
            }
            switch (input)
            {
                case 1: return new SolidColorBrush(Colors.LightBlue);
                case 2: return new SolidColorBrush(Colors.LightGreen);
                case 3: return new SolidColorBrush(Colors.Brown);
                default: return DependencyProperty.UnsetValue;
            }*/
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
