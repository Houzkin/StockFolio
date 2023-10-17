using StockFolio.ViewModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StockFolio.Views {
	public class TargetOnlyEditTemplateSelector : DataTemplateSelector{
		public override DataTemplate SelectTemplate(object item, DependencyObject container) {
			var element = container as FrameworkElement;
			var vm = item as TreeViewNode;
			if (element == null || vm == null) return null;
			
			var nm = vm switch {
				EditablePosition ep => "Position",
				EditableCashPosition ecp => "Cash",
				EditableBasket eb => "Basket",
				_ => "Other",
			};
			return nm == "" ? null : element.FindResource(nm) as DataTemplate;
		}
	}
}
