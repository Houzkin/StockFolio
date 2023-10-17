using Houzkin.Tree;
using LiveChartsCore.Defaults;
using MahApps.Metro.Behaviors;
using PortFolion.Core;
using PortFolion.IO;
using StockFolio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace StockFolio.ExtractData {
	public class DrawCushonValue {
		public string Name { get; set; } = string.Empty;
		public double Amount { get; set; }
		public double Rate { get; set; }
	}
	public class PieBaseValue : DrawCushonValue {
		public int CashCout { get; set; }
	}
	public static class Extract {
		public static IEnumerable<PieBaseValue> ToPieBaseValue(this CommonNode node,int tgtLv,DividePattern div) {
			return node.LevelShift(tgtLv).ToPieBaseValue(div);
		}
		/// <summary>現在のノードから指定した階層だけ下位のノードを返す。</summary>
		private static IEnumerable<CommonNode> LevelShift(this CommonNode cur,int lv) {
			var cd = cur.NodeIndex().CurrentDepth;
			var ch = cur.Height();
			var tg = cd + Math.Min(ch, lv);
			return cur.Levelorder()
				.SkipWhile(a => a.NodeIndex().CurrentDepth < tg)
				.TakeWhile(a => a.NodeIndex().CurrentDepth == tg);
		}
		private static Func<CommonNode,string> getDivFunc(DividePattern div) {
			Func<CommonNode, string> DivFunc;
			if (div.HasFlag(DividePattern.Tag)) {
				DivFunc = a => a.Tag.TagName;
			} else if (div.HasFlag(DividePattern.ThousandRange)) {
				DivFunc = a => {
					if (a is not StockValue riskn) return "other";
					int x = riskn.Code / 1000 * 1000;
					return $"{x}番台";
				};
			} else {
				DivFunc = a => a.Name;
			}
			return DivFunc;
		}
		private static IEnumerable<PieBaseValue> ToPieBaseValue(this IEnumerable<CommonNode> nodes,DividePattern div) {
			//LocationFlagが立っており他フラグも立っていれば、他フラグの処理はデフォルトに任せる
			if (div.HasFlag(DividePattern.Location) && div.FlagCount() >= 2) {
				var secDiv = div & ~DividePattern.Location;
				double sm = nodes.Sum(a => a.Amount);
				return divideNodes(nodes, secDiv)
					.Select(a =>
						nodes.ToLookup(getDivFunc(secDiv))
							.Where(b => b.Key == a.Name)
							.Select(b => divideNodes(b, DividePattern.Location,sm)))
					.SelectMany(a => a.SelectMany(b => b));
			}
			return divideNodes(nodes, div);
		}
		private static IEnumerable<PieBaseValue> divideNodes(this IEnumerable<CommonNode> nodes,DividePattern div,double total=0) {
			var DivFunc = getDivFunc(div);
			total = 0 < total ? total : (double)(nodes.Sum(a => a.Amount));
			if (total == 0) return Enumerable.Empty<PieBaseValue>();
			return nodes.ToLookup(DivFunc)
				.Select(a => {
					var plv = new PieBaseValue();
					plv.Name = a.Key;
					plv.Amount = a.Sum(a => a.Amount);
					plv.Rate = ((double)plv.Amount) / (double)total;
					plv.CashCout = a.Count(b => b.GetNodeType() == NodeType.Cash);
					return plv;
				})
				.GroupBy(a => a.CashCout)
				.OrderBy(a => a.Key)
				.SelectMany(a => a.OrderByDescending(a => a.Amount));
		}
		
	}
	public static class EnumExtension {
		public static bool HasFlags(this Enum self, params Enum[] flags) {
			foreach(var flag in flags) if(self.HasFlag(flag)) return true;
			return false;
		}
		public static int FlagCount<TEnum>(this TEnum self) where TEnum : Enum {
			int count = 0;
            foreach (TEnum flg in Enum.GetValues(typeof(TEnum))) {
				if(self.HasFlag(flg)) count++;
			}
			return count;
        }
		
	}
}
