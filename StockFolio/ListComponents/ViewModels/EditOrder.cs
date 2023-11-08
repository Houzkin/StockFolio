using Houzkin;
using PortFolion.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
	public interface IEditPresenter : INotifyPropertyChanged {
		public IEnumerable<string> ChargeNames { get; }
		public int Priority { get; }
		public void MaybeOrderValueChanged();
		public bool HasOrder { get; }
		public IUndoOrder Apply();
	}
	public interface IUndoOrder {
		void Undo();
		Dictionary<string,List<object?>> ChangedBeforeAfter { get; }
		string Comment { get; }
	}
	public class UndoOrder : IUndoOrder {
		Action undo;
		public UndoOrder(Action undoAction) {
			undo = undoAction;
			Comment =　"Apply time " + DateTime.Now.ToString("HH:mm:ss") + "\n";
		}
		public Dictionary<string, List<object?>> ChangedBeforeAfter { get; set; } = new();

		public string Comment { get; set; }

		public void Undo() { undo(); }
	}
	public abstract class EditPresenterBase : IEditPresenter {
		//protected EditPresenterBase(string[] chargeNames) { names.AddRange(chargeNames); }
		protected EditPresenterBase(int priority, string[] chargeNames) {
			this.Priority = priority;
			names.AddRange(chargeNames);
		}
		List<string> names = new List<string>();
		public IEnumerable<string> ChargeNames => names;

		public abstract bool HasOrder { get; }

		public int Priority { get; }

		public event PropertyChangedEventHandler? PropertyChanged;

		public abstract IUndoOrder Apply();

		public virtual void MaybeOrderValueChanged() {
			RaisePropertyChanged(nameof(HasOrder));
		}
		protected void RaisePropertyChanged(string propName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
	}
	public class EditPresenter<T> : EditPresenterBase {
		Func<T> getOrderValue;
		Func<T> getBaseValue;
		Action<T> applyAction;
		Action<T> undoAction;
		Func<T, T>? argConv;

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="name">変更する対象</param>
		/// <param name="orderValue">変更内容</param>
		/// <param name="baseValue">変更前の値</param>
		/// <param name="apply">更新関数</param>
		/// <param name="conv">Undo直前にapply関数またはundo関数に適用される引数をbaseValueから変換する</param>
		/// <param name="undo">Undo関数。指定しない場合はapply関数が呼び出される</param>
		public EditPresenter(string name, Func<T> orderValue, Func<T> baseValue, Action<T> apply, Func<T, T>? conv = null, Action<T>? undo = null)
			: this(0,new[] { name, }, orderValue, baseValue, apply, conv, undo) {
		}
        /// <summary>インスタンスを初期化する</summary>
        /// <param name="priority">優先度。小さい値ほど優先度は高い。</param>
        /// <param name="name">変更する対象</param>
        /// <param name="orderValue">変更内容</param>
        /// <param name="baseValue">変更前の値</param>
        /// <param name="apply">更新関数</param>
        /// <param name="conv">Undo直前にapply関数またはundo関数に適用される引数をbaseValueから変換する</param>
        /// <param name="undo">Undo関数。指定しない場合はapply関数が呼び出される</param>
        public EditPresenter(int priority, string name, Func<T> orderValue, Func<T> baseValue, Action<T> apply, Func<T, T>? conv = null, Action<T>? undo = null)
			:this(priority, new[] { name, }, orderValue, baseValue, apply, conv, undo) {
		}
		protected EditPresenter(int priority, string[] names, Func<T> orderValue, Func<T> baseValue, Action<T> apply, Func<T, T>? conv = null, Action<T>? undo = null)
			: base(priority, names) {
			getOrderValue = orderValue;
			getBaseValue = baseValue;
			applyAction = apply;
			undoAction = undo ?? apply;
			argConv = conv;
		}
		protected T OrderValue => getBaseValue();
		protected T BaseValue => getBaseValue();

		public override bool HasOrder => !object.Equals(getOrderValue(), getBaseValue());

		public override IUndoOrder Apply() {
			var beforeValue = getBaseValue();
			var arg = getOrderValue();
			var undoarg = argConv == null ? beforeValue : argConv(arg);
			applyAction(arg);
			RaisePropertyChanged(nameof(HasOrder));
			var ud = new UndoOrder(() => undoAction(undoarg));
			ud.ChangedBeforeAfter.Add(string.Join("&",this.ChargeNames), new List<object?> { beforeValue, getBaseValue(), });
			return ud;
		}
	}
	public class MultiParamEditPresenter<T> : EditPresenter<T> {
		Dictionary<string, Func<T?, object>> splitDic;

		public MultiParamEditPresenter(Dictionary<string, Func<T?, object>> names, Func<T> orderValue, Func<T> baseValue, Action<T> apply, Func<T, T>? conv = null, Action<T>? undo = null)
			: this(0, names, orderValue, baseValue, apply, conv, undo) {
			splitDic = names;
		}
		public MultiParamEditPresenter(int priority, Dictionary<string, Func<T?, object>> names, Func<T> orderValue, Func<T> baseValue, Action<T> apply, Func<T, T>? conv = null, Action<T>? undo = null)
			: base(priority, names.Keys.ToArray(), orderValue, baseValue, apply, conv, undo) {
			splitDic = names;
		}
		public override bool HasOrder {
			get {
				//メンバーの比較。全ての比較結果がfalse(変更あり)の時trueを返す。
				return splitDic.Values.All(x => !object.Equals(x(OrderValue), x(BaseValue)));
			}
		}
		public override IUndoOrder Apply() {
			var ud = (base.Apply() as UndoOrder)!;
			var joinName = string.Join("&", this.ChargeNames);
			ResultWithValue.Of<string, List<object?>>(ud.ChangedBeforeAfter.Remove, joinName)
				.TrueOrNot(o => {
					foreach (var name in splitDic.Keys) ud.ChangedBeforeAfter[name]
						= new List<object?>(o.Select(x => {
							return splitDic[name](x is null ? default : (T)x);
							}));
				});
			return ud;
		}
	}

    public class AddNodePresenter : EditPresenterBase {
		Func<bool> _hasOrder;
		Action _apply;
		Action _undo;
		CommonNode _node;
		/// <summary>優先度-1でインスタンスを初期化</summary>
		/// <param name="chargeNames">変更に反応する名前を指定</param>
		/// <param name="node">Undoのステータス取得に使用するnode</param>
		/// <param name="hasOrder">実行可能なオーダーかどうかを示す</param>
		/// <param name="apply">apply関数</param>
		/// <param name="undo">undo関数</param>
		public AddNodePresenter(string[] chargeNames,CommonNode node, Func<bool> hasOrder, Action apply, Action undo)
			: base(-1,chargeNames ){
			_node = node;
			_hasOrder = hasOrder;
			_apply = apply;
			_undo = undo;
		}
        /// <summary>優先度-1でインスタンスを初期化。</summary>
        /// <param name="chargeName">変更に反応する名前を指定</param>
		/// <param name="node">Undoのステータス取得に使用するnode</param>
		/// <param name="hasOrder">実行可能なオーダーかどうかを示す</param>
		/// <param name="apply">apply関数</param>
		/// <param name="undo">undo関数</param>
        public AddNodePresenter(string chargeName,CommonNode node,Func<bool> hasOrder,Action apply, Action undo)
			:this(new string[] { chargeName }, node, hasOrder, apply, undo) { }

		public override bool HasOrder => _hasOrder();

        public override IUndoOrder Apply() {
			_apply();
			RaisePropertyChanged(nameof(HasOrder));
			var ud = new UndoOrder(_undo);
			ud.ChangedBeforeAfter.Add("Name", new List<object?>() {"-",_node.Name });
			var ac = _node as AccountNode;
			if (ac != null) {
				ud.ChangedBeforeAfter.Add("AccountType", new List<object?>() { "-", ac.Account.ToString() });
				ud.ChangedBeforeAfter.Add("Levarage", new List<object?>() {"-",ac.Levarage });
			}
			return ud;
        }
    }
}
