using System;
using LightInject.Extras.MvvmCross.Pcl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightInject.Extras.MvvmCross.UnitTests
{
    [TestClass]
    public class LightInjectMvxIocProviderFixture : IDisposable
    {
        IServiceContainer container;

        LightInjectIocProvider provider;
        
        [TestInitialize]
        public void BeforeEachMethod()
        {
            container = new ServiceContainer();
            provider = new LightInjectIocProvider(container);
        }

        [TestMethod]
        public void CanResolveReturnsTrueWhenMatchingTypeIsRegistered()
        {
            container.Register(c => new object());

            Assert.IsTrue(provider.CanResolve<object>());
        }

        [TestMethod]
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            Assert.IsFalse(provider.CanResolve<object>());
        }

        [TestMethod]
        public void CanResolveThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.CanResolve(null));
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructReturnsRegisteredType()
        {
            container.Register(c => new object());

            Assert.IsTrue(provider.Resolve<object>() is object);
            Assert.IsTrue(provider.Create<object>() is object);
            Assert.IsTrue(provider.IoCConstruct<object>() is object);
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.ThrowsException<ComponentNotRegisteredException>(() => provider.Resolve<object>());
            Assert.ThrowsException<ComponentNotRegisteredException>(() => provider.Create<object>());
            Assert.ThrowsException<ComponentNotRegisteredException>(() => provider.IoCConstruct<object>());
        }

        [TestMethod]
        public void ResolveCreateAndIoCConstructThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.Resolve(null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.Create(null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.IoCConstruct(null));
        }

        [TestMethod]
        public void GetSingletonReturnsSingletonIfTypeRegisteredAsSingleton()
        {
            container.RegisterInstance(new object());

            Assert.IsTrue(provider.GetSingleton<object>() is object);
            Assert.AreSame(provider.GetSingleton<object>(), provider.GetSingleton<object>());
        }

        [TestMethod]
        public void GetSingletonThrowsDependencyResolutionExceptionIfTypeRegisteredButNotAsSingleton()
        {
            container.Register(c => new object());

            Assert.ThrowsException<DependencyResolutionException>(() => provider.GetSingleton<object>());
        }

        [TestMethod]
        public void GetSingletonThrowsComponentNotRegisteredExceptionWhenNoTypeRegistered()
        {
            Assert.ThrowsException<ComponentNotRegisteredException>(() => provider.GetSingleton<object>());
        }

        [TestMethod]
        public void GetSingletonThrowsArgumentNullExceptionWhenCalledWithNoTypeArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.GetSingleton(null));
        }

        [TestMethod]
        public void TryResolveResolvesOutParameterWhenMatchingTypeRegistered()
        {
            container.Register(c => new object());

            object foo;
            var success = provider.TryResolve(out foo);

            Assert.IsTrue(foo is object);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void RegisterTypeRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType<IInterface, Concrete>();
            var instance = provider.Resolve<IInterface>();
            Assert.IsTrue(instance is Concrete);
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterTypeWithDelegateRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType<IInterface>(() => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsTrue(instance is Concrete);
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());

            provider.RegisterType(typeof(IInterface), () => new Concrete());
            Assert.AreNotSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterTypeWithDelegateAndTypeParameterRegistersConcreteTypeAgainstInterface()
        {
            provider.RegisterType(typeof(IInterface), () => new Concrete());
            var instance = provider.Resolve<IInterface>();
            Assert.IsTrue(instance is Concrete);
            Assert.AreNotSame(instance, provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoFromOrToTypeArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(null, typeof(object)));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Type)null));
        }

        [TestMethod]
        public void RegisterTypeThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType((Func<object>)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterType(typeof(object), (Func<object>)null));
        }

        [TestMethod]
        public void RegisterSingletonRegistersConcreteTypeAsSingletonAgainstInterface()
        {
            var concreteViaFunc = new Concrete();
            provider.RegisterSingleton<IInterface>(() => concreteViaFunc);
            Assert.AreEqual(concreteViaFunc, provider.Resolve<IInterface>());
            Assert.AreSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());

            var concreteInstance = new Concrete();
            provider.RegisterSingleton<IInterface>(concreteInstance);
            Assert.AreEqual(concreteInstance, provider.Resolve<IInterface>());
            Assert.AreSame(provider.Resolve<IInterface>(), provider.Resolve<IInterface>());
        }

        [TestMethod]
        public void RegisterSingletoneThrowsArgumentNullExceptionWhenCalledWithNoTypeInstanceOrConstructorArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton((IInterface)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton((Func<IInterface>)null));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(null, new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.RegisterSingleton(typeof(object), null));
        }

        [TestMethod]
        public void CallbackWhenRegisteredFiresSuccessfully()
        {
            var called = false;
            provider.CallbackWhenRegistered<IInterface>(() => called = true);

            provider.RegisterType<IInterface, Concrete>();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void CallbackWhenRegisteredThrowsArgumentNullExceptionWhenCalledWithNoTypeOrActionArgument()
        {
            Assert.ThrowsException<ArgumentNullException>(() => provider.CallbackWhenRegistered(null, () => new object()));
            Assert.ThrowsException<ArgumentNullException>(() => provider.CallbackWhenRegistered(typeof(object), null));
        }

        public void Dispose()
        {
            provider.Dispose();
        }

        private interface IInterface
        {
        }

        private class Concrete : IInterface
        {
        }
    }

}
