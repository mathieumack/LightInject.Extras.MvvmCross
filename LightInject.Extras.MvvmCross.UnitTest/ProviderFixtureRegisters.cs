using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Base;
using MvvmCross.Exceptions;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ProviderFixtureRegisters : IDisposable
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
        public void CanCreateChildContainers()
        {
            // Arrange
            var rootContainer = this.CreateProvider();
            rootContainer.RegisterType<IInterface, Concrete>();

            // Act
            var childContainer = rootContainer.CreateChildContainer();
            childContainer.RegisterType<IInterface1, Concrete1>();

            // Assert
            Assert.IsTrue(childContainer.CanResolve<IInterface>());
            Assert.IsTrue(childContainer.CanResolve<IInterface1>());
            Assert.IsTrue(rootContainer.CanResolve<IInterface>());
            Assert.IsFalse(rootContainer.CanResolve<IInterface1>());
        }
        
        [TestMethod]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType<IInterface, Concrete>();
            var instance = provider.Resolve<IInterface>();
            Assert.IsInstanceOfType(instance, typeof(Concrete));
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(null, typeof(object)));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Type)null));
        }

        [TestMethod]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            var provider = this.CreateProvider();
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType((Func<object>)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Func<object>)null));
        }

        [TestMethod]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsInstanceOfType(instance, typeof(Concrete));
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            var provider = this.CreateProvider();
            provider.RegisterType<IInterface>(() => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsInstanceOfType(instance, typeof(Concrete));
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());
            provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.AreNotSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
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

        private class Concrete : IInterface
        {
            public Exception PropertyToInject { get; set; }

            public Exception PropertyToSkip { get; set; }
        }

        private class Concrete1 : IInterface1
        {
        }
    }
}
