using Houzkin.Architecture;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using PortFolion.Core;
using Reactive.Bindings;
//using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using SkiaSharp;
using StockFolio.ExtractData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StockFolio.ViewModels
{
    public class BrakeDownViewModel:ViewModelBase
    {
        AssetTransitionViewModel _atvm;
        public ObservableCollection<PieSeries<double>> Series { get; private set; } = new();
        public ObservableCollection<object> Legend { get; private set; } = new();
        public virtual SolidColorPaint TooltipTextPaint { get => new SolidColorPaint { Color = SKColors.LightGray, }; } 
        public virtual SolidColorPaint TooltipBackgroundPaint { get; } = new SolidColorPaint { Color = SKColors.Black.WithAlpha(150), };

        CompositeDisposable dettachesList = new ();
        List<IDisposable> ConditionObserver = new ();
        public BrakeDownViewModel(AssetTransitionViewModel atvm) {
            _atvm = atvm;
            var ads = new AttachDetachSet(atvm, AttachFunc, Update, DetachFunc, a => { if (a) DetachFunc(); });
            _atvm.compositeAttacheDetach
                .AttachAndAdd(ads)
                .AddTo(dettachesList);
            
            //this.AttachCommand.Subscribe(_ => ads.Attach());
            //this.DettachCommand.Subscribe(_ => ads.Detach());
            //this.Update();
        }
        void AttachFunc() {
            var trrigers = new List<string> { 
                nameof(_atvm.DateTree.CurrentDate), 
                nameof(_atvm.LocationTree.CurrentNodePath),
                nameof(_atvm.Params.Level),
                nameof(_atvm.Params.Divide)
            };
            Observable.FromEvent<EventHandler<GraphConditionChangedEventArgs>, GraphConditionChangedEventArgs>(
                h => (s, e) => h(e),
                h => _atvm.ConditionChanged += h,
                h => _atvm.ConditionChanged -= h)
                .Where(e => e.ParamNames.Intersect(trrigers).Any()).Subscribe(_ => this.Update()).AddTo(ConditionObserver);// .AddTo(dettachesList);
        }
        void DetachFunc() {
            ConditionObserver.ForEach(a => a.Dispose());
            ConditionObserver.Clear();
        }

        public void Update() {
            Series.Clear();
            Legend.Clear();
            SetSeriesAndLegend(_atvm.LocationTree.CurrentNode,_atvm.Params.Level,_atvm.Params.Divide.Value);

        }
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
            this.dettachesList.Dispose();
		}
		protected virtual IEnumerable<Color> GetColors(int len) {
            return ColorSrc.Pies(len);
        }
        protected virtual void SetSeriesAndLegend(CommonNode? node, int level, DividePattern div) {
            var pb = node?.ToPieBaseValue(level, div).ToArray() ?? Enumerable.Empty<PieBaseValue>();
            var bs = pb.Zip(GetColors(pb.Count()), (bdata, color) => new { Data = bdata, Color = color }).ToArray();
            var srs = bs.Select((x,idx) => {
                    var ps = new PieSeries<double> {
                        Values = new[] { x.Data.Amount },
                        Name = x.Data.Name,
                        Fill = new SolidColorPaint(x.Color.ToSkColor()),
                        Stroke = new SolidColorPaint(Color.Multiply(x.Color, 0.5f).ToSkColor()) { StrokeThickness = 3 },
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        DataLabelsRotation = LiveCharts.CotangentAngle,
                    };
                    ps.ToolTipLabelFormatter = cp => { 
                        return cp.Coordinate.PrimaryValue.ToString("#,0"); 
                    };
                    if (x.Data.Rate > 0.03 || idx < 3) {
                        ps.DataLabelsFormatter = cp => cp.Context.Series.Name;
                        ps.DataLabelsPaint = new SolidColorPaint(SKColors.LightGray);
                    }
                    //if (x.Data.Rate >= 0.1) ps.DataLabelsSize = 0;
                    //else if (x.Data.Rate > 0.05) ps.DataLabelsSize = 0;
                    //else if (x.Data.Rate > 0.03) ps.DataLabelsSize = 0;
                    return ps;
                });
            this.Series.AddRange(srs);
            if (div.FlagCount() > 1) this.Series.ForEach(s => s.InnerRadius = 60);

            var lgd = bs.Zip(this.Series, (bs, sr) => new { 
                Fill = new SolidColorBrush(bs.Color),
                Name =sr.Name,
                Rate = bs.Data.Rate,
            });
            this.Legend.AddRange(lgd);
        }
    }
	public class InnerBrakeDownViewModel : BrakeDownViewModel {
        public ReactiveProperty<int> ZIndex { get; set; } = new(0);
		public InnerBrakeDownViewModel(AssetTransitionViewModel atvm) : base(atvm) {

		}
		protected override IEnumerable<Color> GetColors(int len) {
			return base.GetColors(len).Reverse();
		}
		
        protected override void SetSeriesAndLegend(CommonNode? node, int level, DividePattern div) {
            if (div.FlagCount() <= 1) {
                this.ZIndex.Value = 0;
                return;
            }
			base.SetSeriesAndLegend(node, level, div & ~DividePattern.Location);
            this.Series.ForEach(s => {
                s.DataLabelsPaint = null;
            });
            this.ZIndex.Value = 2;
		}
	}
}
