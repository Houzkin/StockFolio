using Houzkin.Architecture;
using Houzkin.Tree;
using PortFolion.Core;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StockFolio.ViewModels
{
	public class LocationNode : ReadOnlyBindableTreeNode<CommonNode, LocationNode>
	{
		public LocationNode(CommonNode model) : base(model) {
			this.Name = model.Name;
			this.IsSelected.Where(x => x)
				.Do(_ => this.Upstream().Skip(1).ForEach(a => a.IsExpand.Value = true))
				.Subscribe(_ => this.RaiseLocationSelected(this.Model));
		}
		protected LocationNode():base(new AnonymousNode()) { this.Name = "LocationRoot"; }

		protected override LocationNode GenerateChild(CommonNode modelChildNode) => new(modelChildNode);
		
		public bool IsModelEquals(CommonNode other) => other == this.Model;
		public NodePath<string> Path => this.Model.Path;
		public ReactiveProperty<bool> IsExpand { get; } = new(false);
		public ReactiveProperty<bool> IsSelected { get; } = new(false);
		public string Name { get; private set; }
		protected virtual void RaiseLocationSelected(CommonNode node) {
			var r = this.Root();// as LocationAsset;
			if (r != null) { r.RaiseLocationSelected(node); }
		}
	}
	public class LocationAsset : LocationNode {
		public LocationAsset() : base() {
			this.SelectAt(DateTime.Today);
		}

		private ObservableCollection<CommonNode> _Current = new ObservableCollection<CommonNode>();
		/// <summary>
		/// 現在のルート。Children[0]のモデルと同一インスタンス。
		/// </summary>
		public CommonNode? CurrentRoot { 
			get => _Current.Any() ? _Current[0] : null;
			private set {
				if (_Current.Any() && _Current[0] == value) return;
				if(_Current.Any()) _Current.Clear();
				if (value != null) _Current.Add(value);
				this.OnPropertyChanged(nameof(CurrentRoot));
				this.OnPropertyChanged(nameof(Children));
			}
		}
		CommonNode? _node;
		public CommonNode? CurrentNode {
			get => _node ?? this.CurrentRoot;
			set {
				var p = this.CurrentNodePath;
				this.SetProperty(ref _node, value);
				if (!this.CurrentNodePath.SequenceEqual(p)) {
					this.OnPropertyChanged(nameof(CurrentNodePath));
					this.OnPropertyChanged(nameof(CurrentPath));
				}
			}
		}
		public IEnumerable<string> CurrentNodePath { get => this.CurrentNode?.Path ?? this.CurrentRoot?.Path ?? Enumerable.Empty<string>(); }
		public string CurrentPath { get => (this.CurrentNode?.Path ?? this.CurrentRoot?.Path)?.ToString("/") ?? ""; }
		public void SelectAt(CommonNode cur) {
			if (this.CurrentRoot == null || !this.Levelorder().Any(a => a.IsModelEquals(cur))) {
				this.CurrentRoot = cur.Root();
			}
			var s = this.Levelorder().FirstOrDefault(a => a.IsModelEquals(cur));
			if (s != null) {
				s.IsExpand.Value = true;
				s.IsSelected.Value = true;
			}
		}
		public void SelectAt(DateTime date) {
			var slsn = RootCollection.Instance.LastOrDefault(a => a.CurrentDate <= date)
				?? RootCollection.Instance.FirstOrDefault(a => date <= a.CurrentDate);
			var cash = this.CurrentNodePath;
			this.CurrentRoot = slsn;
			if (slsn == null)
				this.CurrentNode = null;
			else 
				SelectAt(slsn.SearchNodeOf(cash));
		}
		protected override IEnumerable<CommonNode> DesignateChildCollection(IEnumerable<CommonNode> modelChildNodes) {
			return this._Current; /*base.DesignateChildCollection(modelChildNodes);*/ 
		}
		protected override void RaiseLocationSelected(CommonNode node) {
			this.CurrentNode = node;
			LocationSelected?.Invoke(this, new LocationSelectedEventArgs(node));
		}
		public event EventHandler<LocationSelectedEventArgs>? LocationSelected;
	}
	public class LocationSelectedEventArgs : EventArgs {
		public LocationSelectedEventArgs(CommonNode node) {
			Location = node;
		}
		public CommonNode Location { get; private set; }
	}
}
