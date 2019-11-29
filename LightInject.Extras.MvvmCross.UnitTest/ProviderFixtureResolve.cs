using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Base;
using MvvmCross.Exceptions;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTest
{
    [TestClass]
    public class ProviderFixtureResolve : IDisposable
    {
        [TestInitialize]
        public void BeforeEachMethod()
        {
            MvxSingleton.ClearAllSingletons();
        }

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var disposable in this._disposables)
            {
                disposable.Dispose();
            }

            this._disposables.Clear();
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(object), () => new object());

            Assert.IsInstanceOfType(provider.Resolve<object>(), typeof(object));
            Assert.IsInstanceOfType(provider.Create<object>(), typeof(object));
            Assert.IsInstanceOfType(provider.IoCConstruct<object>(), typeof(object));
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.Resolve(null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.Create(null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.IoCConstruct(null));
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<MvxIoCResolveException>(() => provider.Resolve<object>());
            Assert.ThrowsException<MvxIoCResolveException>(() => provider.Create<object>());
            Assert.IsNotNull(provider.IoCConstruct<object>());
        }

        private LightInjectIocProvider CreateProvider(MvxPropertyInjectorOptions options = null)
        {
            var container = new ServiceContainer();
            var provider = new LightInjectIocProvider(container, options);
            this._disposables.Add(provider);
            return provider;
        }
    }
}
