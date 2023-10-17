using Houzkin.Architecture;
using Houzkin.Tree;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SkiaSharp;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels
{
	public class AssetTransitionViewModel:ViewModelBase
	{
		public AttachDetachSwitcher compositeAttacheDetach { get; private set; }
		#region Params and Setting
		public DateTreeRootViewModel DateTree { get; private set; }
		public LocationAsset LocationTree { get; private set; }
		public GraphParameter Params { get; private set; }
		CompositeDisposable disposables = new CompositeDisposable();
		HashSet<string> changedStack = new();
		#endregion

		#region Charts
		public BrakeDownViewModel BrakeDownPie { get; private set; }
		public BrakeDownViewModel BrakeDownInnerPie { get; private set; }
		#endregion
		public AssetTransitionViewModel(AttachDetachSwitcher cad) {
			compositeAttacheDetach = cad;
			LocationTree = new();
			DateTree = new();
			this.Params = new GraphParameter(LocationTree);

			var ad = new AttachDetachSet(this, () => { }, () => DateTree.Refresh() , () => { });
			this.compositeAttacheDetach.Add(ad);

			#region 各プロパティ変更の購読
			DateTree.ObserveProperty(x => x.CurrentDate).Do(x => {
				if (x != null) {
					changedStack.Add(nameof(DateTree.CurrentDate));
					LocationTree.SelectAt(x.Value); }
			}).Subscribe(_ => RaiseConditionChanged());

			LocationTree.ObserveProperty(x => x.CurrentNodePath).Do(a => {
				changedStack.Add(nameof(LocationTree.CurrentNodePath));
				Params.Level = -1;
			}).Subscribe(_ => RaiseConditionChanged());

			Params.Divide.Do(a => changedStack.Add(nameof(Params.Divide))).Subscribe(_=>RaiseConditionChanged());
			
			Params.TimePeriod.Do(a => changedStack.Add(nameof(Params.TimePeriod))).Subscribe(_=>RaiseConditionChanged());

			Params.ObserveProperty(x=>x.Level)
				.Do(a => changedStack.Add(nameof(Params.Level)))
				.Subscribe(x => RaiseConditionChanged());
			#endregion

			#region Pie
			this.BrakeDownPie = new BrakeDownViewModel(this);
			this.BrakeDownInnerPie = new InnerBrakeDownViewModel(this);
			#endregion
		}
		public void RaiseConditionChanged() {
			if (changedStack.IsEmpty()) return;
			ConditionChanged?.Invoke(this, new GraphConditionChangedEventArgs(changedStack));
			changedStack.Clear();
		}
		public event EventHandler<GraphConditionChangedEventArgs>? ConditionChanged;

		protected override void Dispose(bool disposing)
		{
			if (disposing) { disposables.Dispose(); }
		}
	}
	public class GraphConditionChangedEventArgs: EventArgs
	{
		public GraphConditionChangedEventArgs(IEnumerable<string> paramNames) {
			this.ParamNames = paramNames ?? Enumerable.Empty<string>();
		}
		public IEnumerable<string> ParamNames { get; private set; }
	}
}
