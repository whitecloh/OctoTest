using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameServicesInstaller servicesInstaller;
        [SerializeField] private SceneInitializer sceneInitializer;

        private EcsWorld _world;
        private IEcsSystems _systems;
        private GameServices _services;
        private bool _isBooted;
        
        public bool IsBooted => _isBooted;

        private void Awake()
        {
            Boot();
        }
        
        private void Boot()
        {
            if (_isBooted)
            {
                return;
            }

            if (servicesInstaller == null || sceneInitializer == null)
            {
                throw new InvalidOperationException($"{nameof(GameBootstrapper)} requires assigned bootstrap references.");
            }

            _world = new EcsWorld();
            _services = servicesInstaller.Build(_world);

            if (_services == null)
            {
                throw new InvalidOperationException($"{nameof(GameBootstrapper)} requires assigned {nameof(GameServices)}.");
            }

            _systems = new EcsCompositionRoot(_services).Create(_world);
            _systems.Init();
            sceneInitializer.Initialize(_services);
            _services.UnitCommands.LoadOrCreateInitial();
            _services.SetDeltaTime(0f);
            _systems.Run();
            sceneInitializer.Refresh(_services);
            _isBooted = true;
        }

        private void Update()
        {
            if (!_isBooted)
            {
                return;
            }

            _services.SetDeltaTime(Time.deltaTime);
            _systems.Run();
            sceneInitializer.Refresh(_services);
        }

        private void OnDestroy()
        {
            sceneInitializer?.Dispose();
            _systems?.Destroy();
            _world?.Destroy();
            _systems = null;
            _world = null;
            _services = null;
            _isBooted = false;
        }
    }
}
