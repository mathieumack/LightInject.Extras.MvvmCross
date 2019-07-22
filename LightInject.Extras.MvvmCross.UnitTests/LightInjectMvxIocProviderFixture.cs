using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Exceptions;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross.UnitTests
{
    [TestClass]
    public class LightInjectMvxIocProviderFixture : IDisposable
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
        public void CanResolveReturnsFalseWhenNoMatchingTypeIsRegistered()
        {
            var provider = this.CreateProvider();
            Assert.IsFalse(provider.CanResolve<object>());
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
        public void IfSetInOptions_OnNonResolvableProperty_Throws()
        {
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                this.CreateProvider(options: new MvxPropertyInjectorOptions()
                {
                    ThrowIfPropertyInjectionFails = true,
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });
            });
        }

        [TestMethod]
        public void IgnoresNonResolvableProperty()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled()
        {
            // Arrange
            var provider = CreateProvider(new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void InjectsOnlyMarkedProperties_WithCustomAttribute_IfEnabled_Lazy()
        {
            // Arrange
            var provider = CreateProvider(new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();
            Mvx.IoCProvider.RegisterSingleton<IHasDependentProperty>(Mvx.IoCProvider.IoCConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.IoCProvider.Resolve<IHasDependentProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void InjectsOnlyMarkedPropertiesIfEnabled()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void InjectsOnlyMarkedPropertiesIfEnabled_Lazy()
        {
            // Arrange
            var provider = this.CreateProvider(options:
                new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties,
                });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();
            Mvx.IoCProvider.RegisterSingleton<IHasDependentProperty>(Mvx.IoCProvider.IoCConstruct<HasDependantProperty>);

            // Act
            var obj = Mvx.IoCProvider.Resolve<IHasDependentProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void InjectsPropertiesIfEnabled()
        {
            // Arrange
            var provider = this.CreateProvider(new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            Mvx.IoCProvider.RegisterType<IInterface1, Concrete1>();
            Mvx.IoCProvider.RegisterType<IInterface2, Concrete2>();

            // Act
            var obj = Mvx.IoCProvider.IoCConstruct<HasDependantProperty>();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Dependency);
            Assert.IsNotNull(obj.MarkedDependency);
        }

        [TestMethod]
        public void PropertyInjectionCanBeCustomized()
        {
            var provider = CreateProvider(new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            this._disposables.Add(provider);
            provider.RegisterType(() => new Concrete());
            provider.RegisterType(typeof(Exception), () => new DivideByZeroException());
            var resolved = provider.Resolve<Concrete>();

            Assert.IsInstanceOfType(resolved.PropertyToInject, typeof(DivideByZeroException));
            Assert.IsNull(resolved.PropertyToSkip);
        }

        [TestMethod]
        public void PropertyInjectionCanBeEnabled()
        {
            var provider = CreateProvider(new MvxPropertyInjectorOptions() { InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties });
            this._disposables.Add(provider);
            provider.RegisterType(() => new Concrete());
            provider.RegisterType(typeof(Exception), () => new DivideByZeroException());
            var resolved = provider.Resolve<Concrete>();

            // Default behavior is to inject all unset properties.
            Assert.IsInstanceOfType(resolved.PropertyToInject, typeof(DivideByZeroException));
            Assert.IsInstanceOfType(resolved.PropertyToSkip, typeof(DivideByZeroException));
        }

        [TestMethod]
        public void PropertyInjectionOffByDefault()
        {
            var provider = this.CreateProvider();
            provider.RegisterType(() => new Concrete());
            var resolved = provider.Resolve<Concrete>();
            Assert.IsNull(resolved.PropertyToInject);
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

        private class CustomAttributeSelector : IPropertySelector
        {
            private readonly Type _customAttributeType;

            public CustomAttributeSelector(Type customAttributeType)
            {
                if (customAttributeType == null) throw new ArgumentNullException(nameof(customAttributeType));
                this._customAttributeType = customAttributeType;
            }

            public IEnumerable<PropertyInfo> Execute(Type type)
            {
                throw new NotImplementedException();
            }

            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                return propertyInfo.GetCustomAttributes(this._customAttributeType).Any();
            }
        }

        private class HasDependantProperty : IHasDependentProperty
        {
            [MyInjection]
            public IInterface1 Dependency { get; set; }

            [MvxInject]
            public IInterface2 MarkedDependency { get; set; }
        }

        private class MyInjectionAttribute : Attribute
        {
        }
    }
}
