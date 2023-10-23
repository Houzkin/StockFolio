using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Prism.Mvvm;
using Houzkin.Architecture;
using System.Reactive.Disposables;

namespace StockFolio.ViewModels {
	public class ReadOnlyGridElements :BindableObject {
		public ReadOnlyGridElements(CommonNode model) {
			this.Status = "変更前";
			this.Name = model.ObserveProperty(x => x.Name).ToReactiveProperty<string?>().AddTo(disposables);
			this.Amount = model.ObserveProperty(x => x.Amount).ToReadOnlyReactiveProperty().AddTo(disposables);
			this.InvestmentValue = model.ObserveProperty(x => x.InvestmentValue).ToReadOnlyReactiveProperty().AddTo(disposables);
			var pp = model as FinancialProduct;
			if (pp != null) {
				this.TradeQuantity = pp.ObserveProperty(x => x.TradeQuantity).ToReadOnlyReactiveProperty().AddTo(disposables);
				this.Quantity = pp.ObserveProperty(x=>x.Quantity).ToReadOnlyReactiveProperty().AddTo(disposables);
				this.PerPrice = new[] {
					this.Amount,this.Quantity,
				}.CombineLatest(x => x[1] != 0 ? (double)x[0] / x[1] : 0).ToReadOnlyReactiveProperty().AddTo(disposables);
			}
		}
		public ReadOnlyGridElements(ElementEditer ee) {
			this.Status = "変更後";
			var ce = ee as BasketEditer;
			if (ce == null) return;
			this.Name = ce.Name.ToReactiveProperty().AddTo(disposables);
			var be = ce as CashPositionEditer;
			if (be == null) return; 
			this.Amount =be.PreviewAmount.ToReadOnlyReactiveProperty().AddTo(disposables);
			this.InvestmentValue = be.PreviewInvestmentValue.ToReadOnlyReactiveProperty().AddTo(disposables);
			var pp = ce as ProductEditor;
			if (pp == null) return;
			this.PerPrice = pp.PreviewPerPrice.ToReadOnlyReactiveProperty().AddTo(disposables);
			this.TradeQuantity = pp.PreviewTradeQuantity.ToReadOnlyReactiveProperty().AddTo(disposables);
			this.Quantity = pp.PreviewQuantity.ToReadOnlyReactiveProperty().AddTo(disposables);
			
		}
		public string Status { get; }
		public bool IsEnabled => false;
		public bool IsReadOnly => true;
		public ReactiveProperty<string?>? Name { get; set; }
		public ReadOnlyReactiveProperty<double>? PerPrice { get; set; }
		public ReadOnlyReactiveProperty<long>? TradeQuantity { get; set; }
		public ReadOnlyReactiveProperty<long>? Quantity { get; set; }
		public ReadOnlyReactiveProperty<long>? InvestmentValue { get; set; }
		public ReadOnlyReactiveProperty<long>? Amount { get; set; }

		CompositeDisposable disposables = new CompositeDisposable();
		protected override void Dispose(bool disposing) {
			if (disposing) { this.disposables.Dispose(); }
			base.Dispose(disposing);
		}
	}
}
