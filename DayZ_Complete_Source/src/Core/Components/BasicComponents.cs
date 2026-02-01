using System;
using System.Collections.Generic;
using System.Linq;

namespace DayZ.Core.Components
{
    /// <summary>
    /// Transform component for positioning and rotation
    /// </summary>
    public class TransformComponent : Component
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Z { get; set; } = 0f;
        public float RotationX { get; set; } = 0f;
        public float RotationY { get; set; } = 0f;
        public float RotationZ { get; set; } = 0f;
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public float ScaleZ { get; set; } = 1f;

        public void SetPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void SetRotation(float x, float y, float z)
        {
            RotationX = x;
            RotationY = y;
            RotationZ = z;
        }

        public void SetScale(float x, float y, float z)
        {
            ScaleX = x;
            ScaleY = y;
            ScaleZ = z;
        }

        public (float, float, float) GetPosition() => (X, Y, Z);
        public (float, float, float) GetRotation() => (RotationX, RotationY, RotationZ);
        public (float, float, float) GetScale() => (ScaleX, ScaleY, ScaleZ);
    }

    /// <summary>
    /// Health component
    /// </summary>
    public class HealthComponent : Component
    {
        private float _currentHealth;
        private float _maxHealth;

        public float CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = Math.Max(0, Math.Min(value, _maxHealth));
        }

        public float MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = Math.Max(1, value);
        }

        public bool IsAlive => _currentHealth > 0;

        public HealthComponent(float maxHealth = 100f)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
        }

        public void Heal(float amount)
        {
            CurrentHealth += amount;
        }

        public override string ToString()
        {
            return $"Health: {_currentHealth:F1}/{_maxHealth:F1}";
        }
    }

    /// <summary>
    /// Inventory component
    /// </summary>
    public class InventoryComponent : Component
    {
        private List<InventoryItem> _items = new();
        public int MaxSlots { get; set; } = 50;

        public void AddItem(string itemId, int quantity = 1, float weight = 0f)
        {
            var existing = _items.FirstOrDefault(i => i.ItemId == itemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new InventoryItem { ItemId = itemId, Quantity = quantity, Weight = weight });
            }
        }

        public void RemoveItem(string itemId, int quantity = 1)
        {
            var item = _items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                item.Quantity -= quantity;
                if (item.Quantity <= 0)
                    _items.Remove(item);
            }
        }

        public IReadOnlyList<InventoryItem> GetItems() => _items.AsReadOnly();
        public int GetItemCount() => _items.Count;
        public float GetTotalWeight() => _items.Sum(i => i.Weight * i.Quantity);
    }

    public class InventoryItem
    {
        public string ItemId { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public float Weight { get; set; } = 0.5f;
    }

    /// <summary>
    /// Animated component
    /// </summary>
    public class AnimatedComponent : Component
    {
        public string CurrentAnimation { get; set; } = "idle";
        public float AnimationProgress { get; set; } = 0f;
        public float AnimationSpeed { get; set; } = 1f;
        public bool IsLooping { get; set; } = true;

        public void PlayAnimation(string animationName, bool loop = true)
        {
            CurrentAnimation = animationName;
            AnimationProgress = 0f;
            IsLooping = loop;
        }

        public override void Update(float deltaTime)
        {
            AnimationProgress += deltaTime * AnimationSpeed;
            if (!IsLooping && AnimationProgress >= 1f)
                AnimationProgress = 1f;
            else if (IsLooping)
                AnimationProgress %= 1f;
        }
    }

    /// <summary>
    /// Physics component
    /// </summary>
    public class PhysicsComponent : Component
    {
        public float VelocityX { get; set; } = 0f;
        public float VelocityY { get; set; } = 0f;
        public float VelocityZ { get; set; } = 0f;
        public float Mass { get; set; } = 1f;
        public float Drag { get; set; } = 0.1f;
        public bool UseGravity { get; set; } = true;
        public float GravityScale { get; set; } = 1f;

        public override void Update(float deltaTime)
        {
            if (UseGravity)
            {
                VelocityY -= 9.8f * GravityScale * deltaTime;
            }

            VelocityX *= (1 - Drag * deltaTime);
            VelocityY *= (1 - Drag * deltaTime);
            VelocityZ *= (1 - Drag * deltaTime);

            var transform = Owner.GetComponent<TransformComponent>();
            if (transform != null)
            {
                transform.X += VelocityX * deltaTime;
                transform.Y += VelocityY * deltaTime;
                transform.Z += VelocityZ * deltaTime;
            }
        }

        public void ApplyForce(float x, float y, float z)
        {
            VelocityX += x / Mass;
            VelocityY += y / Mass;
            VelocityZ += z / Mass;
        }
    }
}
