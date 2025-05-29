// File: Source/World/TileMap.cs

using EscapeSinRetorno.Source.Entities.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace EscapeSinRetorno.Source.World
{
    public class TileMap
    {
        private readonly int _tileSize;
        private Tile[,] _tiles;
        private readonly Dictionary<string, Texture2D> _tileTextures = new();
        private string[][] _mapData;
        public Vector2? PlayerStartPosition { get; private set; } = null;
        public List<(EnemyType type, Vector2 position, string variant)> EnemySpawns { get; private set; } = new();


        public TileMap(int tileSize)
        {
            _tileSize = tileSize;
        }

        public void LoadContent(ContentManager content)
        {
            LoadTileTextures(content);
            LoadMapFromFile("Content/Maps/test.txt");
            BuildTileInstances();
        }

        private void LoadTileTextures(ContentManager content)
        {
            for (int i = 1; i <= 16; i++)
                _tileTextures[$"F{i}"] = content.Load<Texture2D>($"Tiles/floors/floor_{i}");

            _tileTextures["N"] = content.Load<Texture2D>("Tiles/nothing");

            for (int i = 1; i <= 9; i++)
                _tileTextures[$"W{i}"] = content.Load<Texture2D>($"Tiles/walls/wall_left_right/wall_{i}");

            for (int i = 10; i <= 12; i++)
                _tileTextures[$"W{i}"] = content.Load<Texture2D>($"Tiles/walls/wall_up_down/wall_{i}");
        }

        private void LoadMapFromFile(string relativePath)
        {
            using var stream = TitleContainer.OpenStream(relativePath);
            using var reader = new StreamReader(stream);

            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))  // 🔍 filtrar líneas vacías
                    lines.Add(line);
            }

            Console.WriteLine($"📄 Cargadas {lines.Count} líneas desde el mapa");


            int rows = lines.Count;
            _mapData = new string[rows][];

            for (int y = 0; y < rows; y++)
            {
                var tokens = lines[y].Trim().Split(',', StringSplitOptions.None);
                _mapData[y] = new string[tokens.Length];
                for (int x = 0; x < tokens.Length; x++)
                    _mapData[y][x] = tokens[x].Trim().ToUpper();
            }
        }

        private void BuildTileInstances()
        {
            EnemySpawns.Clear();

            int width = _mapData[0].Length;
            int height = _mapData.Length;
            _tiles = new Tile[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var layers = new List<Texture2D>();
                    string code = _mapData[y][x];

                    if (code == "F")
                    {
                        int frame = (x % 4) + (y % 4) * 4 + 1;
                        layers.Add(_tileTextures[$"F{frame}"]);
                    }
                    else if (code == "P")
                    {
                        layers.Add(_tileTextures["F1"]);
                        PlayerStartPosition = new Vector2(x * _tileSize, y * _tileSize);
                    }
                    else if (code.Length >= 2 && code[0] == 'W')
                    {
                        string numeric = code.Substring(1).TrimStart('0');
                        if (numeric == "") numeric = "0";

                        if (int.TryParse(numeric, out int wallId))
                        {
                            string key = $"W{wallId}";

                            if (_tileTextures.TryGetValue(key, out var wallTex))
                            {
                                layers.Add(wallTex);
                                Console.WriteLine($"🧱 Tile ({x},{y}) usa textura '{key}' (wall_{wallId})");
                            }
                            else
                            {
                                Console.WriteLine($"❌ ERROR: No existe textura '{key}' para tile ({x},{y})");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ ERROR: Valor inválido de muro: '{code}' en ({x},{y})");
                        }
                    }
                    else if (code == "EW")
                    {
                        layers.Add(_tileTextures["F1"]);
                        EnemySpawns.Add((EnemyType.EvilWizard, new Vector2(x * _tileSize, y * _tileSize), ""));
                    }
                    else if (code == "NG")
                    {
                        layers.Add(_tileTextures["F1"]);
                        EnemySpawns.Add((EnemyType.NightBorne, new Vector2(x * _tileSize, y * _tileSize), ""));
                    }
                    else if (code == "MR" || code == "MB" || code == "MM")
                    {
                        layers.Add(_tileTextures["F1"]);
                        string variant = code switch
                        {
                            "MR" => "red",
                            "MB" => "blue",
                            "MM" => "magenta",
                            _ => "blue"
                        };
                        EnemySpawns.Add((EnemyType.MageGuardian, new Vector2(x * _tileSize, y * _tileSize), variant));
                    }



                    if (layers.Count > 0)
                    {
                        var pos = new Vector2(x * _tileSize, y * _tileSize);
                        _tiles[x, y] = new Tile(layers, pos);
                    }
                    if (layers.Count > 1)
                    {
                        Console.WriteLine($"⚠️ Tile ({x},{y}) tiene {layers.Count} capas: posibles conflictos.");
                    }
                }

            }
            Console.WriteLine($"✅ Generados {_tiles.Length} tiles ({_mapData.Length} filas × {_mapData[0].Length} columnas)");
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            int screenWidth = 1280;
            int screenHeight = 720;
            int tileDrawSize = _tileSize;

            int minX = (int)(camera.X / tileDrawSize) - 1;
            int maxX = (int)((camera.X + screenWidth) / tileDrawSize) + 1;
            int minY = (int)(camera.Y / tileDrawSize) - 1;
            int maxY = (int)((camera.Y + screenHeight) / tileDrawSize) + 1;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (y >= 0 && y < _tiles.GetLength(1) && x >= 0 && x < _tiles.GetLength(0) && _tiles[x, y] != null)
                        _tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Vector2 camera, int screenWidth, int screenHeight)
        {
            if (!_tileTextures.TryGetValue("N", out var tex)) return;

            int tileDrawSize = _tileSize;
            int minX = (int)(camera.X / tileDrawSize) - 1;
            int maxX = (int)((camera.X + screenWidth) / tileDrawSize) + 1;
            int minY = (int)(camera.Y / tileDrawSize) - 1;
            int maxY = (int)((camera.Y + screenHeight) / tileDrawSize) + 1;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 pos = new Vector2(x * tileDrawSize, y * tileDrawSize);
                    spriteBatch.Draw(tex, pos, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                }
            }
        }

        public bool IsColliding(Vector2 position, int width, int height)
        {
            int leftTile = (int)(position.X / _tileSize);
            int rightTile = (int)((position.X + width) / _tileSize);
            int topTile = (int)(position.Y / _tileSize);
            int bottomTile = (int)((position.Y + height) / _tileSize);

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (y >= 0 && y < _mapData.Length && x >= 0 && x < _mapData[y].Length)
                    {
                        string tileCode = _mapData[y][x];
                        if (tileCode.StartsWith("W") || tileCode == "D")
                            return true;
                    }
                }
            }
            return false;
        }
    }
}