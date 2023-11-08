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
	
	public class TreeViewContainer : ObservableTreeNode<TreeViewContainer>, INotifyPropertyChanged {
		protected static TreeViewContainer Create(CommonNode model) {
			var n = model switch {
				FinancialProduct ep => new PositionContainer(ep),
				CashValue ep=> new CashPositionContainer(ep),
				CommonNode ep=> new BasketContainer(ep),
			};
			return n;
		}
		protected TreeViewContainer(CommonNode model) {
			this.Model = model;
			this.IsSelected.Where(x => x).Subscribe(_ => OnSelected(this));
		}
		protected TreeViewContainer() : this(new AnonymousNode()) { }

		public ReactiveProperty<bool> IsExpand { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>(false);
		public CommonNode Model { get; }
		protected virtual void OnSelected(TreeViewContainer node) {
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
	public class BasketContainer : TreeViewContainer {
		public BasketContainer(CommonNode model):base(model) {
			Disposable.Create(() => this.Editer?.Dispose()).AddTo(Disposables);
			this.Name = model.ObserveProperty(x => x.Name).ToReadOnlyReactiveProperty<string>().AddTo(Disposables);
			this.Amount = model.ObserveProperty(x=>x.Amount).ToReadOnlyReactiveProperty().AddTo(Disposables);
		}
		protected virtual ElementEditer GenerateEditer() {
			return new BasketEditer(Model);
		}
		ElementEditer? _editer;
		public ElementEditer Editer { get => _editer ??= GenerateEditer(); }
		public ReadOnlyReactiveProperty<string> Name { get; }
		public ReadOnlyReactiveProperty<long> Amount { get; }
		ICommand? applyCommand;
		ICommand? cancelCommand;
		public ICommand ApplyCommand {
			get => applyCommand ??= GenerateApplyCommand();
		}
		protected virtual ICommand GenerateApplyCommand() {
			return this.Editer.CanApply.ToReactiveCommand().WithSubscribe(() => this.Editer.Apply()).AddTo(Disposables);
		}
		public ICommand CancelCommand {
			get => cancelCommand ??= GenerateCancelCommand();
		}
		protected virtual ICommand GenerateCancelCommand() {
			return new ReactiveCommand().WithSubscribe(() => this.Editer.Reset()).AddTo(Disposables);
		}
		
	}
    
    public class TreeViewGridRoot : TreeViewContainer {
		private ObservableDictionary<CommonNode, TreeViewContainer> _buffDic { get; }
		public TreeViewGridRoot(ImportAndAdjustmentViewModel viewmodel):base() {
			this._buffDic = viewmodel.NodeCash;
			CurrentDate = _currentDate.ToReadOnlyReactiveProperty();
			CurrentNode = _currentNode.ToReadOnlyReactiveProperty();
			//this.SelectedCommand = new DelegateCommand(() => this.OnSelected(this.Model));
		}
		//public DelegateCommand SelectedCommand { get; set; }
		public ReadOnlyReactiveProperty<DateTime?> CurrentDate { get; private set; }
		private ReactiveProperty<DateTime?> _currentDate { get; } = new ReactiveProperty<DateTime?>();
		public ReadOnlyReactiveProperty<TreeViewContainer?> CurrentNode { get; private set; }
		private ReactiveProperty<TreeViewContainer?> _currentNode { get; } = new ();
		public void DisplayTo(DateTime date) {
			this.DismantleDescendants();
			//this.ClearChildren();
			var root = RootCollection.Instance.FirstOrDefault(a => a.CurrentDate == date);
			if (root == null) {
				this._currentDate.Value = null;
				this._currentNode.Value = null;
				return;
			}
			var shdw = (root as CommonNode).Convert(a => {
				return ResultWithValue.Of<CommonNode, TreeViewContainer>(_buffDic.TryGetValue, a)
					.TrueOrNot(o => o,
					x => {
						var v = TreeViewContainer.Create(a);
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
		protected override void OnSelected(TreeViewContainer node) {
			//base.OnSelected(node);
			this._currentNode.Value = node;
		}
	}
}
