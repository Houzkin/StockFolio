using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortFolion.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Prism.Mvvm;
using Houzkin.Architecture;
using System.Reactive.Disposables;
using Prism.Commands;
using System.Windows.Input;

namespace StockFolio.ViewModels {
    public class AccountContainer : BasketContainer {
        public AccountContainer(CommonNode model) : base(model) {
            Disposables.Add(Disposable.Create(() => {
                _stockPosi?.Dispose();
                _productPosi?.Dispose();
                _fxPosi?.Dispose();
            }));
            
        }
        ElementEditer? _stockPosi;
        ICommand? _addStockCommand;
        public ElementEditer StockPosition {
            get {
                if (_stockPosi == null) StockPosition = new StockPositionCreater(Model); 
                return _stockPosi!;
            }
            set {
                if (_stockPosi != value) {
                    _stockPosi?.Dispose();
                    _stockPosi = value;
                    this.RaisePropertyChanged(nameof(StockPosition));
                }
            }
        }
        public ICommand AddStockPosition {
            get => _addStockCommand ??= new DelegateCommand(() => {
                this.StockPosition.Apply();
                this.StockPosition = new StockPositionCreater(Model);
            },
                () => this.StockPosition.CanApply.Value)
            .ObservesProperty(() => StockPosition.CanApply.Value);
        }

        ElementEditer? _productPosi;
        ICommand? _addProductCommand;
        public ElementEditer ProductPosition {
            get {
                if (_productPosi == null) { ProductPosition = new ProductPositionCreater(Model); }
                return _productPosi!;
            }
            set {
                if (_productPosi != value) {
                    _productPosi?.Dispose();
                    _productPosi = value;
                    this.RaisePropertyChanged(nameof(ProductPosition));
                }
            }
        }
        public ICommand AddProductPosition {
            get => _addProductCommand ??= new DelegateCommand(() => {
                this.ProductPosition.Apply();
                this.ProductPosition = new ProductPositionCreater(Model);
            }, () => this.ProductPosition.CanApply.Value)
                .ObservesProperty(() => ProductPosition.CanApply.Value);
        }

        ElementEditer? _fxPosi;
        ICommand? _addFxCommand;
        public ElementEditer FxPosition {
            get {
                if (_fxPosi == null) { FxPosition = new FxPositionCreater(Model); }
                return _fxPosi!;
            }
            set {
                if (_fxPosi != value) {
                    _fxPosi?.Dispose();
                    _fxPosi = value;
                    this.RaisePropertyChanged(nameof(FxPosition));
                }
            }
        }
        public ICommand AddFxPosition {
            get => _addFxCommand ??= new DelegateCommand(() => {
                this.FxPosition.Apply();
                this.FxPosition = new FxPositionCreater(Model);
            }, () => this.FxPosition.CanApply.Value)
                .ObservesProperty(() => FxPosition.CanApply.Value);
        }
    }
    public class BrokerContainer : BasketContainer {
        public BrokerContainer(CommonNode model) : base(model) {
            Disposables.Add(Disposable.Create(() => _account?.Dispose()));
        }
        ElementEditer? _account;
        ICommand? _addAccountCommand;
        public ElementEditer Account {
            get {
                if (_account == null) this.Account = new AccountCreater(Model);
                return this.Account;
            }
            set {
                if(_account != value) {
                    _account?.Dispose();
                    _account = value;
                    this.RaisePropertyChanged(nameof(Account));
                }
            }
        }
        public ICommand AddAccount {
            get => _addAccountCommand ??= new DelegateCommand(() => {
                this.Account.Apply();
                this.Account = new AccountCreater(Model);
            },()=>this.Account.CanApply.Value)
                .ObservesProperty(()=> Account.CanApply.Value);
        }
    }
}
