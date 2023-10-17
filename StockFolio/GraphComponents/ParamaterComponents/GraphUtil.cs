using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.Defaults;

namespace StockFolio.ViewModels
{
	/// <summary>期間</summary>
	public enum TimeScale
	{
		Weekly,
		Monthly,
		Quarterly,
		Yearly,
	}
	/// <summary>識別方法</summary>
	public enum DividePattern {
		Location = 1,           //0b_0000_0000_0001,
		Tag = 2,                //0b_0000_0000_0010,
		LocationAndTag = Location | Tag,
		Industry17 = 4,
		Industry33 = 8,
		Captialization = 16,
		MarketSegment = 32,
		ThousandRange = 64,
	}
	/// <summary>残高に対するキャッシュフローの扱い方</summary>
	public enum BalanceCashFlow
	{
		Ignore,
		Flow,
		Stack,
	}

}
