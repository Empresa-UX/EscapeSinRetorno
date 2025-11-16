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
        public int TileSize => _tileSize;

        public int Width => _mapData?.Length > 0 ? _mapData[0].Length : 0;
        public int Height => _mapData?.Length ?? 0;

        public Vector2? PlayerStartPosition { get; private set; } = null;
        public List<(EnemyType type, Vector2 position, string variant)> EnemySpawns { get; private set; } = new();

        // 👇 NUEVO: spawns de puertas
        public List<DoorSpawn> DoorSpawns { get; private set; } = new();

        public TileMap(int tileSize) => _tileSize = tileSize;

        public void LoadContent(ContentManager content)
        {
            LoadTileTextures(content);
            LoadMapFromFile("Content/Maps/vFinal.txt");
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
            using var reader = new StreamReader(TitleContainer.OpenStream(relativePath));
            var rows = new List<string[]>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var rawTokens = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < rawTokens.Length; i++)
                    rawTokens[i] = rawTokens[i].Trim().ToUpper();

                if (rawTokens.Length > 0)
                    rows.Add(rawTokens);
            }

            if (rows.Count == 0)
                throw new Exception("vFinal.txt no contiene datos de mapa válidos.");

            int height = rows.Count;
            int maxWidth = 0;
            for (int y = 0; y < height; y++)
                maxWidth = Math.Max(maxWidth, rows[y].Length);

            _mapData = new string[height][];
            for (int y = 0; y < height; y++)
            {
                _mapData[y] = new string[maxWidth];
                for (int x = 0; x < maxWidth; x++)
                {
                    if (x < rows[y].Length && !string.IsNullOrWhiteSpace(rows[y][x]))
                        _mapData[y][x] = rows[y][x];
                    else
                        _mapData[y][x] = "N";
                }
            }
        }

        private void BuildTileInstances()
        {
            EnemySpawns.Clear();
            DoorSpawns.Clear(); // 👈 NUEVO

            if (_mapData == null || _mapData.Length == 0) return;

            int height = _mapData.Length;
            int width = _mapData[0].Length;

            _tiles = new Tile[width, height];
            PlayerStartPosition = null;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    string code = _mapData[y][x];
                    var layers = new List<Texture2D>();
                    var pos = new Vector2(x * _tileSize, y * _tileSize);
                    var spawnOffset = new Vector2(_tileSize * 0.5f, _tileSize);

                    switch (code)
                    {
                        case "F":
                            layers.Add(_tileTextures[$"F{(x % 4) + (y % 4) * 4 + 1}"]);
                            break;

                        case "P":
                            layers.Add(_tileTextures["F1"]);
                            PlayerStartPosition = pos;
                            break;

                        case "EW":
                            layers.Add(_tileTextures["F1"]);
                            EnemySpawns.Add((EnemyType.EvilWizard, pos + spawnOffset, ""));
                            break;

                        case "NG":
                            layers.Add(_tileTextures["F1"]);
                            EnemySpawns.Add((EnemyType.NightBorne, pos + spawnOffset, ""));
                            break;

                        case "MR":
                        case "MB":
                        case "MM":
                            layers.Add(_tileTextures["F1"]);
                            string variant = code switch
                            {
                                "MR" => "red",
                                "MB" => "blue",
                                "MM" => "magenta",
                                _ => "blue"
                            };
                            EnemySpawns.Add((EnemyType.MageGuardian, pos + spawnOffset, variant));
                            break;

                        // 👇 PUERTAS
                        case "DC": // cyan
                            layers.Add(_tileTextures["F1"]);
                            DoorSpawns.Add(new DoorSpawn(DoorType.Cyan, pos + new Vector2(_tileSize * 0.5f, _tileSize)));
                            break;

                        case "DP": // purple
                            layers.Add(_tileTextures["F1"]);
                            DoorSpawns.Add(new DoorSpawn(DoorType.Purple, pos + new Vector2(_tileSize * 0.5f, _tileSize)));
                            break;

                        case "DR": // red
                            layers.Add(_tileTextures["F1"]);
                            DoorSpawns.Add(new DoorSpawn(DoorType.Red, pos + new Vector2(_tileSize * 0.5f, _tileSize)));
                            break;

                        case "DF": // final
                            layers.Add(_tileTextures["F1"]);
                            DoorSpawns.Add(new DoorSpawn(DoorType.Final, pos + new Vector2(_tileSize * 0.5f, _tileSize)));
                            break;

                        default:
                            if (code.StartsWith("W") &&
                                int.TryParse(code[1..].TrimStart('0'), out int wallId) &&
                                _tileTextures.TryGetValue($"W{wallId}", out var wallTex))
                            {
                                layers.Add(wallTex);
                            }
                            break;
                    }

                    if (layers.Count > 0)
                        _tiles[x, y] = new Tile(layers, pos);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraWorld)
        {
            var (minX, maxX, minY, maxY) = GetVisibleBounds(cameraWorld, 1280, 720);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsValidTile(x, y) && _tiles[x, y] != null)
                        _tiles[x, y].Draw(spriteBatch);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Vector2 cameraWorld, int screenWidth, int screenHeight)
        {
            if (!_tileTextures.TryGetValue("N", out var tex)) return;
            var (minX, maxX, minY, maxY) = GetVisibleBounds(cameraWorld, screenWidth, screenHeight);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    spriteBatch.Draw(
                        tex,
                        new Vector2(x * _tileSize, y * _tileSize),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        2f,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private (int minX, int maxX, int minY, int maxY) GetVisibleBounds(Vector2 cameraWorld, int screenWidth, int screenHeight)
        {
            int minX = (int)(cameraWorld.X / _tileSize) - 1;
            int maxX = (int)((cameraWorld.X + screenWidth) / _tileSize) + 1;
            int minY = (int)(cameraWorld.Y / _tileSize) - 1;
            int maxY = (int)((cameraWorld.Y + screenHeight) / _tileSize) + 1;
            return (minX, maxX, minY, maxY);
        }

        private bool IsValidTile(int x, int y)
        {
            return _tiles != null &&
                   y >= 0 && y < _tiles.GetLength(1) &&
                   x >= 0 && x < _tiles.GetLength(0);
        }

        public bool IsColliding(Vector2 position, int width, int height)
        {
            if (_mapData == null) return false;

            int leftTile = (int)(position.X / _tileSize);
            int rightTile = (int)((position.X + width) / _tileSize);
            int topTile = (int)(position.Y / _tileSize);
            int bottomTile = (int)((position.Y + height) / _tileSize);

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (y >= 0 && y < _mapData.Length &&
                        x >= 0 && x < _mapData[y].Length)
                    {
                        string tileCode = _mapData[y][x];
                        // De momento, SOLO paredes bloquean.
                        if (tileCode.StartsWith("W"))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
