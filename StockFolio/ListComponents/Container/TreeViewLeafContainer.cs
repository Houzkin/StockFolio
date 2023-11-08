using Houzkin;
using Houzkin.Tree;
using PortFolion.Core;
using Prism.Modularity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockFolio.ViewModels {

    public class CashPositionContainer : BasketContainer {
        public CashPositionContainer(FinancialValue model) : base(model) {
            this.InvestmentValue = model.ObserveProperty(x => x.InvestmentValue).ToReadOnlyReactiveProperty().AddTo(Disposables);
        }
        protected override ElementEditer GenerateEditer() {
            return new CashPositionEditer(Model);
        }
        public ReadOnlyReactiveProperty<long> InvestmentValue { get; }

        /// <summary>全ての編集を許可する場合はtrue。追記のみを許可する場合はfalse。</summary>
        public ReactiveProperty<bool> Alteration { get; } = new ReactiveProperty<bool>();
    }
    public class PositionContainer : CashPositionContainer {
        public PositionContainer(FinancialProduct model) : base(model) {
            this.TradeQuantity = model.ObserveProperty(x => x.TradeQuantity).ToReadOnlyReactiveProperty().AddTo(Disposables);
            this.Quantity = model.ObserveProperty(x => x.Quantity).ToReadOnlyReactiveProperty().AddTo(Disposables);
            this.PerPrice = new[] { this.Amount, this.Quantity }
                .CombineLatest(x => x[1] != 0 ? (double)x[0] / x[1] : 0)
                .Select(x => string.Format("{0:#,#.##}", x))
                .ToReadOnlyReactiveProperty<string>().AddTo(Disposables);
        }
        protected override ElementEditer GenerateEditer() {
            return new ProductEditor(Model);
        }
        public ReadOnlyReactiveProperty<string> PerPrice { get; }
        public ReadOnlyReactiveProperty<long> TradeQuantity { get; }
        public ReadOnlyReactiveProperty<long> Quantity { get; }
        BasketContainer? subEditer;
        public BasketContainer? SubContainer {
            get => subEditer ??= GetAndSettingSubEditer();
        }
        /// <summary>キャッシュポジションの探索と設定を行う</summary>
        /// <returns>設定が完了したCashPositionContainer</returns>
        private CashPositionContainer GetAndSettingSubEditer() {
            var cp = this.Model.Siblings().Where(a => a is CashValue).FirstOrDefault();
            if (cp == null) cp = (this.Model as AccountNode)!.GetOrCreateNuetral();// return null;
            var cpe = (TreeViewContainer.Create(cp) as CashPositionContainer)!;
            Disposables.Add(cpe!);
            var posedi = this.Editer as ProductEditor;
            //ここでsubscribeを記述
            this.Alteration.Subscribe(a => cpe.Alteration.Value = a).AddTo(Disposables);
            return cpe;
        }
        protected override ICommand GenerateApplyCommand() {
            if (this.SubContainer is null) return base.GenerateCancelCommand();
            var rc = new[] {
                this.Editer.CanApply,this.SubContainer.Editer.CanApply
            }.CombineLatest(x => x[0] || x[1]).ToReactiveCommand().WithSubscribe(() => {
                this.Editer.Apply();
                this.SubContainer.Editer.Apply();
            }).AddTo(Disposables);
            return rc;
        }
        protected override ICommand GenerateCancelCommand() {
            return new ReactiveCommand().WithSubscribe(() => {
                this.Editer.Reset();
                this.SubContainer?.Editer.Reset();
            }).AddTo(Disposables);
        }
    }
}
