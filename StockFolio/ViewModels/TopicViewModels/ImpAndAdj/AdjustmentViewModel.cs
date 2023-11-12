using Houzkin.Architecture;
using ObservableCollections;
using PortFolion.Core;
using Prism.Ioc;
using Reactive.Bindings.Extensions;
using StockFolio.DiContainer;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockFolio.ViewModels {
    public class AdjustmentViewModel : ViewModelBase/*:TopicViewModel*/ {
		UpdateSwitcher compositeAttachDettach;
		public DateTreeRootViewModel DateTree { get; private set; }
        public TreeViewGridRoot EditableTreeRoot { get; private set; }
        CompositeDisposable observer = new CompositeDisposable();
        public AdjustmentViewModel(IContainerProvider provider) {
            compositeAttachDettach = UpdateSwitcher.Switcher;
            this.DateTree = new();
            this.EditableTreeRoot = new TreeViewGridRoot();

            var ad = new AttachDetachSet(provider.Resolve<ImportAndAdjustmentViewModel>(),
                AttachFunc,
                () => {
                    var pred = DateTree.CurrentDate;
                    DateTree.Refresh();
                    var pstd = DateTree.CurrentDate;
                    if(pred == pstd) EditableTreeRoot.Refresh();

                }, 
                DetachFunc);
            this.compositeAttachDettach.RefreshAndAdd(ad);//初回、TreeRootのRefresh が2回実行されてしまう。
        }
        void AttachFunc() {
            DateTree.ObserveProperty(x => x.CurrentDate,false).Subscribe(x => {
                //if (x.HasValue) this.EditableTreeRoot.DisplayTo(x.Value);
                this.EditableTreeRoot.DisplayTo(x.HasValue ? x.Value : DateTime.Today);
            }).AddTo(observer);
        }
        void DetachFunc() {
            observer.Clear();
        }

    }
}
