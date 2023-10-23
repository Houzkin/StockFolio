using Houzkin;
using Houzkin.Tree;
using ObservableCollections;
using PortFolion.Core;
using Prism.Commands;
using Prism.Modularity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace StockFolio.ViewModels {
	
	public class TreeViewNode : ObservableTreeNode<TreeViewNode>, INotifyPropertyChanged {
		protected static TreeViewNode Create(CommonNode model) {
			var n = model switch {
				FinancialProduct ep => new EditablePosition(ep),
				CashValue ep=> new EditableCashPosition(ep),
				CommonNode ep=> new EditableBasket(ep),
			};
			return n;
		}
		protected TreeViewNode(CommonNode model) {
			this.Model = model;
			this.TestName = model.Name;
			this.IsSelected.Where(x => x).Subscribe(_ => OnSelected(this));
		}
		protected TreeViewNode() : this(new AnonymousNode()) { }

		public ReactiveProperty<bool> IsExpand { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>(false);
		public string TestName { get; set; }
		public CommonNode Model { get; }
		protected virtual void OnSelected(TreeViewNode node) {
			if (this.IsRoot()) return;
			this.Root().OnSelected(this);
		}
		protected CompositeDisposable Disposables = new CompositeDisposable();

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void RaisePropertyChanged(string propName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		protected override void Dispose(bool disposing) {
			Disposables.Dispose();
			base.Dispose(disposing); 
		}
	}
	public class EditableBasket : TreeViewNode {
		public EditableBasket(CommonNode model):base(model) {
			Disposable.Create(() => this.Editer?.Dispose()).AddTo(Disposables);
			this.Name = model.ObserveProperty(x => x.Name).ToReadOnlyReactiveProperty<string>().AddTo(Disposables);
			this.Amount = model.ObserveProperty(x=>x.Amount).ToReadOnlyReactiveProperty().AddTo(Disposables);
		}
		protected virtual ElementEditer GenerateEditer() {
			return new BasketEditer(Model);
		}
		protected virtual List<object> GenerateEditerGridItems() {
			return new List<object>() { 
				new ReadOnlyGridElements(Model).AddTo(Disposables), 
				Editer,
				new ReadOnlyGridElements(Editer).AddTo(Disposables)
			};
		}
		ElementEditer? _editer;
		public ElementEditer Editer { get => _editer ??= GenerateEditer(); }
		List<object>? edititems;
		public List<object> EditerGridItems { get => edititems ??= this.GenerateEditerGridItems(); }
		public ReadOnlyReactiveProperty<string> Name { get; }
		public ReadOnlyReactiveProperty<long> Amount { get; }
		
		
	}
	public class EditableCashPosition : EditableBasket {
		public EditableCashPosition(FinancialValue model):base(model) {
			this.InvestmentValue = model.ObserveProperty(x => x.InvestmentValue).ToReadOnlyReactiveProperty().AddTo(Disposables);
		}
		protected override ElementEditer GenerateEditer() {
			return new CashPositionEditer(Model);
		}
		public ReadOnlyReactiveProperty<long> InvestmentValue { get; }
		public virtual List<object>? CashEditerGridItems { get => this.EditerGridItems; }
	}
	public class EditablePosition : EditableCashPosition {
		public EditablePosition(FinancialProduct model):base(model) {
			this.TradeQuantity = model.ObserveProperty(x => x.TradeQuantity).ToReadOnlyReactiveProperty().AddTo(Disposables);
			this.Quantity = model.ObserveProperty(x => x.Quantity).ToReadOnlyReactiveProperty().AddTo(Disposables);
			this.PerPrice = new[] { this.Amount, this.Quantity }
				.CombineLatest(x =>x[1] != 0 ? (double)x[0] / x[1] : 0)
				.Select(x=>string.Format("{0:#,#.##}",x))
				.ToReadOnlyReactiveProperty<string>();
		}
		protected override ElementEditer GenerateEditer() {
			return new ProductEditor(Model);
		}
		public ReadOnlyReactiveProperty<string> PerPrice { get; }
		public ReadOnlyReactiveProperty<long> TradeQuantity { get; }
		public ReadOnlyReactiveProperty<long> Quantity { get; }
		public ElementEditer? CashPositionEditer { get {
				return (this.Siblings().Where(a => a.Model is CashValue).FirstOrDefault() as EditableCashPosition)?.Editer;
			}
		}
		List<object>? cashEditerGridItems;
		public override List<object>? CashEditerGridItems {
			get {
				if(cashEditerGridItems != null) return cashEditerGridItems;
				var editer = CashPositionEditer;
				if (editer == null) return null;
				cashEditerGridItems = new List<object>() {
					new ReadOnlyGridElements(editer.ModelNode).AddTo(Disposables),
					editer,
					new ReadOnlyGridElements(editer).AddTo(Disposables),
				};
				return cashEditerGridItems;
            }
		}
	}
    public class TreeViewGridRoot : TreeViewNode {
		private ObservableDictionary<CommonNode, TreeViewNode> _buffDic { get; }
		public TreeViewGridRoot(ImportAndAdjustmentViewModel viewmodel):base() {
			this._buffDic = viewmodel.NodeCash;
			CurrentDate = _currentDate.ToReadOnlyReactiveProperty();
			CurrentNode = _currentNode.ToReadOnlyReactiveProperty();
			//this.SelectedCommand = new DelegateCommand(() => this.OnSelected(this.Model));
		}
		//public DelegateCommand SelectedCommand { get; set; }
		public ReadOnlyReactiveProperty<DateTime?> CurrentDate { get; private set; }
		private ReactiveProperty<DateTime?> _currentDate { get; } = new ReactiveProperty<DateTime?>();
		public ReadOnlyReactiveProperty<TreeViewNode?> CurrentNode { get; private set; }
		private ReactiveProperty<TreeViewNode?> _currentNode { get; } = new ();
		public void DisplayTo(DateTime date) {
			this.DismantleDescendants();
			this.ClearChildren();
			var root = RootCollection.Instance.FirstOrDefault(a => a.CurrentDate == date);
			if (root == null) {
				this._currentDate.Value = null;
				this._currentNode.Value = null;
				return;
			}
			var shdw = (root as CommonNode).Convert(a => {
				return ResultWithValue.Of<CommonNode, TreeViewNode>(_buffDic.TryGetValue, a)
					.TrueOrNot(o => o,
					x => {
						var v = TreeViewNode.Create(a);
						_buffDic[a] = v;
						return v;
					});
			});//bufferにあれば、そこから取ってくるように変更予定
			this.AddChild(shdw);
			this._currentDate.Value = date;
			var prepath = CurrentNode.Value?.Model.Path;
			var curp = prepath != null ? this.Levelorder().FirstOrDefault(a => a.Model.Path.SequenceEqual(prepath)) : null;
			if (curp != null) {
				this._currentNode.Value = curp;
			} else {
				this._currentNode.Value = this.Levelorder().LastOrDefault();
			}
		}
		public void Refresh() {
			//要確認
			this._buffDic.Select(a => a.Value).ForEach(a => a.Dispose());
			this._buffDic.Clear();
			if (this.CurrentDate.Value != null) DisplayTo(this.CurrentDate.Value.Value);
			else this.DismantleDescendants();
		}
		protected override void OnSelected(TreeViewNode node) {
			//base.OnSelected(node);
			this._currentNode.Value = node;
		}
	}
}
