using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockFolio.ViewModels {
	public static class ColorSrc {
		public static SKColor ToSkColor(this Color color) {
			return new SKColor(color.R,color.G,color.B,color.A);
		}
		static IReadOnlyList<Color>? pieColors;
		public static IEnumerable<Color> Pies(int len) {
			if(pieColors == null) {
				pieColors = new List<Color>() {
					Color.FromRgb(17,140,18),
					Color.FromRgb(214,54,96),
					Color.FromRgb(59,67,255),
					Color.FromRgb(167,68,227),
					Color.FromRgb(237,103,30),

					Color.FromRgb(98,129,19),
					Color.FromRgb(57,63,158),
					Color.FromRgb(248,63,27),
					Color.FromRgb(130,16,142),
					Color.FromRgb(21,157,184),

					Color.FromRgb(175,176,0),
					Color.FromRgb(221,39,124),
					Color.FromRgb(132,79,175),
					Color.FromRgb(7,190,48),
					Color.FromRgb(216,105,26),

					Color.FromRgb(33,145,242),
					Color.FromRgb(225,116,223),
					Color.FromRgb(95,44,159),
					Color.FromRgb(170,119,10),
					Color.FromRgb(69,202,212),

					Color.FromRgb(199,27,76),
					Color.FromRgb(212,200,11),
					Color.FromRgb(11,186,94),
					Color.FromRgb(97,37,241),
					Color.FromRgb(255,81,5),
				};
			}
			var c = pieColors.Repeat().Take(len).ToList();
			if(len % pieColors.Count == 1 && 1 < len) {
				c.RemoveAt(len - 1);
				c.Add(pieColors[3]);
			}
			return c;
		}
		static List<Color>? colors;
		public static IReadOnlyList<Color> BrushColors() {
			return colors = colors ?? new List<Color>() {
				Color.FromRgb(22,207,53),
				Color.FromRgb(255,72,185),
				Color.FromRgb(91,108,247),
				Color.FromRgb(255,156,64),
				Color.FromRgb(187,87,248),

				Color.FromRgb(28,247,194),
				Color.FromRgb(242,212,24),
				Color.FromRgb(212,49,149),
				Color.FromRgb(144,198,43),
				Color.FromRgb(117,92,246),

				Color.FromRgb(246,92,100),
				Color.FromRgb(211,138,62),
				Color.FromRgb(190,99,243),
				Color.FromRgb(197,228,23),
				Color.FromRgb(26,199,146),

				Color.FromRgb(248,59,13),
				Color.FromRgb(88,112,255),
				Color.FromRgb(253,88,254),
				Color.FromRgb(220,150,20),
				Color.FromRgb(20,157,53),

				Color.FromRgb(142,55,151),
				Color.FromRgb(235,59,70),
				Color.FromRgb(248,157,13),
				Color.FromRgb(40,185,199),

			};
		}
	}
}
