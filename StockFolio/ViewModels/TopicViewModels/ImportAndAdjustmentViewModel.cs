using ObservableCollections;
using PortFolion.Core;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
	public class ImportAndAdjustmentViewModel {
		public static ObservableStack<IUndoOrder> UndoOrders { get; } = new ObservableStack<IUndoOrder>();
		public AttachDetachSwitcher GetAttacheDetach() => MainViewModel.GetMainViewModel().compositeAttachDettach;
		public AdjustmentViewModel AdjustmentVM { get; } 
		public ImportViewModel ImportVM { get; }
		public ObservableDictionary<CommonNode,TreeViewContainer> NodeCash { get; } = new();
		public ObservableStack<IUndoOrder> UndoOrderList => UndoOrders;
		public ImportAndAdjustmentViewModel() { 
			this.AdjustmentVM = new AdjustmentViewModel(this);
			this.ImportVM = new ImportViewModel(this);
			
		}
	}
}
