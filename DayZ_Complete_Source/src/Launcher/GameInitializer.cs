using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DayZ.Core;
using DayZ.Core.Components;
using DayZ.Game;
using DayZ.Server;
using DayZ.Mods;

namespace DayZ.Launcher
{
    /// <summary>
    /// Inicializador completo do DayZ com todas as camadas
    /// </summary>
    public class GameInitializer
    {
        private GameEngine? _engine;
        private ServerManager? _serverManager;
        private ModManager? _modManager;
        private EconomySystem? _economySystem;

        public async Task InitializeGameAsync()
        {
            try
            {
                Console.WriteLine("🚀 Inicializando DayZ 1.25...");

                // 1. Criar engine
                _engine = new GameEngine();
                _engine.Initialize();
                Console.WriteLine("✅ Game Engine inicializado");

                // 2. Carregar configuração do servidor
                var config = LoadServerConfig();
                _serverManager = new ServerManager(config);
                Console.WriteLine("✅ Server Manager carregado");

                // 3. Inicializar sistema de economia
                _economySystem = new EconomySystem();
                Console.WriteLine($"✅ Item Database: {_economySystem.ItemDatabase.GetItemCount()} itens");

                // 4. Inicializar sistema de mods
                _modManager = new ModManager();
                RegisterDefaultMods();
                _modManager.InitializeAllMods();
                Console.WriteLine($"✅ Mods carregados: {_modManager.LoadedModCount}");

                // 5. Criar mundo
                var world = new World();
                Console.WriteLine("✅ Mundo criado");

                // 6. Criar exemplo de jogador
                var player = _serverManager.CreatePlayer("TestPlayer");
                var playerEntity = world.CreateEntity("Player");
                playerEntity.AddComponent(new TransformComponent { X = 100, Y = 50, Z = 100 });
                playerEntity.AddComponent(new HealthComponent(100f));
                playerEntity.AddComponent(new InventoryComponent { MaxSlots = 50 });
                Console.WriteLine("✅ Jogador de teste criado");

                // 7. Notificar mods de player join
                _modManager.NotifyPlayerJoin(player.PlayerId);

                // 8. Iniciar mods
                _modManager.StartAllMods();

                Console.WriteLine("\n🎮 Sistema totalmente inicializado!");
                Console.WriteLine($"Server: {_serverManager.GetServerStatus()}");
                
                await RunGameLoopAsync(world);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro na inicialização: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task RunGameLoopAsync(World world)
        {
            Console.WriteLine("\n▶️  Iniciando loop do jogo...");
            Console.WriteLine("Pressione Ctrl+C para sair");
            
            var cts = new System.Threading.CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    _engine?.Tick();
                    world.Update(_engine?.DeltaTime ?? 0.016f);
                    
                    await Task.Delay(16); // ~60 FPS
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n⏹️  Game loop interrompido");
            }

            Shutdown();
        }

        private void RegisterDefaultMods()
        {
            _modManager?.RegisterMod(
                new ExpansionMod(),
                new ModMetadata
                {
                    ModId = "expansion",
                    ModName = "DayZ Expansion",
                    Version = "1.5.0",
                    LoadPriority = 10
                }
            );

            _modManager?.RegisterMod(
                new CommunityOnlineToolsMod(),
                new ModMetadata
                {
                    ModId = "community_online_tools",
                    ModName = "Community Online Tools",
                    Version = "1.3.0",
                    LoadPriority = 20
                }
            );

            _modManager?.RegisterMod(
                new DabsFrameworkMod(),
                new ModMetadata
                {
                    ModId = "dabs_framework",
                    ModName = "Dabs Framework",
                    Version = "1.0.0",
                    LoadPriority = 5
                }
            );
        }

        private ServerConfiguration LoadServerConfig()
        {
            var configPath = "config/server_config.json";
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return ServerConfiguration.LoadFromJson(json);
            }
            return new ServerConfiguration();
        }

        private void Shutdown()
        {
            Console.WriteLine("\n🛑 Desligando sistemas...");
            
            if (_modManager != null)
            {
                _modManager.StopAllMods();
                Console.WriteLine("✅ Mods desligados");
            }

            if (_engine != null)
            {
                _engine.Shutdown();
                Console.WriteLine("✅ Engine desligado");
            }

            if (_economySystem != null)
            {
                _economySystem.Dispose();
                Console.WriteLine("✅ Economy System desligado");
            }

            Console.WriteLine("\n👋 Até logo!");
        }
    }

    public class ConsoleGameLauncher
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            if (args.Contains("--console"))
            {
                // Modo console (para debug)
                var initializer = new GameInitializer();
                await initializer.InitializeGameAsync();
            }
            else
            {
                // Modo GUI (padrão)
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.Run(new LauncherForm());
            }
        }
    }
}
