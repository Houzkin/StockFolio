using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockFolio.Views {
	/// <summary>
	/// Adjustment.xaml の相互作用ロジック
	/// </summary>
	public partial class Adjustment : UserControl {
		public Adjustment() {
			InitializeComponent();
		}

		private void TreeListView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {

        }
    }
}
