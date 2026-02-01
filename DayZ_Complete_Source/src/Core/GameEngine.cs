using System;
using System.Collections.Generic;

namespace DayZ.Core
{
    /// <summary>
    /// Base interface for all engine systems
    /// </summary>
    public interface IEngineSystem : IDisposable
    {
        string Name { get; }
        void Initialize();
        void Update(float deltaTime);
        void Shutdown();
    }

    /// <summary>
    /// Core game engine for DayZ
    /// </summary>
    public class GameEngine
    {
        private Dictionary<string, IEngineSystem> _systems = new();
        private bool _isRunning = false;
        private float _deltaTime = 0f;
        private long _lastTickTime = 0;

        public event EventHandler<EventArgs>? OnEngineInitialized;
        public event EventHandler<EventArgs>? OnEngineShutdown;
        public event EventHandler<float>? OnUpdate;

        public bool IsRunning => _isRunning;
        public float DeltaTime => _deltaTime;

        public GameEngine()
        {
            _lastTickTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Register an engine system
        /// </summary>
        public void RegisterSystem(IEngineSystem system)
        {
            if (_systems.ContainsKey(system.Name))
                throw new InvalidOperationException($"System {system.Name} already registered");

            _systems[system.Name] = system;
        }

        /// <summary>
        /// Get a registered system by name
        /// </summary>
        public T? GetSystem<T>(string name) where T : IEngineSystem
        {
            if (_systems.TryGetValue(name, out var system))
                return system as T;
            return null;
        }

        /// <summary>
        /// Initialize all registered systems
        /// </summary>
        public void Initialize()
        {
            foreach (var system in _systems.Values)
            {
                try
                {
                    system.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing system {system.Name}: {ex.Message}");
                    throw;
                }
            }

            _isRunning = true;
            OnEngineInitialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Main engine loop tick
        /// </summary>
        public void Tick()
        {
            if (!_isRunning) return;

            // Calculate delta time
            long currentTime = DateTime.Now.Ticks;
            _deltaTime = (float)(currentTime - _lastTickTime) / TimeSpan.TicksPerSecond;
            _lastTickTime = currentTime;

            // Update all systems
            foreach (var system in _systems.Values)
            {
                try
                {
                    system.Update(_deltaTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating system {system.Name}: {ex.Message}");
                }
            }

            OnUpdate?.Invoke(this, _deltaTime);
        }

        /// <summary>
        /// Shutdown all systems
        /// </summary>
        public void Shutdown()
        {
            _isRunning = false;

            foreach (var system in _systems.Values)
            {
                try
                {
                    system.Shutdown();
                    system.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error shutting down system {system.Name}: {ex.Message}");
                }
            }

            _systems.Clear();
            OnEngineShutdown?.Invoke(this, EventArgs.Empty);
        }
    }
}
