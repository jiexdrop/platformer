// SettingsMenu.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Platformer
{
    public class SettingsMenu
    {
        private readonly GameBase _game;
        private readonly SpriteFont _font;
        private readonly SpriteBatch _batch;

        public bool IsOpen { get; private set; }

        private readonly string[] _menuItems = { "Fullscreen: ", "Close" };
        private int _selectedIndex = 0;

        public SettingsMenu(GameBase game, SpriteFont font, SpriteBatch batch)
        {
            _game = game;
            _font = font;
            _batch = batch;
        }

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;

        public void Update()
        {
            if (!IsOpen) return;

            var keyboard = KeyboardExtended.GetState();

            if (keyboard.IsKeyDown(Keys.Up))
                _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;

            if (keyboard.IsKeyDown(Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;

            if (keyboard.IsKeyDown(Keys.Enter))
                Confirm();

            if (keyboard.IsKeyDown(Keys.Escape))
                Close();
        }

        private void Confirm()
        {
            switch (_selectedIndex)
            {
                case 0: // Fullscreen toggle
                    _game.ToggleFullscreen();
                    break;
                case 1: // Close
                    Close();
                    break;
            }
        }

        public void Draw()
        {
            if (!IsOpen) return;

            var screenWidth = _game.GraphicsDevice.Viewport.Width;
            var screenHeight = _game.GraphicsDevice.Viewport.Height;

            _batch.Begin();

            // Dim background
            var overlay = new Texture2D(_game.GraphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 180) });
            _batch.Draw(overlay, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            // Panel
            int panelW = 300, panelH = 200;
            int panelX = (screenWidth - panelW) / 2;
            int panelY = (screenHeight - panelH) / 2;

            var panel = new Texture2D(_game.GraphicsDevice, 1, 1);
            panel.SetData(new[] { new Color(30, 30, 30) });
            _batch.Draw(panel, new Rectangle(panelX, panelY, panelW, panelH), Color.White);

            // Title
            _batch.DrawString(_font, "Settings", 
                new Vector2(panelX + 90, panelY + 16), Color.White);

            // Menu items
            for (int i = 0; i < _menuItems.Length; i++)
            {
                var label = i == 0
                    ? $"Fullscreen: {(_game.Settings.IsFullscreen ? "ON" : "OFF")}"
                    : _menuItems[i];

                var color = i == _selectedIndex ? Color.Yellow : Color.LightGray;
                var prefix = i == _selectedIndex ? "> " : "  ";
                _batch.DrawString(_font, prefix + label,
                    new Vector2(panelX + 30, panelY + 60 + i * 40), color);
            }

            _batch.End();
        }
    }
}