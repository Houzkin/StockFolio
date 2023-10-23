using CsvHelper;
using Houzkin.Collections;
using StatefulModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortFolion.Core {
	//public class Roots : ObservableSortedCollection<TotalRiskFundNode, DateTime> ,INotifyCollectionChanged{
		
	//	public Roots():base(Enumerable.Empty<TotalRiskFundNode>(),x=>x.CurrentDate) {
	//	}
		
	//	#region Dictionary like
	//	public TotalRiskFundNode this[DateTime key] {
	//		get {
	//			return Items.FirstOrDefault(x => x.CurrentDate == key) ?? throw new KeyNotFoundException();
	//		}
	//		set {
	//			this.Remove(key);
	//			this.Add(key, value);
	//		}
	//	}

	//	public ICollection<DateTime> Keys => Items.Select(x=> x.CurrentDate).ToArray();

	//	public bool Add(DateTime key, TotalRiskFundNode value) {
	//		if(Items.Any(x=>x.CurrentDate == key)) return false;
	//		value.CurrentDate = key;
	//		this.Insert(0, value);
	//		return true;
	//	}
		
	//	public bool Remove(DateTime key) {
	//		var v = Items.FirstOrDefault(x=>x.CurrentDate == key);
	//		if(v!=null) return this.Remove(v);
	//		return false;
	//	}

	//	public bool ContainsKey(DateTime key) {
	//		return Items.Select(x=> x.CurrentDate == key).Any();
	//	}

	//	public bool TryGetValue(DateTime key, out TotalRiskFundNode value) {
	//		value = Items.FirstOrDefault(x => x.CurrentDate == key);
	//		return value != null ? true : false;
	//	}
	//	#endregion
	//	protected override void InsertItem(int _, TotalRiskFundNode item) {
	//		if(ContainsKey(item.CurrentDate) || Contains(item)) return;
	//		base.InsertItem(_, item);
	//	}
	//	protected override void SetItem(int index, TotalRiskFundNode item) {
	//		if (Contains(item)) return;
	//		item.CurrentDate = Keys.ToList()[index];
	//		base.SetItem(index, item);
	//	}
	//}
}
