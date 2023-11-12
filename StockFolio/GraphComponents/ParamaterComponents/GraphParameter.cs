using Houzkin.Architecture;
using Houzkin.Tree;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels
{
	public class GraphParameter:ViewModelBase
	{
		//public ReactiveProperty<int> TargetLevelOrder { get; } = new ReactiveProperty<int>(1);
		//public ReactiveProperty<int> TargetLevel { get; private set; }
		public ReactiveProperty<TimeScale> TimePeriod { get; } = new(TimeScale.Weekly);
		public ReactiveProperty<DividePattern> Divide { get; } = new(DividePattern.Location);
		private LocationAsset location;
		public GraphParameter(LocationAsset la)
		{
			location = la;
			//this.TargetLevel = TargetLevelOrder.Select(x => Math.Max(x, 0)).ToReactiveProperty(1);
			//this.TargetLevel.Where(x => 0 <= x && x <= la.CurrentNode.Height()).Subscribe(x => this.TargetLevelOrder.Value = x);
			
		}
		int _level = 1;
		public int Level
		{
			get {
				return Math.Max(0, Math.Min(_level, location.CurrentNode?.Height() ?? 1)); 
			}
			set {
				var pre = this.Level;
				var tes = location.CurrentNode?.Height() ?? 1;
				if (0 <= value && value <= tes) _level = value;
				if(pre!=this.Level || value== -1)
					this.OnPropertyChanged(nameof(Level));
			}
		}
	}
}
