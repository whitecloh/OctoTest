using System;
using System.Collections.Generic;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class ServiceRegistry : IDisposable
    {
        private readonly Dictionary<Type, object> _services = new ();

        public void Bind<TService>(TService service) where TService : class
        {
            _services[typeof(TService)] = service ?? throw new ArgumentNullException(nameof(service));
        }

        public TService Resolve<TService>() where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out object service))
            {
                return (TService)service;
            }

            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered.");
        }

        public void Dispose()
        {
            HashSet<object> disposed = new HashSet<object>();
            foreach (object service in _services.Values)
            {
                if (service is IDisposable disposable && disposed.Add(service))
                {
                    disposable.Dispose();
                }
            }

            _services.Clear();
        }
    }
}
