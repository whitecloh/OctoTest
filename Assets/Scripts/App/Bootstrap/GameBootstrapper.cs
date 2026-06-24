using System;
using UnityEngine;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameServicesInstaller servicesInstaller;
        [SerializeField] private SceneInitializer sceneInitializer;

        private GameContext _context;
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

            _context = servicesInstaller.Build();

            if (_context == null)
            {
                throw new InvalidOperationException($"{nameof(GameBootstrapper)} requires assigned {nameof(GameContext)}.");
            }
            
            sceneInitializer.Initialize(_context);
            _isBooted = true;
        }

        private void OnDestroy()
        {
            sceneInitializer?.Dispose();
            _context?.Dispose();
            _context = null;
            _isBooted = false;
        }
    }
}
