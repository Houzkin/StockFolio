using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using StockFolio.ExtractData;
using StockFolio.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StockFolio.ViewModels
{
	internal class MainViewModel : BindableBase
	{
		static MainViewModel? _self;
		static AssetTransitionViewModel? _assetTransition;
		static PortfolioHoldingsViewModel? _portfolioHoldings;
		static TradeHistoryViewModel? _tradeHistory;
		static TradeAnalysisViewModel? _tradeRsults;
		static ImportAndAdjustmentViewModel? _adjustment;
		public static MainViewModel GetMainViewModel()
		{
			return _self ?? new();
		}

		public AttachDetachSwitcher compositeAttachDettach { get; } = new AttachDetachSwitcher();
		public AssetTransitionViewModel AssetTransitionVM => _assetTransition ??= new AssetTransitionViewModel(compositeAttachDettach);
		public PortfolioHoldingsViewModel PortfolioHoldingsVM => _portfolioHoldings ??= new PortfolioHoldingsViewModel(compositeAttachDettach);
		public TradeHistoryViewModel TradeHistoryVM => _tradeHistory ??= new TradeHistoryViewModel();
		public TradeAnalysisViewModel TradeAnalysisVM => _tradeRsults ??= new TradeAnalysisViewModel();
		public ImportAndAdjustmentViewModel ImportAndAdjustmentVM => _adjustment ??= new ImportAndAdjustmentViewModel();
		//public DelegateCommand<SelectionChangedEventArgs> SelectedTabCommand { get; }
		public ReactiveCommand<AttachOrderArgs> TabSelectionCommand { get; } = new ReactiveCommand<AttachOrderArgs>();
		public DelegateCommand ReloadData { get; }
		public DelegateCommand ShowImputDataLayar { get; }

		public DelegateCommand ClosedCommand { get; }

		public MainViewModel() {
			_self = this;
			this.ReloadData = new DelegateCommand(
				() => { /* データ再読み込みコマンドの処理 */
					compositeAttachDettach.Refresh();
				});
			this.ShowImputDataLayar = new DelegateCommand(
				() => { /* インプットレイヤーの表示処理 */ });

			//this.SelectedTabCommand = new DelegateCommand<SelectionChangedEventArgs>(e => {
			//	var cur = (e.AddedItems[0] as TabItem)?.DataContext;
			//	var rmv = (e.RemovedItems[0] as TabItem)?.DataContext;
			//	compositeAttachDettach.OffAndOn(rmv, cur);
			//});
			this.TabSelectionCommand.Subscribe(a => {
				if (a.OriginSourceDataContext != this) return;
				compositeAttachDettach.OnAndOff(a.ActiveDataContext, a.DeactiveDataContext);
			});
			this.ClosedCommand = new DelegateCommand(() => this.compositeAttachDettach.Dispose());
		}
		
		
	}
}
