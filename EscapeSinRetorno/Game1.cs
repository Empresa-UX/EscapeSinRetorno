﻿using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using EscapeSinRetorno.Source.Entities; // Recuerda importar Player
using EscapeSinRetorno.Source.Entities.Enemies; // Recuerda importar Player


namespace EscapeSinRetorno
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private EnemyManager _enemyManager;

        private TileMap _tileMap;
        private Player _player;
        private Camera2D _camera;

        public static readonly System.Random Random = new System.Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _tileMap = new TileMap(tileSize: 16);
            _tileMap.LoadContent(Content);

            _player = new Player();
            _player.LoadContent(Content, GraphicsDevice);

            if (_tileMap.PlayerStartPosition.HasValue)
            {
                _player.SetPosition(_tileMap.PlayerStartPosition.Value);
            }

            _enemyManager = new EnemyManager();
            _enemyManager.SpawnFromMapData(_tileMap.EnemySpawns);
            _enemyManager.LoadContent(Content);


            _camera = new Camera2D(GraphicsDevice.Viewport);
            _camera.SetZoom(5.0f); // Zoom x2
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _enemyManager.Update(gameTime, _player.Position);

            _player.Update(gameTime, _tileMap);
            
            _camera.Follow(_player.Position, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix: _camera.GetTransform());

            _tileMap.DrawBackground(_spriteBatch, camera: _camera.GetPosition(), screenWidth: 1366, screenHeight: 768);
            _tileMap.Draw(_spriteBatch, Vector2.Zero);

            _enemyManager.Draw(_spriteBatch, _camera.GetPosition());

            _player.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
