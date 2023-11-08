using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {

    public abstract class BasketCreater : ElementEditer {
        public BasketCreater(CommonNode node) : base(node) {
            this.NewNode = GenerateNode();

            this.Name = NewNode.ObserveProperty(a => a.Name)
                .ToReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須" : null)
                .SetValidateNotifyError(x => !node.CanAdd(NewNode) ? "項目名が不正です" : null)
                .AddTo(Disposables);
            this.Name.ObserveHasErrors.Subscribe(hasErr => this.ErrorChanged(hasErr, nameof(Name)));
            this.OrderReserver.Add(new EditPresenter<string>(-2, nameof(Name),
                () => this.Name.Value, () => NewNode.Name, x => NewNode.Name = x));
            this.Name.Subscribe(_ => this.CheckChangedProperty(nameof(Name)));

            this.OrderReserver.Add(new AddNodePresenter(nameof(Name),
                NewNode,
                () => !string.IsNullOrEmpty(Name.Value) && node.CanAdd(Name.Value) && this.HasEditOrder.Value,
                () => node.AddChild(NewNode),
                () => node.RemoveChild(NewNode)));
        }
        protected abstract CommonNode GenerateNode();
        public CommonNode NewNode { get; private set; }

        public ReactiveProperty<string> Name { get; }
        public override void Reset() {
            this.Name.Value = "";
        }
    }
    public class AccountCreater : BasketCreater {
        public AccountCreater(CommonNode node) : base(node) { }
        protected override CommonNode GenerateNode() {
            return new AccountNode();
        }
    }
    public class BrokerCreater : BasketCreater {
        public BrokerCreater(CommonNode node) : base(node) { }
        protected override CommonNode GenerateNode() {
            return new BrokerNode();
        }
    }
}
