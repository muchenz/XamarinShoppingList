using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace XamarinShoppingList1.Converters
{
    public class StatusToTextDecoration : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var decorationText = TextDecorations.None;
            if (value == null) return decorationText;

            if (System.Convert.ToInt32(value) == 1) decorationText = TextDecorations.Strikethrough;

            return decorationText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //For this case you can ignore this
            return null;
        }
    }
}
