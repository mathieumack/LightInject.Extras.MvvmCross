using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.IoC;

namespace LightInject.Extras.MvvmCross
{
    public class LightInjectIocProvider : IMvxIoCProvider, IDisposable
    {
        private readonly IServiceContainer container;
        private readonly MvxPropertyInjectorOptions options;

        readonly Dictionary<Type, Action> callbackRegisters;
        private readonly List<Type> singletons;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        public LightInjectIocProvider(IServiceContainer container, MvxPropertyInjectorOptions options = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            this.options = options ?? new MvxPropertyInjectorOptions();
            this.container = new ServiceContainer();
            this.callbackRegisters = new Dictionary<Type, Action>();
            singletons = new List<Type>();
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

            var item = container.GetAllInstances(type);
            return item != null && item.Any(e => e.GetType().Equals(type));
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

        public IMvxIoCProvider CreateChildContainer()
        {
            return new LightInjectIocProvider(container, options);
        }

        public void Dispose()
        {
            container.Dispose();
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
            if(item != null && item.Any() && !singletons.Contains(type))
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
            return container.Create(type);
        }

        public T IoCConstruct<T>(IDictionary<string, object> arguments) where T : class
        {
            return (T)container.GetInstance(typeof(T), arguments.Values.ToArray());
        }

        public T IoCConstruct<T>(object arguments) where T : class
        {
            return (T)container.GetInstance(typeof(T), new object[] { arguments });
        }

        public T IoCConstruct<T>(params object[] arguments) where T : class
        {
            return (T)container.GetInstance(typeof(T), arguments);
        }

        public object IoCConstruct(Type type, IDictionary<string, object> arguments)
        {
            return container.GetInstance(type, arguments.Values.ToArray());
        }

        public object IoCConstruct(Type type, object arguments)
        {
            throw new NotImplementedException();
        }

        public object IoCConstruct(Type type, params object[] arguments)
        {
            throw new NotImplementedException();
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
            
            if (!singletons.Contains(tInterface))
                singletons.Add(tInterface);

            container.RegisterInstance(tInterface, theObject);
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

            if (!singletons.Contains(tInterface))
                singletons.Add(tInterface);

            container.RegisterInstance(tInterface, theConstructor());
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
            
            container.Register<TInterface>(factory => constructor());
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

            container.Register(t, factory => constructor());
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

            container.Register(tFrom, tTo);
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

            var item = container.GetInstance(type);
            //if (!item.GetType().Equals(type))
            //    throw new ComponentNotRegisteredException();

            return item;
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

            var item = container.TryGetInstance(type);
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