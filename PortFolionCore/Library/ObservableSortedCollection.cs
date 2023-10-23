using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections{
	public class ObservableSortedCollection<T, TKey> : ObservableCollection<T>, ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
		where TKey : IComparable<TKey> where T : INotifyPropertyChanged {

		Expression<Func<T, TKey>> _selector;
		Func<T, TKey> _toKey;
		IComparer<TKey> _comparer;
		bool _isDescending;
		string _propName;
		object locObj = new object();
		public ObservableSortedCollection(IEnumerable<T> items, Expression<Func<T, TKey>> keyselector, IComparer<TKey> compare = null, bool isDescending = true) {
			_selector = keyselector;
			_toKey = keyselector.Compile();
			_isDescending = isDescending;
			_propName = (keyselector.Body as MemberExpression)?.Member.Name ?? throw new ArgumentException("プロパティ名を特定できません");

			if (compare == null) {
				_comparer = AnonymousComparer.Create<TKey>();
			} else {
				_comparer = compare;
			}
			_comparer = isDescending ? new AnonymousComparer<TKey>((x, y) => _comparer.Compare(x, y) * -1) : _comparer;
			foreach (var item in items) { this.Add(item); }
		}
		protected int FirstIndexOf(T item) {
			TKey itmky = _toKey(item);
			var ti = this.FirstOrDefault(x => _comparer.Compare(_toKey(x), itmky) >= 0);
			if (ti == null) return -1;
			return this.IndexOf(ti);
		}
		protected int LastIndexOf(T item) {
			TKey itmky = _toKey(item);
			var ti = this.LastOrDefault(x => _comparer.Compare(_toKey(x), itmky) <= 0);
			if (ti == null) return -1;
			return this.IndexOf(ti);
		}
		void RaiseMoveProcessWhenPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (sender is T item && e.PropertyName == _propName) this.MoveItem(IndexOf(item), 0);
		}
		#region 追加処理
		protected override void InsertItem(int _, T item) {
			lock (locObj) {
				var idx = LastIndexOf(item) + 1;
				base.InsertItem(idx, item);
				item.PropertyChanged += RaiseMoveProcessWhenPropertyChanged;
			}
		}

		protected override void SetItem(int index, T item) {
			lock (locObj) {
				this[index].PropertyChanged -= RaiseMoveProcessWhenPropertyChanged;
				base.SetItem(index, item);
				base.MoveItem(index, 0);
				item.PropertyChanged += RaiseMoveProcessWhenPropertyChanged;
			}
		}
		#endregion
		protected override void MoveItem(int oldIndex, int _) {
			lock (locObj) {
				var firstIndex = this.FirstIndexOf(this[oldIndex]);
				if (oldIndex < firstIndex)
					base.MoveItem(oldIndex, firstIndex);

				var lastIndex = this.LastIndexOf(this[oldIndex]);
				if (lastIndex < oldIndex)
					base.MoveItem(oldIndex, lastIndex + 1);
			}
		}
		#region 削除処理
		protected override void RemoveItem(int index) {
			lock (locObj) {
				this[index].PropertyChanged -= RaiseMoveProcessWhenPropertyChanged;
				base.RemoveItem(index);
			}
		}
		protected override void ClearItems() {
			lock (locObj) {
				foreach (var item in this) {
					item.PropertyChanged -= RaiseMoveProcessWhenPropertyChanged;
				}
				base.ClearItems();
			}
		}
		#endregion


	}
}
