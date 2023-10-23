using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
	public class BasketEditer: ElementEditer {
		public BasketEditer(CommonNode node) : base(node) {
			this.Name = node.ObserveProperty(x => x.Name)
				.ToReactiveProperty<string>()
				.SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "項目名を入力してください" : null)
				.SetValidateNotifyError(x => (x != node.Name && !node.CanChangeName(x)) ? "変更範囲において、同一の項目名が既に存在します" : null)
				.AddTo(Disposables);
			this.Name.ObserveHasErrors.Subscribe(hasErr => this.ErrorChanged(hasErr, nameof(this.Name)));
			this.OrderReserver.Add(new EditPresenter<string>(nameof(Name), () => this.Name.Value , () => node.Name, x => node.ChangeName(x)));
			this.Name.Subscribe(_ => this.CheckChangedProperty(nameof(Name)));

			this.ModelName = node.ObserveProperty(x => x.Name)
				.ToReadOnlyReactiveProperty<string>()
				.AddTo(Disposables);
		}
		public ReactiveProperty<string> Name { get;}
		public ReadOnlyReactiveProperty<string> ModelName { get; }
		public override void Reset() {
			this.Name.Value = Model.Name;
		}
		public bool IsEnabled => true;
		public bool IsReadOnly => false;
		public string Status => "変更内容";
	}
	public abstract class FinancialEditer : BasketEditer {
		protected FinancialEditer(CommonNode node):base(node) {
			#region 単一編集
			this.Amount = node.ObserveProperty(x => x.Amount).ToReactiveProperty().AddTo(Disposables);
			this.Amount.Subscribe(_ => this.CheckChangedProperty(nameof(Amount)));
			OrderReserver.Add(new EditPresenter<long>(nameof(Amount),
				() => this.Amount.Value,
				() => node.Amount,
				x => innerModel.SetAmount(x)));
			this.InvestmentValue = node.ObserveProperty(x => x.InvestmentValue).ToReactiveProperty().AddTo(Disposables);
			this.InvestmentValue.Subscribe(_=>CheckChangedProperty(nameof(InvestmentValue)));
			OrderReserver.Add(new EditPresenter<long>(nameof(InvestmentValue),
				() => this.InvestmentValue.Value,
				() => node.InvestmentValue,
				x => node.SetInvestmentValue(x)));
			#endregion
		}
		public override void Reset() {
			base.Reset();
			this.Amount.Value = Model.Amount;
			this.InvestmentValue.Value = Model.InvestmentValue;
		}
		FinancialValue innerModel { get => (FinancialValue)base.Model; }
		public ReactiveProperty<long> Amount { get; }
		public ReactiveProperty<long> InvestmentValue { get; }
		public abstract ReadOnlyReactiveProperty<long> PreviewAmount { get; }
		public abstract ReadOnlyReactiveProperty<long> PreviewInvestmentValue { get;}
	}
	public class CashPositionEditer : FinancialEditer {
		public CashPositionEditer(CommonNode model) : base(model) {
			#region 単一編集
			this.AddInvestmentValue = new ReactiveProperty<long>(0);
			this.AddInvestmentValue.Subscribe(_ => this.CheckChangedProperty(nameof(AddInvestmentValue)));
			this.OrderReserver.Add(new EditPresenter<long>(nameof(AddInvestmentValue),
				() => this.AddInvestmentValue.Value,
				() => 0,
				x => innerModel.AddInvestment(x),
				x => x * -1));
			#endregion

			#region 時系列編集
			this.AddAmount = new ReactiveProperty<long>(0);
			this.AddAmount.Subscribe(_ => this.CheckChangedProperty(nameof(AddAmount)));
			this.OrderReserver.Add(new EditPresenter<long>(nameof(AddAmount),
				() => this.AddAmount.Value,
				() => 0,
				x => innerModel.AddAmount(x),
				x => x * -1));
			#endregion
			#region 連動変化
			this.AddInvestmentValue.Subscribe(x => this.AddAmount.Value = x);
			#endregion
			#region プレビュー
			PreviewAmount = new[] {
				this.Amount,this.AddAmount,
			}.CombineLatest(x => x.Sum()).ToReadOnlyReactiveProperty();

			PreviewInvestmentValue = new[] {
				this.InvestmentValue,this.AddInvestmentValue,
			}.CombineLatest(x => x.Sum()).ToReadOnlyReactiveProperty();
			#endregion
		}
		public override void Reset() {
			base.Reset();
			this.AddInvestmentValue.Value = 0;
			this.AddAmount.Value = 0;
		}
		CashValue innerModel { get => (CashValue)base.Model; }
		public ReactiveProperty<long> AddAmount { get; }
		public ReactiveProperty<long> AddInvestmentValue { get; }
		public override ReadOnlyReactiveProperty<long> PreviewAmount { get; }
		public override ReadOnlyReactiveProperty<long> PreviewInvestmentValue { get; }
	}
	public class ProductEditor : FinancialEditer {
		public ProductEditor(CommonNode model) : base(model) {
			#region 単一編集
			this.TradeQuantity = innerModel.ObserveProperty(x => x.TradeQuantity).ToReactiveProperty().AddTo(Disposables);
			this.TradeQuantity.Subscribe(_ => this.CheckChangedProperty(nameof(TradeQuantity)));
			OrderReserver.Add(new EditPresenter<long>(nameof(TradeQuantity),
				() => this.TradeQuantity.Value,
				() => innerModel.TradeQuantity,
				x => innerModel.SetTradeQuantity(x)));

			this.Quantity = innerModel.ObserveProperty(x => x.Quantity).ToReactiveProperty().AddTo(Disposables);
			this.Quantity.Subscribe(_ => this.CheckChangedProperty(nameof(Quantity)));
			OrderReserver.Add(new EditPresenter<long>(nameof(Quantity),
				() => this.Quantity.Value,
				() => innerModel.Quantity,
				x => innerModel.SetQuantity(x)));
			#endregion

			#region 時系列編集
			this.QuantitySplitRatio = new ReactiveProperty<double>(1)
				.SetValidateNotifyError(x => x == 0 ? "0分割することはできません" : null);
			this.QuantitySplitRatio.ObserveHasErrors.Subscribe(hasErr => this.ErrorChanged(hasErr, nameof(this.QuantitySplitRatio)));
			this.QuantitySplitRatio.Subscribe(_ => CheckChangedProperty(nameof(QuantitySplitRatio)));
			OrderReserver.Add(new EditPresenter<double>(-1, nameof(QuantitySplitRatio),
				() => this.QuantitySplitRatio.Value,
				() => 1,
				x => innerModel.SplitQuantity(x),
				x => 1 / x));

			this.AddTradeQuantity = new ReactiveProperty<long>(0);
			this.AddTradeQuantity.Subscribe(_ => CheckChangedProperty(nameof(AddTradeQuantity)));
			this.AddInvestmentValue = new ReactiveProperty<long>(0);
			this.AddInvestmentValue.Subscribe(_ => CheckChangedProperty(nameof(AddInvestmentValue)));
			OrderReserver.Add(
				new MultiParamEditPresenter<(long AddTq, long AddInv)>(1,
					new Dictionary<string, Func<(long AddTq, long AddInv), object>>() {
						[nameof(AddTradeQuantity)] = x => x.AddTq,
						[nameof(AddInvestmentValue)] = x => x.AddInv,
					},
					() => (this.AddTradeQuantity.Value, this.AddInvestmentValue.Value),
					() => (0, 0),
					x => innerModel.AddTrade(x.AddTq, x.AddInv),
					x => (x.AddTq * -1, x.AddInv * -1)
				));
			#endregion

			#region 連動変更
			this.PerPrice = new[] {
				innerModel.ObserveProperty(x => x.Quantity).Select(x => (double)x),
				innerModel.ObserveProperty(x => x.Amount).Select(x => (double)x),
				this.QuantitySplitRatio,
			}.CombineLatest(a => a[0] != 0 ? (a[1] / a[0]) / a[2] : 0).ToReactiveProperty().AddTo(Disposables);
			//PreviewQuantity.Subscribe(x => { });
			//this.AddTradeQuantity.Subscribe(x => this.AddQuantity.Value = x);
			#endregion

			#region プレビュー

			PreviewInvestmentValue = new[] {
				this.InvestmentValue,this.AddInvestmentValue,
			}.CombineLatest(x => x.Sum()).ToReadOnlyReactiveProperty();

			PreviewQuantity = new[] {
				this.QuantitySplitRatio,
				this.Quantity.Select(x=>(double)x),
				this.AddTradeQuantity.Select(x=>(double)x),
			}.CombineLatest(a => (long)(a[0] * a[1] + a[2])).ToReadOnlyReactiveProperty();

			PreviewAmount = new[] {
				PreviewQuantity.Select(x=>(double)x), this.PerPrice,
			}.CombineLatest(x =>(long)(x[0] * x[1])).ToReadOnlyReactiveProperty();

			PreviewTradeQuantity = new[] {
				this.AddTradeQuantity,this.TradeQuantity,
			}.CombineLatest(x=>x.Sum()).ToReadOnlyReactiveProperty();

			PreviewPerPrice = new[] {
				this.PreviewAmount,this.PreviewQuantity,
			}.CombineLatest(a => a[1] != 0 ? (double)a[0] / a[1] : innerModel.Quantity != 0 ? (double)innerModel.Amount / innerModel.Quantity : 0)
			.ToReadOnlyReactiveProperty();
			#endregion
		}
		public override void Reset() {
			base.Reset();
			this.TradeQuantity.Value = innerModel.TradeQuantity;
			this.Quantity.Value = innerModel.Quantity;
			this.QuantitySplitRatio.Value = 1;
			this.AddTradeQuantity.Value = 0;
			this.AddInvestmentValue.Value = 0;
		}
		FinancialProduct innerModel { get => (FinancialProduct)base.Model; }
		public ReactiveProperty<long> Quantity { get; }
		public ReactiveProperty<long> TradeQuantity { get; }
		public ReactiveProperty<double>PerPrice { get; }

		public ReactiveProperty<double> QuantitySplitRatio { get; }
		public ReactiveProperty<long> AddTradeQuantity { get; }
		public ReactiveProperty<long> AddInvestmentValue { get; }
		public override ReadOnlyReactiveProperty<long> PreviewAmount { get; }
		public override ReadOnlyReactiveProperty<long> PreviewInvestmentValue { get; }
		public ReadOnlyReactiveProperty<long> PreviewQuantity { get; }
		public ReadOnlyReactiveProperty<long> PreviewTradeQuantity { get; }
		public ReadOnlyReactiveProperty<double> PreviewPerPrice { get; }

	}
}
