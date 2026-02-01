using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DayZ.Game
{
    public class ItemDefinition
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("category")]
        public string Category { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("weight")]
        public float Weight { get; set; } = 0.5f;

        [JsonProperty("rarity")]
        public string Rarity { get; set; } = "common";

        [JsonProperty("spawn_probability")]
        public float SpawnProbability { get; set; } = 0.5f;

        [JsonProperty("max_stack")]
        public int MaxStack { get; set; } = 1;

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();

        public override string ToString()
        {
            return $"{Name} ({Id}) - {Category}/{Type}";
        }
    }

    public class ItemDatabase
    {
        private Dictionary<string, ItemDefinition> _items = new();

        public ItemDatabase()
        {
            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            // Weapons
            AddItem(new ItemDefinition
            {
                Id = "mosin_nagant",
                Name = "Mosin Nagant",
                Category = "Weapons",
                Type = "Rifle",
                Weight = 3.5f,
                Rarity = "uncommon",
                SpawnProbability = 0.3f,
                Description = "Soviet bolt-action rifle"
            });

            AddItem(new ItemDefinition
            {
                Id = "akm",
                Name = "AKM",
                Category = "Weapons",
                Type = "Rifle",
                Weight = 3.2f,
                Rarity = "uncommon",
                SpawnProbability = 0.25f,
                Description = "Soviet assault rifle"
            });

            // Ammunition
            AddItem(new ItemDefinition
            {
                Id = "ammo_762x54",
                Name = "7.62x54R Ammunition",
                Category = "Ammunition",
                Type = "Rifle Ammo",
                Weight = 0.05f,
                Rarity = "uncommon",
                SpawnProbability = 0.4f,
                MaxStack = 20,
                Description = "Rifle ammunition"
            });

            // Food
            AddItem(new ItemDefinition
            {
                Id = "canned_beans",
                Name = "Canned Beans",
                Category = "Food",
                Type = "Food",
                Weight = 0.4f,
                Rarity = "common",
                SpawnProbability = 0.6f,
                Description = "Canned beans for food"
            });

            AddItem(new ItemDefinition
            {
                Id = "canned_meat",
                Name = "Canned Meat",
                Category = "Food",
                Type = "Food",
                Weight = 0.4f,
                Rarity = "common",
                SpawnProbability = 0.5f,
                Description = "Canned meat for food"
            });

            // Medical
            AddItem(new ItemDefinition
            {
                Id = "bandage",
                Name = "Bandage",
                Category = "Medical",
                Type = "Consumable",
                Weight = 0.1f,
                Rarity = "common",
                SpawnProbability = 0.7f,
                MaxStack = 10,
                Description = "Basic bandage for medical treatment"
            });

            AddItem(new ItemDefinition
            {
                Id = "morphine",
                Name = "Morphine",
                Category = "Medical",
                Type = "Consumable",
                Weight = 0.1f,
                Rarity = "rare",
                SpawnProbability = 0.15f,
                Description = "Morphine for pain relief"
            });

            // Clothing
            AddItem(new ItemDefinition
            {
                Id = "tactical_vest",
                Name = "Tactical Vest",
                Category = "Clothing",
                Type = "Armor",
                Weight = 2.0f,
                Rarity = "uncommon",
                SpawnProbability = 0.2f,
                Description = "Protective tactical vest"
            });

            AddItem(new ItemDefinition
            {
                Id = "gas_mask",
                Name = "Gas Mask",
                Category = "Clothing",
                Type = "Armor",
                Weight = 0.5f,
                Rarity = "uncommon",
                SpawnProbability = 0.25f,
                Description = "Protective gas mask"
            });

            // Tools
            AddItem(new ItemDefinition
            {
                Id = "flashlight",
                Name = "Flashlight",
                Category = "Tools",
                Type = "Equipment",
                Weight = 0.3f,
                Rarity = "common",
                SpawnProbability = 0.5f,
                Description = "Electric flashlight"
            });

            AddItem(new ItemDefinition
            {
                Id = "map",
                Name = "Map",
                Category = "Tools",
                Type = "Equipment",
                Weight = 0.2f,
                Rarity = "common",
                SpawnProbability = 0.8f,
                Description = "Detailed map of the region"
            });
        }

        public void AddItem(ItemDefinition item)
        {
            _items[item.Id] = item;
        }

        public ItemDefinition? GetItem(string id)
        {
            return _items.TryGetValue(id, out var item) ? item : null;
        }

        public IEnumerable<ItemDefinition> GetItemsByCategory(string category)
        {
            return _items.Values.Where(i => i.Category == category);
        }

        public IEnumerable<ItemDefinition> GetItemsByRarity(string rarity)
        {
            return _items.Values.Where(i => i.Rarity == rarity);
        }

        public IEnumerable<ItemDefinition> GetAllItems()
        {
            return _items.Values;
        }

        public int GetItemCount()
        {
            return _items.Count;
        }
    }

    public class EconomySystem : IDisposable
    {
        private ItemDatabase _itemDatabase;
        private Dictionary<string, ItemSpawn> _spawnPoints = new();

        public ItemDatabase ItemDatabase => _itemDatabase;

        public EconomySystem()
        {
            _itemDatabase = new ItemDatabase();
        }

        public void RegisterSpawnPoint(string locationName, ItemSpawn spawn)
        {
            _spawnPoints[locationName] = spawn;
        }

        public ItemSpawn? GetSpawnPoint(string locationName)
        {
            return _spawnPoints.TryGetValue(locationName, out var spawn) ? spawn : null;
        }

        public List<string> GetSpawnedItems(string locationName)
        {
            if (!_spawnPoints.TryGetValue(locationName, out var spawn))
                return new List<string>();

            var items = new List<string>();
            Random random = new Random();

            foreach (var itemDef in _itemDatabase.GetAllItems())
            {
                if (random.NextDouble() < itemDef.SpawnProbability * spawn.SpawnMultiplier)
                {
                    items.Add(itemDef.Id);
                }
            }

            return items;
        }

        public void Dispose()
        {
            _spawnPoints.Clear();
        }
    }

    public class ItemSpawn
    {
        public string LocationName { get; set; } = "";
        public float SpawnMultiplier { get; set; } = 1.0f;
        public int MaxItems { get; set; } = 100;
        public bool IsMilitaryZone { get; set; } = false;
    }
}
