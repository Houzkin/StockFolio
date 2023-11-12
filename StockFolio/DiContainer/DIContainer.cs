using DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFolio.DiContainer {
    public sealed class DIContainer {
        
        public static IContainerExtension Container => ContainerLocator.Current;
    }
}
