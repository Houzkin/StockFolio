using Houzkin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ExtractData
{
	public class AttachDetachSet {
		private object _key;
		private Action _attach;
		private Action _detach;
		private Action _refresh;
		private Action<bool> _del;
		/// <summary>
		/// 
		/// </summary>
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
		public void Refresh() { if(IsAttached) _refresh(); }
		public void Delete() { _del(this.IsAttached); IsAttached = false; }
		public bool IsAttached { get; private set; } = false;
	}
	public class AttachDetachSwitcher : IDisposable{
		List<AttachDetachSet> lst = new List<AttachDetachSet>();
		/// <summary>アタッチ関数と更新関数を実行して追加する。</summary>
		/// <param name="process"></param>
		/// <returns>破棄により、デタッチ関数の実行と登録抹消</returns>
		public IDisposable AttachAndAdd(AttachDetachSet process) {
			lst.Add(process);
			process.Attach();
			process.Refresh();
			return Disposable.Create(()=> {
				process.Delete();
				lst.Remove(process);
			});
		}
		public IDisposable Add(AttachDetachSet process) {
			lst.Add(process);
			process.Attach();
			return Disposable.Create(() => {
				process.Delete();
				lst.Remove(process);
			});
		}
		/// <summary>デタッチ関数を実行する</summary>
		/// <param name="key">キー</param>
		public void Detach(object key) {
			lst.Where(a => a.Key == key).Where(a => a.IsAttached).ForEach(a => a.Detach());
        }
		/// <summary>指定したプロセスキーのみをアタッチ状態にする。</summary>
		/// <param name="key">プロセスキー</param>
		public void AttachOnly(object key) {
			lst.Where(a => a.Key != key).ForEach(a => a.Detach());
			lst.Where(a => a.Key == key).ForEach(a => a.Attach());
		}
		void Attach(IList<object> keys) {
			lst.Where(a => keys.Contains(a.Key)).ForEach(a => a.Attach());
		}
		public void OnAndOff(object onkey,object offkey) {
			lst.Where(a => a.Key == offkey).ForEach(a => a.Detach());
			lst.Where(a => a.Key == onkey).ForEach(a => a.Attach());
		}
		/// <summary>アタッチ状態のプロセスを更新する。</summary>
		public void Refresh() {
			lst.ForEach(a=> a.Refresh());
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
