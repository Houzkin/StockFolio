using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StockFolio.ViewModels;

namespace StockFolio.Views
{
	[ValueConversion(typeof(DividePattern),typeof(string))]
	public class DividePatternToStringConerter : IValueConverter
	{
		/// <summary>ViewModelからViewへ変換</summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DividePattern)
			{
				var dp = (DividePattern)value;
				switch (dp)
				{
				case DividePattern.Location:
					return "ロケーション";
				case DividePattern.Tag:
					return "タグ";
				case DividePattern.LocationAndTag:
					return "ロケーション＆タグ";
				}
			}
			return value.ToString();
		}
		/// <summary>ViewからViewModelへ変換</summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DividePattern)
				return value;
			throw new ArgumentException("不正な値が入力されました");
		}
	}
}
