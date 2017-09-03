using System;
using System.Collections.Generic;
using System.Linq;
using LightInject.Extras.MvvmCross.Pcl;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.IoC;

namespace LightInject.Extras.MvvmCross
{
    public class LightInjectIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        private const string serviceName = "LightInjectServiceName";

        readonly IServiceContainer container;

        readonly Dictionary<Type, Action> callbackRegisters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        public LightInjectIocProvider(IServiceContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            this.container = new ServiceContainer();
            this.callbackRegisters = new Dictionary<Type, Action>();
        }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </typeparam>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public void CallbackWhenRegistered<T>(Action action)
        {
            CallbackWhenRegistered(typeof(T), action);
        }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </param>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public void CallbackWhenRegistered(Type type, Action action)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (action == null)
                throw new ArgumentNullException("action");

            if (!callbackRegisters.ContainsKey(type))
                callbackRegisters.Add(type, action);
            else
                callbackRegisters[type] = action;
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </typeparam>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the type <typeparamref name="T"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the type is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public bool CanResolve<T>() where T : class
        {
            return CanResolve(typeof(T));
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the <paramref name="type"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the <paramref name="type"/> is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public bool CanResolve(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return container.CanGetInstance(type, serviceName);
            //return item != null && item.Any(e => e.GetType().Equals(type));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T Create<T>() where T : class
        {
            return (T)Create(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object Create(Type type)
        {
            return Resolve(type);
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved singleton instance of type <typeparamref name="T"/>.
        /// </returns>
        public T GetSingleton<T>() where T : class
        {
            return (T)GetSingleton(typeof(T));
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved singleton instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DependencyResolutionException">
        /// Thrown if the <paramref name="type"/> is not registered as a singleton.
        /// </exception>
        public object GetSingleton(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var item = container.GetAllInstances(type);
            if(item != null && item.Count() > 1)
                // Ensure the dependency is registered as a singleton WITHOUT resolving the dependency twice.
                throw new DependencyResolutionException(type.Name + "is registered more than once time.");
            else if (item != null && item.Count(e => e.GetType().Equals(type)) == 1)
                return item.First(e => e.GetType().Equals(type));
            else
                throw new ComponentNotRegisteredException(type.Name + " not registered");
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T IoCConstruct<T>() where T : class
        {
            return (T)IoCConstruct(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object IoCConstruct(Type type)
        {
            return Resolve(type);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
        {
            RegisterSingleton(typeof(TInterface), theObject);
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            RegisterSingleton(typeof(TInterface), theConstructor);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton(Type tInterface, object theObject)
        {
            if (tInterface == null)
                throw new ArgumentNullException("tInterface");
            if (theObject == null)
                throw new ArgumentNullException("theObject");

            container.RegisterInstance(tInterface, theObject, serviceName);
            if (callbackRegisters.ContainsKey(tInterface))
                callbackRegisters[tInterface]();
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            if (tInterface == null)
                throw new ArgumentNullException("tInterface");
            if (theConstructor == null)
                throw new ArgumentNullException("theConstructor");

            container.RegisterInstance(tInterface, theConstructor(), serviceName);
            if (callbackRegisters.ContainsKey(tInterface))
                callbackRegisters[tInterface]();
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <typeparam name="TFrom">
        /// The component type that implements the service to register.
        /// </typeparam>
        /// <typeparam name="TTo">
        /// The service type that will be resolved from the container.
        /// </typeparam>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <typeparamref name="TFrom"/> to its own implementing
        /// type as well as to the service type <typeparamref name="TTo"/>.
        /// </para>
        /// </remarks>
        public void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom
        {
            RegisterType(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");
            
            var callbackRegisters = container.Register<TInterface>((type, service) => { return constructor(); });
            if (callbackRegisters.ContainsKey(typeof(TInterface)))
                callbackRegisters[typeof(TInterface)]();
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <param name="t">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="t"/> or <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterType(Type t, Func<object> constructor)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            container.RegisterInstance(t, constructor(), serviceName);
            if (callbackRegisters.ContainsKey(t))
                callbackRegisters[t]();
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <param name="tFrom">
        /// The component type that implements the service to register.
        /// </param>
        /// <param name="tTo">
        /// The service type that will be resolved from the container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tFrom"/> or <paramref name="tTo"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <paramref name="tFrom"/> to its own implementing
        /// type as well as to the service type <paramref name="tTo"/>.
        /// </para>
        /// </remarks>
        public void RegisterType(Type tFrom, Type tTo)
        {
            if (tFrom == null)
                throw new ArgumentNullException("tFrom");
            if (tTo == null)
                throw new ArgumentNullException("tTo");

            container.Register(tFrom, tTo, serviceName);
            if (callbackRegisters.ContainsKey(tFrom))
                callbackRegisters[tFrom]();
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object Resolve(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return container.GetInstance(type, serviceName);
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The service <see cref="System.Type"/> to resolve.
        /// </typeparam>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or default(T) if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public bool TryResolve<T>(out T resolved) where T : class
        {
            var item = container.TryGetInstance(typeof(T));
            if (item != null && item is T)
            {
                resolved = item as T;
                return true;
            }

            resolved = null;
            return false;
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <param name="type">
        /// The service <see cref="System.Type"/> to resolve.
        /// </param>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or <see langword="null"/> if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public bool TryResolve(Type type, out object resolved)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var item = container.TryGetInstance(type, serviceName);
            if (item != null)
            {
                resolved = item;
                return true;
            }

            resolved = null;
            return false;
        }
    }
}