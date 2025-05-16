// File: Source/World/TileMap.cs

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
        private readonly List<Tile> _tiles = new();
        private readonly Dictionary<string, Texture2D> _tileTextures = new();
        private readonly Dictionary<(int x, int y), string> _doorOverrides = new();
        private readonly Random _rng = new();
        private string[][] _mapData;
        public Vector2? PlayerStartPosition { get; private set; } = null;


        public TileMap(int tileSize)
        {
            _tileSize = tileSize;
        }

        public void LoadContent(ContentManager content)
        {
            LoadTileTextures(content);
            LoadMapFromFile("Content/Maps/v1.txt");
            PreprocessDoors();
            BuildTileInstances();
        }

        private void LoadTileTextures(ContentManager content)
        {
            for (int i = 1; i <= 12; i++)
                _tileTextures[$"F{i}"] = content.Load<Texture2D>($"Tiles/floors/floor_{i}");

            _tileTextures["N"] = content.Load<Texture2D>("Tiles/nothing");

            foreach (var type in new[] { "with_floor", "with_nothing" })
            {
                foreach (var dir in new[] { "wall_up", "wall_down", "wall_left", "wall_right" })
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        string key = $"{type}_{dir}_{i}";
                        string path = $"Tiles/walls/{type}/{dir}/wall_{i}";
                        try { _tileTextures[key] = content.Load<Texture2D>(path); } catch { }
                    }
                }
            }

            foreach (var dir in new[] { "door_normal", "door_side" })
            {
                for (int i = 1; i <= 2; i++)
                {
                    string key = $"{dir}_door_{i}";
                    string path = $"Tiles/doors/{dir}/door_{i}";
                    try { _tileTextures[key] = content.Load<Texture2D>(path); } catch { }
                }
            }
        }

        private void LoadMapFromFile(string relativePath)
        {
            using var stream = TitleContainer.OpenStream(relativePath);
            using var reader = new StreamReader(stream);

            var lines = new List<string>();
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine());

            _mapData = new string[lines.Count][];
            for (int y = 0; y < lines.Count; y++)
            {
                var tokens = lines[y].Split(',', StringSplitOptions.RemoveEmptyEntries);
                _mapData[y] = new string[tokens.Length];
                for (int x = 0; x < tokens.Length; x++)
                    _mapData[y][x] = tokens[x].Trim();
            }
        }

        private void PreprocessDoors()
        {
            for (int y = 0; y < _mapData.Length; y++)
            {
                for (int x = 0; x < _mapData[y].Length; x++)
                {
                    if (_mapData[y][x] != "D") continue;

                    if (IsValid(x + 1, y) && _mapData[y][x + 1] == "D")
                    {
                        _doorOverrides[(x, y)] = "door_normal_door_1";
                        _doorOverrides[(x + 1, y)] = "door_normal_door_2";
                    }
                    else if (IsValid(x, y + 1) && _mapData[y + 1][x] == "D")
                    {
                        _doorOverrides[(x, y)] = "door_side_door_2";
                        _doorOverrides[(x, y + 1)] = "door_side_door_1";
                    }
                }
            }
        }

        private void BuildTileInstances()
        {
            _tiles.Clear();

            for (int y = 0; y < _mapData.Length; y++)
            {
                for (int x = 0; x < _mapData[y].Length; x++)
                {
                    var layers = new List<Texture2D>();
                    string code = _mapData[y][x];

                    if (code == "F")
                    {
                        string floorCode = $"F{_rng.Next(1, 13)}";
                        layers.Add(_tileTextures[floorCode]);
                    }
                    else if (code == "W")
                    {
                        string context = HasFloorNeighbor(x, y) ? "with_floor" : "with_nothing";
                        string dir = DetectWallDirection(x, y);
                        string variant = DetectWallVariant(x, y, dir);
                        string key = $"{context}_{dir}_{variant}";
                        if (_tileTextures.TryGetValue(key, out var tex))
                            layers.Add(tex);
                    }
                    else if (code == "D")
                    {
                        if (_doorOverrides.TryGetValue((x, y), out var key) && _tileTextures.TryGetValue(key, out var tex))
                        {
                            layers.Add(tex);
                        }
                    }
                    else if (_tileTextures.TryGetValue(code, out var tex))
                    {
                        layers.Add(tex);
                    }
                    else if (code == "P")
                    {
                        string floorCode = $"F{_rng.Next(1, 13)}";
                        layers.Add(_tileTextures[floorCode]);
                        PlayerStartPosition = new Vector2(x * _tileSize * 2, y * _tileSize * 2);
                    }


                    if (layers.Count > 0)
                    {
                        var pos = new Vector2(x * _tileSize * 2, y * _tileSize * 2);
                        _tiles.Add(new Tile(layers, pos));
                    }
                }
            }
        }

        private string DetectWallDirection(int x, int y)
        {
            int wCount = 0;
            for (int dx = -2; dx <= 2; dx++)
            {
                if (IsWall(x + dx, y)) wCount++;
            }

            if (wCount >= 3) return "wall_up";

            bool floorBelow = IsFloor(x, y + 1);
            bool floorLeft = IsFloor(x - 1, y);
            bool floorRight = IsFloor(x + 1, y);

            if (floorBelow && floorLeft) return "wall_left";
            if (floorBelow && floorRight) return "wall_right";
            if (floorBelow) return "wall_down";
            if (floorLeft) return "wall_left";
            if (floorRight) return "wall_right";

            return "wall_up";
        }

        private string DetectWallVariant(int x, int y, string dir)
        {
            if ((dir == "wall_left" || dir == "wall_right") && IsFloor(x, y + 1))
                return "3";

            bool above = IsWall(x, y - 1);
            bool below = IsWall(x, y + 1);
            if (above && below) return "2";

            return "1";
        }

        private void DrawNothingTile(SpriteBatch spriteBatch, Vector2 worldPos)
        {
            if (_tileTextures.TryGetValue("N", out var tex))
            {
                spriteBatch.Draw(tex, worldPos, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Vector2 camera, int screenWidth, int screenHeight)
        {
            if (!_tileTextures.TryGetValue("N", out var tex))
                return;

            int tileDrawSize = _tileSize * 2;

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


        private bool IsWall(int x, int y)
        {
            return IsValid(x, y) && _mapData[y][x] == "W";
        }

        private bool IsFloor(int x, int y)
        {
            return IsValid(x, y) && _mapData[y][x].StartsWith("F");
        }

        private bool HasFloorNeighbor(int x, int y)
        {
            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                if (IsFloor(x + dx, y + dy)) return true;
            return false;
        }

        private bool IsValid(int x, int y)
        {
            return y >= 0 && y < _mapData.Length && x >= 0 && x < _mapData[y].Length;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            int screenWidth = 1280;
            int screenHeight = 720;
            int tileDrawSize = _tileSize * 2;

            int minX = (int)(camera.X / tileDrawSize) - 1;
            int maxX = (int)((camera.X + screenWidth) / tileDrawSize) + 1;
            int minY = (int)(camera.Y / tileDrawSize) - 1;
            int maxY = (int)((camera.Y + screenHeight) / tileDrawSize) + 1;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 pos = new Vector2(x * tileDrawSize, y * tileDrawSize);

                    if (IsValid(x, y))
                    {
                        foreach (var tile in _tiles)
                        {
                            if ((int)tile.Position.X == (int)pos.X && (int)tile.Position.Y == (int)pos.Y)
                            {
                                tile.Draw(spriteBatch, camera);
                                break;
                            }
                        }
                    }
                    else
                    {
                        DrawNothingTile(spriteBatch, pos); // <- usar pos ya creado
                    }
                }
            }
        }



        public bool IsColliding(Vector2 position, int width, int height)
        {
            int leftTile = (int)(position.X / (_tileSize * 2));
            int rightTile = (int)((position.X + width) / (_tileSize * 2));
            int topTile = (int)(position.Y / (_tileSize * 2));
            int bottomTile = (int)((position.Y + height) / (_tileSize * 2));

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (!IsValid(x, y))
                        continue;

                    string tileCode = _mapData[y][x];
                    if (tileCode == "W" || tileCode == "D")
                        return true;
                }
            }
            return false;
        }

    }
}