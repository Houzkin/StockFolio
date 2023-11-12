using Houzkin.Architecture;
using Prism.Commands;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StockFolio.DiContainer;
using StockFolio.ExtractData;
using StockFolio.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		static IContainerExtension extension;
		

        public UpdateSwitcher compositeAttachDettach { get; } = UpdateSwitcher.Switcher;
		public ReactiveCommand<AttachOrderArgs> TabSelectionCommand { get; } = new ReactiveCommand<AttachOrderArgs>();
		public DelegateCommand ReloadData { get; }
		public DelegateCommand ShowImputDataLayar { get; }

		public DelegateCommand ClosedCommand { get; }

		public MainViewModel(IContainerExtension containerExtension) {
			extension = containerExtension;
			
			//UpdateSwitcher.Switcher.AttachOnly(extension.Resolve<AssetTransitionViewModel>());
			this.ReloadData = new DelegateCommand(
				() => { /* データ再読み込みコマンドの処理 */
					compositeAttachDettach.Refresh();
				});
			this.ShowImputDataLayar = new DelegateCommand(
				() => { /* インプットレイヤーの表示処理 */ });

			//this.TabSelectionCommand.Subscribe(a => {
			//	if (a.OriginSourceDataContext != this) return;
			//	compositeAttachDettach.OnAndOff(a.ActiveDataContext, a.DeactiveDataContext);
			//});
			this.ClosedCommand = new DelegateCommand(() => this.compositeAttachDettach.Dispose());
		}
		
		
	}
	public abstract class TopicViewModel : ViewModelBase {
		bool _isActive = false;
		public bool IsActive {
			get { return _isActive;}
			set { 
				if(_isActive == value) return;
				_isActive = value;
				OnPropertyChanged();
			}
		}
		protected TopicViewModel() {
			this.ObserveProperty(x => x.IsActive)
				.Subscribe(a => {
					if (a) 
						this.OnActived();
					else 
						this.OnDeactived();
				});
		}
		protected virtual void OnActived() {
			UpdateSwitcher.Switcher.AttachOnly(this);

			//Debug.Print(this.ToString() + " is Attached");
		}
		protected virtual void OnDeactived() {
            //Debug.Print(this.ToString() + " is Detached");
        }
	}
}
