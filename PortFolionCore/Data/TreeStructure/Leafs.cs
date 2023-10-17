using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Houzkin.Tree;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PortFolion.IO;
using Houzkin;

namespace PortFolion.Core {

    public abstract class  FinancialValue: CommonNode {
		protected FinancialValue() { }
		internal FinancialValue(CushionNode cushion) : base(cushion) {
			_amount = cushion.Amount;
		}
		protected override bool CanAddChild(CommonNode child) => false;

		long _amount;
		public void SetAmount(long amount) {
			if (_amount == amount) return;
			_amount = amount;
			RaisePropertyChanged(nameof(Amount));
		}
		public override long Amount {
			get { return _amount; }
		}
		public override bool HasPosition
			=> Amount != 0;
		public override bool HasTrading
			=> InvestmentValue != 0;
		protected override CommonNode Clone(CommonNode node) {
			(node as FinancialValue)._amount = Amount;
			return base.Clone(node);
		}
		
		internal override CushionNode ToSerialCushion() {
			var obj = base.ToSerialCushion();
			obj.Amount = _amount;
			return obj;
		}
	}
    public class CashValue : FinancialValue {
		public CashValue() { }
		internal CashValue(CushionNode cushion) : base(cushion) {
		}
		/// <summary>現在の投資額に変更を加える。</summary>
		/// <param name="addvalue">投資額。以降の残高は変更されない。</param>
		public void AddInvestment(long addvalue) {
			this.SetInvestmentValue(this.InvestmentValue + addvalue);
		}
		/// <summary>現在と以降の残高に加算する。</summary>
		/// <param name="addvalue">加算する値</param>
		public void AddAmount(long addvalue) {
			var nd = RootCollection.GetNodeLineFrom(this.Path, (this.Root() as TotalRiskFundNode).CurrentDate).Values.OfType<CashValue>();
			foreach (var fv in nd) {
				fv.SetAmount(fv.Amount + addvalue);
			}
		}
		public override CommonNode Clone() {
			return Clone(new CashValue());
		}
		public override NodeType GetNodeType() {
			return NodeType.Cash;
		}
	}
	/// <summary>金融商品</summary>
	public class FinancialProduct : FinancialValue {
		public FinancialProduct() { }
		internal FinancialProduct(CushionNode cushion) : base(cushion) {
			_quantity = cushion.Quantity;
			_tradeQuantity = cushion.TradeQuantity;
		}
		long _quantity;
		public void SetQuantity(long quantity) {
			if (_quantity == quantity) return;
			_quantity = quantity;
			RaisePropertyChanged(nameof(Quantity));
		}
		public long Quantity {
			get { return _quantity; }
		}
		public override bool HasPosition
			=> base.HasPosition || Quantity != 0;
		public override bool HasTrading
			=> base.HasTrading || TradeQuantity != 0;

		long _tradeQuantity;
		public void SetTradeQuantity(long tradeQuantity) {
			if (_tradeQuantity == tradeQuantity) return;
			_tradeQuantity = tradeQuantity;
			RaisePropertyChanged(nameof(TradeQuantity));
		}
		public long TradeQuantity => _tradeQuantity;

		/// <summary>現在以降に継続するポジションにおいて保有数を分割処理する。残高、投資額、投資数量は変更しない。</summary>
		/// <param name="spliting">分割係数</param>
		public void SplitQuantity(double spliting) {
			var nd = RootCollection.GetNodeLineFrom(this.Path, (this.Root() as TotalRiskFundNode).CurrentDate).Values.OfType<FinancialProduct>();
			foreach (var fv in nd) {
				fv.SetQuantity((long)(fv.Quantity * spliting));
			}
		}
		/// <summary>トレードを追加する。現在以降に継続するポジションの保有数と残高も変更される。</summary>
		/// <param name="tQ">取引数量。購入は＋、売却は－</param>
		/// <param name="Inv">取引金額。購入は＋、売却は－</param>
		public void AddTrade(long tQ,long Inv) {
			this.SetTradeQuantity(this.TradeQuantity + tQ);
			this.SetInvestmentValue(this.InvestmentValue + Inv);
			var nd = RootCollection.GetNodeLineFrom(this.Path, (this.Root() as TotalRiskFundNode).CurrentDate).Values.OfType<FinancialProduct>();
			var per = Inv / tQ;//取引時の単価
			foreach (var fv in nd) {
				per = fv.Quantity != 0 ? fv.Amount / fv.Quantity : per;//単価の更新
				fv.SetQuantity(fv.Quantity + tQ);
				fv.SetAmount(per * fv.Quantity);
			}
		}
		public void AddTrade(long tQ,long Inv,double pp) {
			this.SetTradeQuantity(this.TradeQuantity + tQ);
			this.SetInvestmentValue(this.InvestmentValue + Inv);
			this.SetQuantity(this.Quantity + tQ);
			this.SetAmount((long)(pp * this.Quantity));
			var pr = this.Quantity != 0 ? this.Amount / this.Quantity : pp;
			var nd = RootCollection.GetNodeLineFrom(this.Path, (this.Root() as TotalRiskFundNode).CurrentDate).Values.OfType<FinancialProduct>();
			foreach(var fv in nd.Skip(1)) {
				pr = fv.Quantity != 0 ? fv.Amount / fv.Quantity : pr;
				fv.SetQuantity(fv.Quantity + tQ);
				fv.SetAmount((long)(pr * fv.Quantity));
			}
		}
		protected override CommonNode Clone(CommonNode node) {
			var n = node as FinancialProduct;
			n._quantity = Quantity;
			return base.Clone(node);
		}
		public override CommonNode Clone() {
			return Clone(new FinancialProduct());
		}
		internal override CushionNode ToSerialCushion() {
			var obj = base.ToSerialCushion();
			obj.Quantity = _quantity;
			obj.TradeQuantity = _tradeQuantity;
			return obj;
		}
		public override NodeType GetNodeType() {
			return NodeType.OtherProduct;
		}
	}
	public class StockValue : FinancialProduct {
		public StockValue() { }
		internal StockValue(CushionNode cushion) : base(cushion) {
			_code = ResultWithValue.Of<int>(int.TryParse, cushion.Code)
				.EitherWay(r => r);
		}
		int _code;
		public int Code {
			get { return _code; }
			set {
				if (_code == value) return;
				_code = value;
				RaisePropertyChanged();
			}
		}
		protected override CommonNode Clone(CommonNode node) {
			(node as StockValue)._code = Code;
			return base.Clone(node);
		}
		public override CommonNode Clone() {
			return Clone(new StockValue());
		}
		internal override CushionNode ToSerialCushion() {
			var obj = base.ToSerialCushion();
			obj.Code = _code.ToString();
			return obj;
		}
		public override NodeType GetNodeType() {
			return NodeType.Stock;
		}
	}
	public class ForexValue : FinancialProduct {
		internal ForexValue() { }
		internal ForexValue(CushionNode cushion) : base(cushion) {
			_pair = cushion.Code;
		}
		string _pair;
		public string Pair {
			get { return _pair; }
			set {
				if (_pair == value) return;
				_pair = value;
				RaisePropertyChanged();
			}
		}
		protected override CommonNode Clone(CommonNode node) {
			(node as ForexValue)._pair = Pair;
			return base.Clone(node);
		}
		public override CommonNode Clone() {
			return Clone(new ForexValue());
		}
		internal override CushionNode ToSerialCushion() {
			var obj = base.ToSerialCushion();
			obj.Code = Pair;
			return obj;
		}
		public override NodeType GetNodeType() {
			return NodeType.Forex;
		}
	}
}
