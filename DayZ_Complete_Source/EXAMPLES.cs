using System;
using DayZ.Core;
using DayZ.Core.Components;
using DayZ.Game;
using DayZ.Server;
using DayZ.Mods;

namespace DayZ.Examples
{
    /// <summary>
    /// Exemplos de como usar e expandir o sistema
    /// </summary>
    public class UsageExamples
    {
        // ============================================================
        // EXEMPLO 1: Criar Entity com Componentes
        // ============================================================
        public void Example1_CreateEntity()
        {
            var world = new World();

            // Criar um jogador
            var player = world.CreateEntity("PlayerEntity");
            
            // Adicionar componentes
            var transform = player.AddComponent(new TransformComponent());
            transform.SetPosition(100, 50, 100);
            
            var health = player.AddComponent(new HealthComponent(100f));
            var inventory = player.AddComponent(new InventoryComponent { MaxSlots = 50 });
            
            // Adicionar itens ao inventário
            inventory.AddItem("canned_beans", 5, 0.4f);
            inventory.AddItem("bandage", 10, 0.1f);
            
            Console.WriteLine($"Player health: {health.CurrentHealth}");
            Console.WriteLine($"Inventory weight: {inventory.GetTotalWeight()}");
        }

        // ============================================================
        // EXEMPLO 2: Item Database
        // ============================================================
        public void Example2_ItemDatabase()
        {
            var economy = new EconomySystem();
            var db = economy.ItemDatabase;
            
            // Obter item específico
            var mosin = db.GetItem("mosin_nagant");
            if (mosin != null)
                Console.WriteLine($"Found: {mosin.Name} - Weight: {mosin.Weight}");
            
            // Listar itens por categoria
            var weapons = db.GetItemsByCategory("Weapons");
            Console.WriteLine($"Weapons count: {weapons.Count()}");
            
            // Listar itens raros
            var rareItems = db.GetItemsByRarity("rare");
            Console.WriteLine($"Rare items: {rareItems.Count()}");
        }

        // ============================================================
        // EXEMPLO 3: Server Manager
        // ============================================================
        public void Example3_ServerManager()
        {
            var config = new ServerConfiguration
            {
                ServerName = "Community Server",
                MaxPlayers = 100,
                Difficulty = "Hard"
            };
            
            var server = new ServerManager(config);
            
            // Criar jogador
            var player1 = server.CreatePlayer("Player1");
            var player2 = server.CreatePlayer("Player2");
            
            Console.WriteLine($"Players: {server.PlayerCount}/{config.MaxPlayers}");
            
            // Ban de jogador
            server.BanPlayer(player1.PlayerId);
            Console.WriteLine($"Player banned: {server.IsPlayerBanned("Player1")}");
            
            // Whitelist
            server.WhitelistPlayer("Admin");
            Console.WriteLine($"Whitelisted: {server.IsPlayerWhitelisted("Admin")}");
        }

        // ============================================================
        // EXEMPLO 4: Mod System
        // ============================================================
        public void Example4_ModSystem()
        {
            var modManager = new ModManager();
            
            // Registrar mods
            modManager.RegisterMod(
                new ExpansionMod(),
                new ModMetadata { ModId = "expansion", ModName = "Expansion", Version = "1.5.0" }
            );
            
            modManager.RegisterMod(
                new CommunityOnlineToolsMod(),
                new ModMetadata { ModId = "tools", ModName = "Tools", Version = "1.0.0" }
            );
            
            // Inicializar
            modManager.InitializeAllMods();
            Console.WriteLine($"Mods loaded: {modManager.LoadedModCount}");
            
            // Notificar eventos
            modManager.NotifyPlayerJoin("player_123");
            modManager.StartAllMods();
        }

        // ============================================================
        // EXEMPLO 5: Criar Componente Customizado
        // ============================================================
        public class FatigueComponent : Component
        {
            public float Fatigue { get; set; } = 0f;
            public float MaxFatigue { get; set; } = 100f;

            public void AddFatigue(float amount)
            {
                Fatigue = Math.Min(MaxFatigue, Fatigue + amount);
            }

            public void Rest(float amount)
            {
                Fatigue = Math.Max(0, Fatigue - amount);
            }

            public override void Update(float deltaTime)
            {
                // A cada segundo de atividade, aumenta fadiga
                if (Owner.HasComponent<PhysicsComponent>())
                {
                    var physics = Owner.GetComponent<PhysicsComponent>();
                    if (physics != null)
                    {
                        float speed = (float)Math.Sqrt(
                            physics.VelocityX * physics.VelocityX +
                            physics.VelocityY * physics.VelocityY +
                            physics.VelocityZ * physics.VelocityZ
                        );
                        
                        if (speed > 0.1f)
                            AddFatigue(deltaTime * 10f);
                    }
                }
                
                // Repouso natural
                Rest(deltaTime * 2f);
            }
        }

        // ============================================================
        // EXEMPLO 6: Criar Mod Customizado
        // ============================================================
        public class CustomMod : BaseMod
        {
            public override string ModId => "custom_mod";
            public override string ModName => "Custom Mod";
            public override string Version => "1.0.0";

            public override void Initialize()
            {
                Console.WriteLine($"{ModName} initializing...");
            }

            public override void OnGameStart()
            {
                Console.WriteLine($"{ModName} game started");
            }

            public override void OnPlayerJoin(string playerId)
            {
                Console.WriteLine($"{ModName} detected player join: {playerId}");
            }

            public override void OnPlayerLeave(string playerId)
            {
                Console.WriteLine($"{ModName} detected player leave: {playerId}");
            }
        }

        // ============================================================
        // EXEMPLO 7: Game Engine Loop
        // ============================================================
        public void Example7_GameLoop()
        {
            var engine = new GameEngine();
            var world = new World();

            // Criar entities
            var entity = world.CreateEntity("TestEntity");
            entity.AddComponent(new TransformComponent());

            // Setup engine
            engine.OnUpdate += (sender, deltaTime) =>
            {
                world.Update(deltaTime);
                Console.WriteLine($"Frame: deltaTime={deltaTime:F3}ms, entities={world.GetEntityCount()}");
            };

            engine.Initialize();

            // Simular alguns frames
            for (int i = 0; i < 5; i++)
            {
                engine.Tick();
                System.Threading.Thread.Sleep(16); // ~60 FPS
            }

            engine.Shutdown();
        }

        // ============================================================
        // EXEMPLO 8: Adicionar Novo Item
        // ============================================================
        public void Example8_AddCustomItem()
        {
            var economy = new EconomySystem();
            
            // Adicionar novo item customizado
            economy.ItemDatabase.AddItem(new ItemDefinition
            {
                Id = "laser_rifle",
                Name = "Laser Rifle",
                Category = "Weapons",
                Type = "Energy Weapon",
                Weight = 2.5f,
                Rarity = "legendary",
                SpawnProbability = 0.05f,
                MaxStack = 1,
                Description = "Futuristic laser weapon",
                Properties = new()
                {
                    { "damage", 150 },
                    { "fire_rate", 10 },
                    { "ammo_type", "energy_cell" }
                }
            });
            
            var item = economy.ItemDatabase.GetItem("laser_rifle");
            Console.WriteLine($"New item: {item?.Name} - {item?.Description}");
        }

        // ============================================================
        // EXEMPLO 9: Spawn Point Configuration
        // ============================================================
        public void Example9_SpawnPoints()
        {
            var economy = new EconomySystem();
            
            // Registrar spawn points
            economy.RegisterSpawnPoint("Forest", new ItemSpawn
            {
                LocationName = "Forest",
                SpawnMultiplier = 0.8f,
                MaxItems = 100,
                IsMilitaryZone = false
            });

            economy.RegisterSpawnPoint("Military_Base", new ItemSpawn
            {
                LocationName = "Military_Base",
                SpawnMultiplier = 2.0f,
                MaxItems = 300,
                IsMilitaryZone = true
            });

            // Gerar itens para uma localização
            var spawnedItems = economy.GetSpawnedItems("Military_Base");
            Console.WriteLine($"Items spawned in Military_Base: {spawnedItems.Count}");
        }

        // ============================================================
        // MAIN - Executar exemplos
        // ============================================================
        public static void Main(string[] args)
        {
            var examples = new UsageExamples();
            
            Console.WriteLine("╔════════════════════════════════════╗");
            Console.WriteLine("║  DayZ 1.25 - Usage Examples        ║");
            Console.WriteLine("╚════════════════════════════════════╝\n");

            Console.WriteLine("Example 1: Create Entity with Components");
            examples.Example1_CreateEntity();
            Console.WriteLine();

            Console.WriteLine("Example 2: Item Database");
            examples.Example2_ItemDatabase();
            Console.WriteLine();

            Console.WriteLine("Example 3: Server Manager");
            examples.Example3_ServerManager();
            Console.WriteLine();

            Console.WriteLine("Example 4: Mod System");
            examples.Example4_ModSystem();
            Console.WriteLine();

            Console.WriteLine("Example 5: Custom Component (FatigueComponent)");
            Console.WriteLine("✅ FatigueComponent defined and ready to use");
            Console.WriteLine();

            Console.WriteLine("Example 6: Custom Mod");
            Console.WriteLine("✅ CustomMod defined and ready to use");
            Console.WriteLine();

            Console.WriteLine("Example 7: Game Engine Loop");
            examples.Example7_GameLoop();
            Console.WriteLine();

            Console.WriteLine("Example 8: Add Custom Item");
            examples.Example8_AddCustomItem();
            Console.WriteLine();

            Console.WriteLine("Example 9: Spawn Points");
            examples.Example9_SpawnPoints();
            Console.WriteLine();

            Console.WriteLine("╔════════════════════════════════════╗");
            Console.WriteLine("║  All examples completed!           ║");
            Console.WriteLine("╚════════════════════════════════════╝");
        }
    }
}
