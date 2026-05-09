using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public interface IPlayerServices
    {
        void EnableServices();
        void DisableServices();
        void EnableService<T>() where T : PlayerService;
        void DisableService<T>() where T : PlayerService;
    }

    public class PlayerServices : IPlayerServices, IInitializable, IStartable, ITickable, ILateTickable, IFixedTickable, IDisposable
    {
        private readonly Dictionary<Type, PlayerService> _playerServices = new();

        public bool IsEnabled { get; private set; }
        
        public PlayerServices(IEnumerable<PlayerService> services)
        {
            foreach (var playerService in services)
                _playerServices.Add(playerService.GetType(), playerService);
        }

        public void EnableServices()
        {
            IsEnabled = true;
            foreach (var playerService in _playerServices)
                playerService.Value.OnEnable();
        }

        public void DisableServices()
        {
            IsEnabled = false;
            foreach (var playerService in _playerServices)
                playerService.Value.OnDisable();
        }

        public void EnableService<T>() where T : PlayerService
        {
            if (!_playerServices.TryGetValue(typeof(T), out var playerService))
            {
                Debug.LogError($"{typeof(T)} does not exist");
                return;
            }
            
            playerService.OnEnable();
        }
        
        public void DisableService<T>() where T : PlayerService
        {
            if (!_playerServices.TryGetValue(typeof(T), out var playerService))
            {
                Debug.LogError($"{typeof(T)} does not exist");
                return;
            }
            
            playerService.OnDisable();
        }
        
        public void Initialize()
        {
            foreach (var playerService in _playerServices)
                playerService.Value.OnInitialize();
        }

        public void Start()
        {
            foreach (var playerService in _playerServices)
                playerService.Value.OnStart();
        }
        
        public void Tick()
        {
            if (!IsEnabled)
                return;

            foreach (var playerService in _playerServices)
            {
                if (!playerService.Value.IsEnabled)
                    continue;
                
                playerService.Value.OnUpdate();
            }
        }   
        
        public void LateTick()
        {
            if (!IsEnabled)
                return;

            foreach (var playerService in _playerServices)
            {
                if (!playerService.Value.IsEnabled)
                    continue;
                
                playerService.Value.OnLateUpdate();
            }
        }
        
        public void FixedTick()
        {
            if (!IsEnabled)
                return;

            foreach (var playerService in _playerServices)
            {
                if (!playerService.Value.IsEnabled)
                    continue;

                playerService.Value.OnFixedUpdate();
            }
        }

        public void Dispose()
        {
            foreach (var playerService in _playerServices)
                playerService.Value.OnDispose();
        }
    }
}