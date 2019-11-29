using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Base;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTest
{
    [TestClass]
    public class ProviderFixtureCallbacks : IDisposable
    {
        [TestInitialize]
        public void BeforeEachMethod()
        {
            MvxSingleton.ClearAllSingletons();
        }

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
            public Exception PropertyToInject { get; set; }

            public Exception PropertyToSkip { get; set; }
        }

        public void Dispose()
        {
            foreach (var disposable in this._disposables)
            {
                disposable.Dispose();
            }

            this._disposables.Clear();
        }

        [TestMethod]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            var provider = this.CreateProvider();
            provider.CallbackWhenRegistered<IInterface>(() => called = true);
            provider.RegisterType<IInterface, Concrete>();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.CallbackWhenRegistered(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.CallbackWhenRegistered(typeof(object), null));
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
