using Houzkin.Tree;
using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels
{
	abstract public class DateTreeViewModel : ObservableTreeNode<DateTreeViewModel>, INotifyPropertyChanged
	{
		protected DateTreeViewModel(Func<int,string> toDisplay)
		{
			this.Children.CollectionChanged += (o, e) => RaisePropertyChanged(nameof(Children));
			this.NumberDsp = Number.Select(x => toDisplay(x)).ToReadOnlyReactiveProperty();
		}
		protected void RaisePropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		public event PropertyChangedEventHandler? PropertyChanged;

		public virtual ReactiveProperty<int> Number { get; } = new();
		public ReadOnlyReactiveProperty<string?> NumberDsp { get; private set; }
		public ReactiveProperty<bool> IsExpand { get; } = new(false);
		public ReactiveProperty<bool> IsSelected { get; } = new(false);
		protected virtual void RaiseDateTimeSelected(DateTime? date)
			=> this.Root().RaiseDateTimeSelected(date);

		public virtual DateTime? CurrentDate
			=> this.Levelorder().OfType<DateTreeLeafViewModel>().Select(a => a.CurrentDate).OrderBy(a => a).LastOrDefault();
		public void Sort() => this.ChildNodes.Sort(a => a.Number.Value);
	}
	public class DateTreeLeafViewModel : DateTreeViewModel
	{
		public DateTreeLeafViewModel(DateTime date) : base(num => $"{num}日")
		{
			_date = date;
			this.Number.Value = _date.Day;
			this.IsSelected.Where(x => x).Subscribe(_ =>
			{
				foreach (var t in this.Upstream().Skip(1)) t.IsExpand.Value = true;
				this.RaiseDateTimeSelected(_date);
			});
		}
		readonly DateTime _date;
		public override DateTime? CurrentDate => _date;
	}
	public class DateTreeNodeViewModel : DateTreeViewModel
	{
		public DateTreeNodeViewModel(int num,Func<int,string> toDisplay) : base(toDisplay)
		{
			this.Number.Value = num;
		}
	}
	public class DateTreeRootViewModel : DateTreeViewModel
	{
		public DateTreeRootViewModel() : base(_ => "")
		{
			//Refresh();
		}
		void assembleTree(DateTime date)
		{
			var y = Children.FirstOrDefault(a => a.Number.Value == date.Year);
			if (y == null)
			{
				y = new DateTreeNodeViewModel(date.Year, a => $"{a}年");
				this.AddChild(y);
			}
			var m = y.Children.FirstOrDefault(a => a.Number.Value == date.Month);
			if (m == null)
			{
				m = new DateTreeNodeViewModel(date.Month, a => $"{a}月");
				y.AddChild(m);
			}
			if (!m.Children.Any(a => a.CurrentDate == date)) m.AddChild(new DateTreeLeafViewModel(date));

		}
		public void Refresh()
		{
			var cur = RootCollection.Instance.Select(a => a.CurrentDate);
			var prv = this.Preorder().OfType<DateTreeLeafViewModel>().Select(a => (DateTime)a.CurrentDate);
			var ads = cur.Except(prv).ToArray();
			var rmv = prv.Except(cur).ToArray();
			foreach (var d in ads)
				assembleTree(d);
			foreach (var d in rmv)
				foreach (var dd in this.Preorder().OfType<DateTreeLeafViewModel>().Where(a => a.CurrentDate == d).ToArray())
					dd.Parent.RemoveChild(dd);
			var rl = this.Levelorder().Skip(1).Where(a => a.Preorder().OfType<DateTreeLeafViewModel>().IsEmpty()).ToArray();
			foreach (var r in rl) r.MaybeRemoveOwn();
			this.Levelorder().ToArray().ForEach(a => a.Sort());
			
			if (this.CurrentDate != null) this.SelectAt((DateTime)this.CurrentDate);
		}
		public event EventHandler<DateTimeSelectedEventArgs>? DateTimeSelected;
		/// <summary>
		/// プログラム内より日付を選択する関数。
		/// </summary>
		/// <param name="date">日付。該当する日付が存在しなかった場合は直前の、それでも存在しない場合は直後の日付が選択される。</param>
		public void SelectAt(DateTime date)
		{
			var n = this.Levelorder().OfType<DateTreeLeafViewModel>().LastOrDefault(a => a.CurrentDate <= date)
				?? this.Levelorder().OfType<DateTreeLeafViewModel>().FirstOrDefault(a=>a.CurrentDate >= date);
			if (n != null) n.IsSelected.Value = true;
		}
		protected override void RaiseDateTimeSelected(DateTime? date)
		{
			_date = date;
			RaisePropertyChanged(nameof(this.CurrentDate));
			DateTimeSelected?.Invoke(this, new DateTimeSelectedEventArgs(date));
		}
		DateTime? _date;
		public override DateTime? CurrentDate => _date ?? base.CurrentDate;
	}

	public class DateTimeSelectedEventArgs : EventArgs
	{
		public DateTime? SelectedDateTime { get; private set; }
		public DateTimeSelectedEventArgs(DateTime? dt) { SelectedDateTime = dt; }
	}
	
}
