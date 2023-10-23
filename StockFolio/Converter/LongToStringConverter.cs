using Houzkin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StockFolio.Views {
	[ValueConversion(typeof(long), typeof(string))]
	public class LongToStringConverter : IValueConverter {
		/// <summary>VMからVへ</summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value.ToString();
		}
		/// <summary>VからVMへ</summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			var s = value.ToString();
			if (string.IsNullOrEmpty(s)) return 0;
			return System.Convert.ToInt64(s);
		}
	}
}
