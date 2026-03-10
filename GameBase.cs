using Autofac;
using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json;

namespace Platformer
{
    public abstract class GameBase : Game
    {
        protected GraphicsDeviceManager GraphicsDeviceManager { get; }
        protected IContainer Container { get; private set; }
        public GameSettings Settings { get; private set; }

        private static readonly string SettingsPath = 
            Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.ApplicationData), 
                "Platformer", "settings.json");

        public int Width { get; }
        public int Height { get; }

        protected GameBase(int width = 800, int height = 480)
        {
            Width = width;
            Height = height;
            GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height
            };
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";

            Settings = LoadSettings();
            ApplySettings();
        }

        protected override void Initialize()
        {
            var containerBuilder = new ContainerBuilder();
            RegisterDependencies(containerBuilder);
            Container = containerBuilder.Build();
            base.Initialize();
        }

        protected abstract void RegisterDependencies(ContainerBuilder builder);

        // --- Settings persistence ---

        public GameSettings LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
            }
            return new GameSettings();
        }

        public void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }

        public void ApplySettings()
        {
            GraphicsDeviceManager.IsFullScreen = Settings.IsFullscreen;
            GraphicsDeviceManager.ApplyChanges();
        }

        public void ToggleFullscreen()
        {
            Settings.IsFullscreen = !Settings.IsFullscreen;
            ApplySettings();
            SaveSettings();
        }
    }
}