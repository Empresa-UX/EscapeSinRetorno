using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShadowSky.World
{
    public class TileMap
    {
        private readonly int _tileSize;
        private TileType[,] _tiles;
        private int[,] _tileTextureIndices;
        private Dictionary<TileType, List<Texture2D>> _tileTextures;
        private readonly Random _rng = new();

        public TileMap(int tileSize)
        {
            _tileSize = tileSize;
            _tiles = new TileType[255, 135];
            _tileTextureIndices = new int[255, 135];

            // Por defecto todo es Grass
            for (int y = 0; y < 135; y++)
                for (int x = 0; x < 255; x++)
                {
                    _tiles[x, y] = TileType.Grass;
                    _tileTextureIndices[x, y] = 0; // Inicializa en 0 por defecto
                }
        }

        public void LoadContent(ContentManager content)
        {
            _tileTextures = new()
            {
                [TileType.Grass] = LoadTextures(content, "Tiles/terrain/grass", "grass", 4),
                [TileType.Path] = LoadTextures(content, "Tiles/terrain/path", "path", 4),
                [TileType.Water] = LoadTextures(content, "Tiles/terrain/water", "water", 4),
                [TileType.Stone] = LoadTextures(content, "Tiles/terrain/stone", "stone", 4),
                [TileType.Dirt] = LoadTextures(content, "Tiles/terrain/dirt", "dirt", 4)
            };

            var ventanas = new (string file, int offsetX, int offsetY)[]
            {
                ("ventana1.txt", 0, 0),     ("ventana2.txt", 85, 0),     ("ventana3.txt", 170, 0),
                ("ventana4.txt", 0, 45),    ("ventana5.txt", 85, 45),    ("ventana6.txt", 170, 45),
                ("ventana7.txt", 0, 90),    ("ventana8.txt", 85, 90),    ("ventana9.txt", 170, 90)
            };

            foreach (var (file, offsetX, offsetY) in ventanas)
            {
                using var stream = TitleContainer.OpenStream(Path.Combine("Content", "Maps", file));
                using var reader = new StreamReader(stream);

                int y = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    for (int x = 0; x < tokens.Length && offsetX + x < _tiles.GetLength(0); x++)
                    {
                        if (offsetY + y < _tiles.GetLength(1))
                        {
                            TileType type = CharToTile(tokens[x][0]);
                            _tiles[offsetX + x, offsetY + y] = type;

                            if (_tileTextures.ContainsKey(type))
                            {
                                int textureCount = _tileTextures[type].Count;
                                _tileTextureIndices[offsetX + x, offsetY + y] = _rng.Next(textureCount);
                            }
                            else
                            {
                                _tileTextureIndices[offsetX + x, offsetY + y] = 0;
                            }
                        }
                    }
                    y++;
                }
            }
        }

        private List<Texture2D> LoadTextures(ContentManager content, string folder, string prefix, int count)
        {
            var list = new List<Texture2D>();
            for (int i = 1; i <= count; i++)
                list.Add(content.Load<Texture2D>($"{folder}/{prefix}_{i}"));
            return list;
        }

        private TileType CharToTile(char c) => c switch
        {
            'G' => TileType.Grass,
            'P' => TileType.Path,
            'W' => TileType.Water,
            'D' => TileType.Dirt,
            'S' => TileType.Stone,
            _ => TileType.Empty
        };

        public TileType GetTileType(int x, int y)
        {
            if (x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1))
                return _tiles[x, y];
            return TileType.Empty;
        }

        private Texture2D GetTileTexture(TileType tile, int index)
        {
            if (_tileTextures.TryGetValue(tile, out var list) && index >= 0 && index < list.Count)
                return list[index];
            return null;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera, int screenWidth, int screenHeight)
        {
            int tilesWide = screenWidth / _tileSize + 2;
            int tilesHigh = screenHeight / _tileSize + 2;

            int startX = (int)(camera.X / _tileSize);
            int startY = (int)(camera.Y / _tileSize);

            for (int y = startY; y < startY + tilesHigh; y++)
            {
                for (int x = startX; x < startX + tilesWide; x++)
                {
                    if (x < 0 || y < 0 || x >= _tiles.GetLength(0) || y >= _tiles.GetLength(1))
                        continue;

                    int textureIndex = _tileTextureIndices[x, y];
                    var texture = GetTileTexture(_tiles[x, y], textureIndex);

                    if (texture != null)
                    {
                        spriteBatch.Draw(texture,
                            new Vector2(x * _tileSize - camera.X, y * _tileSize - camera.Y),
                            Color.White);
                    }
                }
            }
        }
    }
}
