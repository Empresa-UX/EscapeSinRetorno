﻿// Tile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.World
{
    public class Tile
    {
        private readonly List<Texture2D> _layers;
        public Vector2 Position { get; }

        public Tile(List<Texture2D> layers, Vector2 pos)
        {
            _layers = layers;
            Position = pos;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            if (_layers.Count > 0)
                spriteBatch.Draw(_layers[0], Position - camera, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}