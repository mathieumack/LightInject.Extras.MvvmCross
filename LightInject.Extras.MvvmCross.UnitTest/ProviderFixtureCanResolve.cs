using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Base;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTest
{
    [TestClass]
    public class ProviderFixtureCanResolve : IDisposable
    {
        [TestInitialize]
        public void BeforeEachMethod()
        {
            MvxSingleton.ClearAllSingletons();
        }

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private interface IInterface1
        {
        }

        private interface IInterface2
        {
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
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            var provider = this.CreateProvider();
            Assert.IsFalse(provider.CanResolve<object>());
        }

        [TestMethod]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(object), () => new object());

            object foo;
            var success = provider.TryResolve(out foo);

            Assert.IsInstanceOfType(foo, typeof(object));
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(object));
            Assert.IsTrue(provider.CanResolve<object>());
        }

        [TestMethod]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.CanResolve(null));
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
