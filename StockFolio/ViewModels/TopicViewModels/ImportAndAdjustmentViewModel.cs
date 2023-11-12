using ObservableCollections;
using PortFolion.Core;
using Prism.Ioc;
using Reactive.Bindings;
using StockFolio.DiContainer;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockFolio.ViewModels {
	public class ImportAndAdjustmentViewModel : TopicViewModel {
		public static ObservableStack<IUndoOrder> UndoOrders { get; } = new ObservableStack<IUndoOrder>();
        UpdateSwitcher compositeAttacheDetach { get; set; }
		
		//AdjustmentViewModel? _adjVM;
		//public AdjustmentViewModel AdjustmentVM => _adjVM ??= new AdjustmentViewModel();

		//ImportViewModel? _impVM;
		//public ImportViewModel ImportVM => _impVM ??= new ImportViewModel(this);
		//public ObservableDictionary<CommonNode,TreeViewContainer> NodeCash { get; } = new();
		public ObservableStack<IUndoOrder> UndoOrderList => UndoOrders;
        public ImportAndAdjustmentViewModel() {
			this.compositeAttacheDetach = UpdateSwitcher.Switcher;
			//IsSelected.Where(x => x).Subscribe(_ => { MessageBox.Show("refreshed"); });
		}
	}
}
