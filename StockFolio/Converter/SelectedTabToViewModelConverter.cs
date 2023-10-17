using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StockFolio.Views {
	public class SelectedTabToVMConverter : ReactiveConverter<SelectionChangedEventArgs, AttachOrderArgs> {
		protected override IObservable<AttachOrderArgs?> OnConvert(IObservable<SelectionChangedEventArgs?> source) {
			return source.Select(e => {
				var aoa = new AttachOrderArgs();
				aoa.OriginSourceDataContext = (e.OriginalSource as TabControl)?.DataContext;
				if(e.AddedItems.Count!=0)
					aoa.ActiveDataContext = ((e.AddedItems[0] as ContentControl)?.Content as FrameworkElement)?.DataContext;
				if(e.RemovedItems.Count!=0)
					aoa.DeactiveDataContext = ((e.RemovedItems[0] as ContentControl)?.Content as FrameworkElement)?.DataContext;
				return aoa;
			}).Where(a=>a.OriginSourceDataContext != null);
		}
	}
	public class AttachOrderArgs {
		public object? OriginSourceDataContext { get; set; }
		public object? ActiveDataContext { get; set; }
		public object? DeactiveDataContext { get; set; }
	}
}
