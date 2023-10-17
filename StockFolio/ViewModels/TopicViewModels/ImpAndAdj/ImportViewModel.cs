using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
	public class ImportViewModel {
		ImportAndAdjustmentViewModel _parent;
		public ImportViewModel(ImportAndAdjustmentViewModel viewModel) {
			_parent = viewModel;
		}

	}
}
