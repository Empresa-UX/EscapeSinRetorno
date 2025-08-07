using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.UI;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.Core
{
    public class GamePlayState : IGameState
    {
        private TileMap tileMap;
        private Player player;
        private EnemyManager enemyManager;
        private Camera2D camera;
        private GameStateManager stateManager;
        private GraphicsDeviceManager graphics;

        public GamePlayState(GameStateManager stateManager, GraphicsDeviceManager graphics)
        {
            this.stateManager = stateManager;
            this.graphics = graphics;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            tileMap = new TileMap(tileSize: 16);
            tileMap.LoadContent(content);

            player = new Player();
            player.LoadContent(content, graphicsDevice);

            if (tileMap.PlayerStartPosition.HasValue)
            {
                player.SetPosition(tileMap.PlayerStartPosition.Value);
            }

            enemyManager = new EnemyManager();
            enemyManager.SpawnFromMapData(tileMap.EnemySpawns);
            enemyManager.LoadContent(content);

            Enemy.LoadDebugTexture(graphicsDevice);

            camera = new Camera2D(graphicsDevice.Viewport);
            camera.SetZoom(5.0f);
        }

        public void Update(GameTime gameTime)
        {
            enemyManager.Update(gameTime, player , tileMap);
            player.Update(gameTime, tileMap);
            camera.Follow(player.Position, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: camera.GetTransform());

            tileMap.DrawBackground(spriteBatch, Vector2.Zero, 1366, 768);
            tileMap.Draw(spriteBatch, Vector2.Zero);
            enemyManager.Draw(spriteBatch);
            player.Draw(spriteBatch);

            spriteBatch.End();
        }

        public void HandleInput()
        {
            // Permitir volver al menú con ESC
            if (InputManager.IsKeyPressed(Keys.Escape))
            {
                stateManager.ChangeState("Menu");
            }
        }
    }
}