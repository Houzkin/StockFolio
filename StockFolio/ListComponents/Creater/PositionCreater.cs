using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels{
    public class ProductPositionCreater: BasketCreater {
        public ProductPositionCreater(CommonNode node) : base(node) {

        }
        protected override CommonNode GenerateNode() {
            return new FinancialProduct();
        }
    }
    public class StockPositionCreater : ProductPositionCreater {
        public StockPositionCreater(CommonNode node) : base(node) {

            this.Code = stockPosition.ObserveProperty(a => a.Code)
                .ToReactiveProperty()
                .SetValidateNotifyError(x => Model.Children.OfType<StockValue>().Any(a => a.Code == x) ? "重複があります" : null)
                .AddTo(Disposables);
            this.Code.ObserveHasErrors.Subscribe(hasErr => this.ErrorChanged(hasErr, nameof(Code)));
            this.OrderReserver.Add(new EditPresenter<int>(nameof(Code),
                () => this.Code.Value,
                () => stockPosition.Code,
                a => stockPosition.Code = a));
        }
        private StockValue stockPosition {
            get => (this.NewNode as StockValue)!;
        }
        protected override CommonNode GenerateNode() {
            return new StockValue();
        }
        public ReactiveProperty<int> Code { get; }
        public override void Reset() {
            base.Reset();
            this.Code.Value = 0;
        }
    }
    public class FxPositionCreater : ProductPositionCreater {
        public FxPositionCreater(CommonNode node) : base(node) {
            this.Pair = this.fxPosition.ObserveProperty(a => a.Pair)
                .ToReactiveProperty<string>().AddTo(Disposables);
            this.OrderReserver.Add(new EditPresenter<string>(nameof(Pair),
                ()=> this.Pair.Value,
                ()=>fxPosition.Pair,
                a=> fxPosition.Pair = a));
        }
        private ForexValue fxPosition {
            get => (this.NewNode as ForexValue)!; 
        }
        protected override CommonNode GenerateNode() {
            return new ForexValue();
        }
        public ReactiveProperty<string> Pair { get; }
    }
}
