﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Prism;
using Prism.Ioc;
using StockFolio.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using StockFolio.ExtractData;
using StockFolio.ViewModels;
using Prism.Regions;

namespace StockFolio
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App{
		private Mutex mutex = new Mutex(false, "HalationGostStockFolio");
		protected override Window CreateShell() {
			var shell = Container.Resolve<Main>();
			return shell;
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			containerRegistry.RegisterSingleton<AssetTransitionViewModel>();
			containerRegistry.RegisterSingleton<PortfolioHoldingsViewModel>();
			containerRegistry.RegisterSingleton<ImportAndAdjustmentViewModel>();
			containerRegistry.RegisterSingleton<TradeHistoryViewModel>();
			containerRegistry.RegisterSingleton<TradeAnalysisViewModel>();
			containerRegistry.RegisterSingleton<MainViewModel>();
			containerRegistry.RegisterSingleton<AdjustmentViewModel>();
			containerRegistry.RegisterSingleton<ImportViewModel>();

			//var regionManager = Container.Resolve<IRegionManager>();
			//regionManager.RegisterViewWithRegion("TopicsRegion", typeof(AssetTransition));
   //         regionManager.RegisterViewWithRegion("TopicsRegion", typeof(ImportAndAdjustment));
            //IRegion region= regionManager.Regions["TopicsRegion"];
            //region.Add(Container.Resolve<ImportAndAdjustment>());
        }
		private  void PrismApplication_Startup(object sender,StartupEventArgs e) {
			if (this.mutex.WaitOne(0, false)) return;
			MessageBox.Show("二重起動できません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
			this.mutex.Close();
			this.mutex = null;
			this.Shutdown();
		}

		private void PrismApplication_Exit(object sender, ExitEventArgs e) {
			if (this.mutex != null) {
				this.mutex.ReleaseMutex();
				this.mutex.Close();
			}
		}
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
            LiveCharts.Configure(config => {
				config.AddDarkTheme();
				config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('あ'));
				//config.HasMap<string>((s, point) => { point.Coordinate = new LiveChartsCore.Kernel.Coordinate(point.Index, s.Length); }) ;
			});
		}
	}
}
