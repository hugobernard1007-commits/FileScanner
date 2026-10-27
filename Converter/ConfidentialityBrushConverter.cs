using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderAnalyzer.Converters;

public class ConfidentialityBrushConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        int score = (int)value;

        if (score > 60)
            return System.Windows.Media.Brushes.Red;

        if (score > 30)
            return System.Windows.Media.Brushes.Orange;

        if (score > 0)
            return System.Windows.Media.Brushes.Gold;

        return System.Windows.Media.Brushes.LightGreen;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
