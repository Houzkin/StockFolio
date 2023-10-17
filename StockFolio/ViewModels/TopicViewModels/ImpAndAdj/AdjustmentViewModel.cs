using Reactive.Bindings.Extensions;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
    public class AdjustmentViewModel {
		public AttachDetachSwitcher GetAttacheDetach() => MainViewModel.GetMainViewModel().compositeAttachDettach;
		public DateTreeRootViewModel DateTree { get; private set; }
        public TreeViewGridRoot EditableTreeRoot { get; }
        public AdjustmentViewModel(ImportAndAdjustmentViewModel iaavm) {
            this.DateTree = new();
            var ad = new AttachDetachSet(iaavm, () => { }, () => DateTree.Refresh(), () => { });
            this.GetAttacheDetach().Add(ad);

            this.EditableTreeRoot = new TreeViewGridRoot(iaavm);
            //if(DateTree.CurrentDate.HasValue)
            //    this.EditableTreeRoot.DisplayTo(DateTree.CurrentDate.Value);
            DateTree.ObserveProperty(x => x.CurrentDate).Subscribe(x => {
                if (x.HasValue) this.EditableTreeRoot.DisplayTo(x.Value);
            });
        }

    }
}
