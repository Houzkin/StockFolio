using Houzkin;
using Prism.Ioc;
using Prism.Mvvm;
using StockFolio.DiContainer;
using StockFolio.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ExtractData
{
	public class AttachDetachSet:INotifyPropertyChanged{
		private object _key;
		private Action _attach;
		private Action _detach;
		private Action _refresh;
		private Action<bool> _del;
		/// <summary></summary>
		/// <param name="key">プロセスを表すkey。TabのVMを想定。</param>
		/// <param name="attach">アタッチ処理。</param>
		/// <param name="refresh">更新処理。</param>
		/// <param name="detach">デタッチ処理。この処理の後、IsAttachedはfalseに変更される。</param>
		/// <param name="del">アタッチ状態で呼ばれた場合は引数にTrueをとる。状態を示すパラメータの保存を想定。</param>
		public AttachDetachSet(object key, Action attach,Action refresh,Action detach,Action<bool> del) {
			_key = key; _refresh = refresh; _attach = attach; _detach = detach; _del = del;
		}
		public AttachDetachSet(object key, Action attach, Action refresh, Action detach) 
			: this(key, attach,  refresh, detach, a => { if (a) detach(); }) {
		}
		public static AttachDetachSet Empty(object key) {
			return new AttachDetachSet(key, () => { }, () => { }, () => { });
		}

		public object Key { get { return _key; } }
		public void Attach() {
            if(this.IsAttached) return;
            _attach();
			IsAttached = true;
		}
		public void Detach() {
			if (!this.IsAttached) return; 
			_detach();
			IsAttached = false;
		}
		public bool Refresh() {
			if (IsAttached) { _refresh(); return true; }
			return false;
		}
		public void Delete() { _del(this.IsAttached); IsAttached = false; }

        public event PropertyChangedEventHandler? PropertyChanged;

		bool _isAttached = false;
        public bool IsAttached {
			get { return _isAttached; }
			private set { 
				if (this.IsAttached != value) {
					_isAttached = value;
					PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(IsAttached)));
				}
			}
		}
	}
	public class UpdateSwitcher : IDisposable{
		public static UpdateSwitcher Switcher { get; } = new UpdateSwitcher();
		private UpdateSwitcher() {
		}

		List<AttachDetachSet> lst = new List<AttachDetachSet>();
		HashSet<object> RefreshRequests = new HashSet<object>();
		void attach(AttachDetachSet[] process) {
			var log = new HashSet<object>();
			foreach (AttachDetachSet prcs in process) {
				prcs.Attach();
				if (RefreshRequests.Contains(prcs.Key)) {
					prcs.Refresh();
					log.Add(prcs.Key);
				}
			}
			log.ForEach(a => RefreshRequests.Remove(a));
		}
		/// <summary>アタッチ関数と更新関数を実行して追加する。</summary>
		/// <param name="process"></param>
		/// <returns>破棄により、デタッチ関数の実行と登録抹消</returns>
		public IDisposable RefreshAndAdd(AttachDetachSet process) {
			RefreshRequests.Add(process.Key);
			return Add(process);
		}
		public IDisposable Add(AttachDetachSet process) {
			lst.Add(process);
			if (this.CurrentKey == process.Key) { attach(new[] { process }); }
			return Disposable.Create(() => {
				process.Delete();
				lst.Remove(process);
			});
		}
		public object? CurrentKey { get; set; }
		
		/// <summary>排他的にデタッチ関数を実行する</summary>
		/// <param name="key">キー</param>
		public void Detach(object key) {
			lst.Where(a => a.Key == key).Where(a => a.IsAttached).ForEach(a => a.Detach());
        }
		/// <summary>指定したプロセスキーのみをアタッチ状態にする。</summary>
		/// <param name="key">プロセスキー</param>
		public void AttachOnly(object key) {
			lst.Where(a => a.Key != key).ForEach(a => a.Detach());
			attach(lst.Where(a => a.Key == key).ToArray());//.ForEach(a => a.Attach());
			this.CurrentKey = key;
		}
		void Attach(IList<object> keys) {
			attach(lst.Where(a => keys.Contains(a.Key)).ToArray());//.ForEach(a => a.Attach());
		}
		public void OnAndOff(object onkey,object offkey) {
			lst.Where(a => a.Key == offkey).ForEach(a => a.Detach());
			attach(lst.Where(a => a.Key == onkey).ToArray());//.ForEach(a => a.Attach());
			this.CurrentKey = onkey;
		}
		/// <summary>アタッチ状態のプロセスを更新する。</summary>
		public void Refresh() {
			var tgt = lst.Where(a => a.IsAttached);
			tgt.ForEach(a => a.Refresh());
			tgt.Select(a => a.Key).Distinct().ForEach(a => RefreshRequests.Remove(a));
			//lst.ForEach(a=> a.Refresh());
		}
		public void DetachAll() {
			lst.ForEach(a => a.Detach());
		}
		IList<object> GetAttachingKeys() {//windowの取り込みlayoutを想定 -> Tabにしたのでいらないかも
			return lst.Where(a => a.IsAttached).Select(a => a.Key).Distinct().ToList();
		}
		public IDisposable Pending() {//windowの取り込みlayoutを想定 -> Tabにしたのでいらないかも
			var memori = GetAttachingKeys();
			DetachAll();
			return Disposable.Create(() => {
				Attach(memori);
				Refresh();
			});
		}
		public void Dispose() {
			lst.ForEach(a => a.Delete());
			lst.Clear();
		}
	}
}
