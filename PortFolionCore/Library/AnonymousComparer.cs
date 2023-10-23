using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections {
	public class AnonymousComparer<T> : IComparer<T>, IComparer {
		private readonly Func<T, T, int> _comparer;
		public AnonymousComparer(Func<T, T, int> comparer) {
			_comparer = comparer;
		}

		public int Compare(T x, T y) {
			return _comparer(x, y);
		}

		public int Compare(object x, object y) {
			return _comparer((T)x, (T)y);
		}
	}
	public static class AnonymousComparer {
		public static AnonymousComparer<T> Create<T>() {
			//Func<T, T, int> cmp = 
			return new AnonymousComparer<T>((x, y) => {
				var gsrc = x as IComparable<T>;
				var gtgt = y as IComparable<T>;
				if (gsrc != null) return gsrc.CompareTo(y);
				var src = x as IComparable;
				var tgt = y as IComparable;
				return src != null ? src.CompareTo(tgt) : 0;
			});
		}
	}
}
