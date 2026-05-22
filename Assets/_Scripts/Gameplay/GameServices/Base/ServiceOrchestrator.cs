using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Gameplay.Services
{
    public abstract class ServiceOrchestrator<T> : IInitializable, IStartable, ITickable, ILateTickable, IFixedTickable, IDisposable
        where T : Service
    {
        protected readonly Dictionary<Type, T> Services = new();

        public bool IsEnabled { get; private set; }

        protected ServiceOrchestrator(IEnumerable<T> services)
        {
            foreach (var service in services)
                Services.Add(service.GetType(), service);
        }

        public void EnableServices()
        {
            IsEnabled = true;
            foreach (var service in Services.Values)
                service.Enable();
        }

        public void DisableServices()
        {
            IsEnabled = false;
            foreach (var service in Services.Values)
                service.Disable();
        }

        protected void EnableService(Type type)
        {
            if (!Services.TryGetValue(type, out var service))
            {
                Debug.LogError($"{type} does not exist");
                return;
            }
            service.Enable();
        }

        protected void DisableService(Type type)
        {
            if (!Services.TryGetValue(type, out var service))
            {
                Debug.LogError($"{type} does not exist");
                return;
            }
            service.Disable();
        }

        public void Initialize()
        {
            foreach (var service in Services.Values)
                service.OnInitialize();
        }

        public void Start()
        {
            foreach (var service in Services.Values)
                service.OnStart();
        }

        public void Tick()
        {
            if (!IsEnabled) return;
            foreach (var service in Services.Values)
                if (service.IsEnabled) service.OnUpdate();
        }

        public void LateTick()
        {
            if (!IsEnabled) return;
            foreach (var service in Services.Values)
                if (service.IsEnabled) service.OnLateUpdate();
        }

        public void FixedTick()
        {
            if (!IsEnabled) return;
            foreach (var service in Services.Values)
                if (service.IsEnabled) service.OnFixedUpdate();
        }

        public void Dispose()
        {
            foreach (var service in Services.Values)
                service.Dispose();
        }
    }
}
