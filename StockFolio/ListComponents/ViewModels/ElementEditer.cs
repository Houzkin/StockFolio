using Houzkin.Architecture;
using Livet.EventListeners;
using ObservableCollections;
using PortFolion.Core;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.ObjectExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.ViewModels {
	public static class Extensions {
		public class Observable<T> : IObservable<T> {
			Func<IObserver<T>, IDisposable> _subscribe;
			public Observable(Func<IObserver<T>, IDisposable> subscribe) => _subscribe = subscribe;
			public IDisposable Subscribe(IObserver<T> observer) => _subscribe(observer);
		}
		public static IObservable<NotifyCollectionChangedEventArgs> CollectionChangedAsObservable<T>(this IObservableCollection<T> source) {
			var obs = new Observable<NotifyCollectionChangedEventArgs>(observer => {
				NotifyCollectionChangedEventHandler<T> handler = (in NotifyCollectionChangedEventArgs<T> e)=> observer.OnNext(e.ToStandardEventArgs());
				source.CollectionChanged += handler;
				return Disposable.Create(() => source.CollectionChanged -= handler);
			});
			return obs;
		}
	}
	public abstract class ElementEditer :BindableObject<CommonNode> {
		protected ElementEditer(CommonNode node) : base(node) {

			EditOrders = OrderReserver.ToFilteredReadOnlyObservableCollection(x => x.HasOrder);

			HasEditOrder = EditOrders.CollectionChangedAsObservable().Select(_ => EditOrders.Any()).ToReadOnlyReactiveProperty();

			HasErrors = ErrorProperties.CollectionChangedAsObservable().Select(_ => ErrorProperties.Any()).ToReadOnlyReactiveProperty();

			ApplyCommand = new[] { HasEditOrder, HasErrors.Inverse(), }
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(() => {
					EditOrders.OrderBy(x=>x.Priority).ToArray().ForEach(
						a => ImportAndAdjustmentViewModel.UndoOrders.Push(a.Apply())
					);
					this.Reset();
				});
		}
		protected ObservableCollection<EditPresenterBase> OrderReserver { get; } = new();
		public IFilteredReadOnlyObservableCollection<EditPresenterBase> EditOrders { get; }
		public ReadOnlyReactiveProperty<bool> HasEditOrder { get; }
		protected void CheckChangedProperty(string propertyName) {
			this.OrderReserver
				.Where(a => a.ChargeNames.Contains(propertyName))
				.ForEach(a => a.MaybeOrderValueChanged());
		}
		private ObservableHashSet<string> ErrorProperties { get; } = new();
		public ReadOnlyReactiveProperty<bool> HasErrors { get; }
		protected void ErrorChanged(bool hasError,string propName) {
			if (hasError) ErrorProperties.Add(propName);
            else ErrorProperties.Remove(propName);
        }
		public ReactiveCommand ApplyCommand { get; }
		public abstract void Reset();

		protected CompositeDisposable Disposables = new CompositeDisposable();
		protected override void Dispose(bool disposing) {
			if(disposing) Disposables.Dispose();
			base.Dispose(disposing);
		}
	}
}
