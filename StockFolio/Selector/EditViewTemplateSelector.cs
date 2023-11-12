using StockFolio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PortFolion.Core;
using PortFolion.IO;

namespace StockFolio.Views {
    public class EditViewTemplateSelector : DataTemplateSelector {
        
        public DataTemplate? PositionEditView { get; set; }
        public DataTemplate? CashPositionEditView { get; set; }
        public DataTemplate? AccountEditView { get; set; }
        public DataTemplate? BrokerEditView { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {

            var vm = item as TreeViewContainer;
            if (vm == null) return null;
            var ev = vm.Model.GetNodeType() switch {
                NodeType.Total => BrokerEditView,
                NodeType.Broker => BrokerEditView,
                NodeType.Account => AccountEditView,
                NodeType.Cash => CashPositionEditView,
                NodeType.Stock => PositionEditView,
                NodeType.OtherProduct => PositionEditView,
                NodeType.Forex => PositionEditView,
                _ => null
            };
            return ev;
            //return base.SelectTemplate(item, container);
        }
    }
    public class AddEditViewTemplateSelector : DataTemplateSelector {
        public DataTemplate? GeneralOrCreditAccountTemplate { get; set; }
        public DataTemplate? ForexAccountTemplate { get; set; }
        public DataTemplate? BrokerTemplate { get; set; }
        public DataTemplate? TotalNodeTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var vm = item as TreeViewContainer;
            DataTemplate? returnobj = null;
            if (vm == null || vm.Model is FinancialValue) returnobj = null;
            else if(vm.Model.GetNodeType() == NodeType.Account) {
                returnobj = ((AccountNode)vm.Model).Account switch {
                    AccountClass.General => GeneralOrCreditAccountTemplate,
                    AccountClass.Credit => GeneralOrCreditAccountTemplate,
                    AccountClass.FX => ForexAccountTemplate,
                    AccountClass.None => null,
                    _ => null
                };
            } else if(vm.Model.GetNodeType() == NodeType.Broker) {
                returnobj = BrokerTemplate;
            } else if(vm.Model.GetNodeType() == NodeType.Total) {
                returnobj= TotalNodeTemplate;
            }
            return returnobj ?? new DataTemplate();
        }
    }
}
