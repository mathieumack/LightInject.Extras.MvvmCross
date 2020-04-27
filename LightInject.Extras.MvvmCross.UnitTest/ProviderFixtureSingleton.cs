using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Base;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ProviderFixtureSingleton : IDisposable
    {
        [TestInitialize]
        public void BeforeEachMethod()
        {
            MvxSingleton.ClearAllSingletons();
        }

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private interface IHasDependentProperty
        {
            IInterface1 Dependency { get; set; }

            IInterface2 MarkedDependency { get; set; }
        }

        private interface IInterface
        {
        }

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
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            var provider = this.CreateProvider();
            provider.RegisterSingleton(typeof(object), () => new object());
            Assert.IsInstanceOfType(provider.GetSingleton<object>(), typeof(object));
            Assert.AreSame(provider.GetSingleton<object>(), provider.GetSingleton<object>());
        }

        [TestMethod]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.GetSingleton(null));
        }

        [TestMethod]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ComponentNotRegisteredException>(() => provider.GetSingleton<object>());
        }

        [TestMethod]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(object), () => new object());
            Assert.ThrowsException<DependencyResolutionException>(() => provider.GetSingleton<object>());
        }

        [TestMethod]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton((IInterface)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton((Func<IInterface>)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(null, new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(typeof(object), null));
        }

        [TestMethod]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterfaceWithFisrtResolve()
        {
            var provider = this.CreateProvider();
            var concreteViaFunc = new Concrete();
            provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.AreEqual(concreteViaFunc, provider.Resolve<IInterface>());
            Assert.AreSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());

            var concreteInstance = new Concrete();
            provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.AreNotEqual(concreteInstance, provider.Resolve<IInterface>());
            Assert.AreEqual(concreteViaFunc, provider.Resolve<IInterface>());
            Assert.AreSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var provider = this.CreateProvider();
            var concreteViaFunc = new Concrete();
            provider.RegisterSingleton<IInterface>(() => concreteViaFunc);

            var concreteInstance = new Concrete();
            provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.AreEqual(concreteInstance, provider.Resolve<IInterface>());
            Assert.AreSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        private LightInjectIocProvider CreateProvider(MvxPropertyInjectorOptions options = null)
        {
            var container = new ServiceContainer();
            var provider = new LightInjectIocProvider(container, options);
            this._disposables.Add(provider);
            return provider;
        }

        private class Concrete : IInterface
        {
            public Exception PropertyToInject { get; set; }

            public Exception PropertyToSkip { get; set; }
        }

        private class Concrete1 : IInterface1
        {
        }

        private class Concrete2 : IInterface2
        {
        }

        private class HasDependantProperty : IHasDependentProperty
        {
            public IInterface1 Dependency { get; set; }

            [Inject]
            public IInterface2 MarkedDependency { get; set; }
        }
    }
}
