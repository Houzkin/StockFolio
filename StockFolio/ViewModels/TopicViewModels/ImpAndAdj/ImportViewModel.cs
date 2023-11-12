using Houzkin.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockFolio.ViewModels {
	public class ImportViewModel : ViewModelBase /*: TopicViewModel*/ {//
		//やっぱりTopicViewModelの継承はなし、IｓActive のバインドも解除予定
		//ImportVMからAssetTransitionVMのとき、ImportVMはtrueのまま
		//ImportAndAdjustmentViewModel _parent;
		public ImportViewModel() {
			//_parent = viewModel;
			Text = "aaaa";
		}
		string text;
		public string Text {
			get { return text; }
			set {
				if(text == value) return;
				text = value;
				this.OnPropertyChanged("Text");
			}
		}

    }
}
