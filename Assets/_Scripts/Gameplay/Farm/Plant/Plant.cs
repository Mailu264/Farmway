using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class Plant
    {
        private readonly List<PlantComponent> _components = new();
        private readonly Dictionary<Type, PlantService> _services = new();

        private readonly ReactiveProperty<int> _stage = new(0);
        private readonly ReactiveProperty<bool> _isReadyToHarvest = new(false);

        public PlantType Type { get; }

        public IReadOnlyReactiveProperty<int> Stage => _stage;
        public IReadOnlyReactiveProperty<bool> IsReadyToHarvest => _isReadyToHarvest;

        public Plant(PlantType type) => Type = type;

        public void AddBehaviour(PlantBehaviour behaviour)
        {
            behaviour.Bind(this);
            _components.Add(behaviour);
        }

        public void AddService(PlantService service)
        {
            service.Bind(this);
            _components.Add(service);
            _services.Add(service.GetType(), service);
        }

        public T GetService<T>() where T : PlantService
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            Debug.LogError($"[Plant] Service {typeof(T)} not found");
            return null;
        }

        public void OnPlanted()
        {
            foreach (var component in _components)
                component.OnPlanted();
        }

        public void Tick(float hours)
        {
            foreach (var component in _components)
                component.OnTick(hours);
        }

        public void Dispose()
        {
            foreach (var component in _components)
                component.Dispose();
        }

        // --- мутации состояния, используются компонентами роста ---

        public void SetStage(int stage) => _stage.Value = stage;
        public void SetReadyToHarvest() => _isReadyToHarvest.Value = true;
    }
}
