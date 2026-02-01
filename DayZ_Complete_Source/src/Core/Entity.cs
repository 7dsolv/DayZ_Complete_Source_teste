using System;
using System.Collections.Generic;

namespace DayZ.Core
{
    /// <summary>
    /// Base interface for all entity components
    /// </summary>
    public interface IComponent
    {
        Entity Owner { get; set; }
        void OnAttached();
        void OnDetached();
        void Update(float deltaTime);
    }

    /// <summary>
    /// Base component class
    /// </summary>
    public abstract class Component : IComponent
    {
        public Entity Owner { get; set; } = null!;

        public virtual void OnAttached() { }
        public virtual void OnDetached() { }
        public virtual void Update(float deltaTime) { }
    }

    /// <summary>
    /// Base entity class
    /// </summary>
    public class Entity
    {
        private Dictionary<Type, IComponent> _components = new();
        public string Name { get; set; } = "Entity";
        public Guid Id { get; private set; } = Guid.NewGuid();
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Add a component to this entity
        /// </summary>
        public T AddComponent<T>(T component) where T : IComponent
        {
            var type = typeof(T);
            if (_components.ContainsKey(type))
                throw new InvalidOperationException($"Component {type.Name} already attached");

            component.Owner = this;
            _components[type] = component;
            component.OnAttached();

            return component;
        }

        /// <summary>
        /// Get a component from this entity
        /// </summary>
        public T? GetComponent<T>() where T : IComponent
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var component))
                return (T)component;
            return default;
        }

        /// <summary>
        /// Check if entity has a component
        /// </summary>
        public bool HasComponent<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Remove a component from this entity
        /// </summary>
        public bool RemoveComponent<T>() where T : IComponent
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var component))
            {
                component.OnDetached();
                _components.Remove(type);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update all components
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            foreach (var component in _components.Values)
            {
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// Get all components
        /// </summary>
        public IEnumerable<IComponent> GetAllComponents()
        {
            return _components.Values;
        }
    }

    /// <summary>
    /// World/Scene manager
    /// </summary>
    public class World
    {
        private Dictionary<Guid, Entity> _entities = new();
        private List<Entity> _entitiesToRemove = new();

        public event EventHandler<Entity>? OnEntityAdded;
        public event EventHandler<Entity>? OnEntityRemoved;

        /// <summary>
        /// Create and add a new entity to the world
        /// </summary>
        public Entity CreateEntity(string name = "Entity")
        {
            var entity = new Entity { Name = name };
            _entities[entity.Id] = entity;
            OnEntityAdded?.Invoke(this, entity);
            return entity;
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public Entity? GetEntity(Guid id)
        {
            if (_entities.TryGetValue(id, out var entity))
                return entity;
            return null;
        }

        /// <summary>
        /// Get entities by name
        /// </summary>
        public IEnumerable<Entity> GetEntitiesByName(string name)
        {
            return _entities.Values.Where(e => e.Name == name);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        public IEnumerable<Entity> GetAllEntities()
        {
            return _entities.Values.ToList();
        }

        /// <summary>
        /// Remove entity from world
        /// </summary>
        public void RemoveEntity(Guid id)
        {
            if (_entities.TryGetValue(id, out var entity))
            {
                _entitiesToRemove.Add(entity);
                OnEntityRemoved?.Invoke(this, entity);
            }
        }

        /// <summary>
        /// Update all entities
        /// </summary>
        public void Update(float deltaTime)
        {
            // Remove marked entities
            foreach (var entity in _entitiesToRemove)
            {
                _entities.Remove(entity.Id);
            }
            _entitiesToRemove.Clear();

            // Update all active entities
            foreach (var entity in _entities.Values)
            {
                entity.Update(deltaTime);
            }
        }

        /// <summary>
        /// Clear all entities
        /// </summary>
        public void Clear()
        {
            _entities.Clear();
            _entitiesToRemove.Clear();
        }

        /// <summary>
        /// Get entity count
        /// </summary>
        public int GetEntityCount()
        {
            return _entities.Count;
        }
    }
}
