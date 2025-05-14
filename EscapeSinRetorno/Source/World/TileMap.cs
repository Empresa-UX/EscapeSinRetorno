// TileMap.cs
// ESCAPE SIN RETORNO — Mapa con capas, contexto, dirección y escalado

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace EscapeSinRetorno.World
{
    public enum TileLayer { Base, Object }

    public class TileMap
    {
        private readonly int _tileSize;
        private readonly List<Tile> _tiles = new();
        private readonly Dictionary<string, Texture2D> _tileTextures = new();
        private readonly Random _rng = new();
        private string[][] _mapData;

        public TileMap(int tileSize)
        {
            _tileSize = tileSize;
        }

        public void LoadContent(ContentManager content)
        {
            LoadTileTextures(content);
            LoadMapFromFile("Content/Maps/v1.txt");
            BuildTileInstances();
        }

        private void LoadTileTextures(ContentManager content)
        {
            for (int i = 1; i <= 4; i++)
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

        private void BuildTileInstances()
        {
            for (int y = 0; y < _mapData.Length; y++)
            {
                for (int x = 0; x < _mapData[y].Length; x++)
                {
                    string code = _mapData[y][x];
                    var layers = new List<Texture2D>();
                    string floorCode = code == "F" ? $"F{_rng.Next(1, 5)}" : null;

                    if (floorCode != null)
                        layers.Add(_tileTextures[floorCode]);

                    if (code == "W")
                    {
                        string context = HasFloorNeighbor(x, y) ? "with_floor" : "with_nothing";
                        string dir = DetectWallDirection(x, y);
                        string variant = DetectWallVariant(x, y, dir);
                        string tileKey = $"{context}_{dir}_{variant}";
                        if (_tileTextures.ContainsKey(tileKey))
                            layers.Add(_tileTextures[tileKey]);
                    }
                    else if (_tileTextures.ContainsKey(code))
                    {
                        layers.Add(_tileTextures[code]);
                    }

                    if (layers.Count > 0)
                    {
                        var pos = new Vector2(x * _tileSize * 2, y * _tileSize * 2);
                        _tiles.Add(new Tile(layers, pos));
                    }
                }
            }
        }

        private bool HasFloorNeighbor(int x, int y)
        {
            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int nx = x + dx, ny = y + dy;
                if (ny >= 0 && ny < _mapData.Length && nx >= 0 && nx < _mapData[ny].Length)
                    if (_mapData[ny][nx].StartsWith("F")) return true;
            }
            return false;
        }

        private string DetectWallDirection(int x, int y)
        {
            bool up = IsFloor(x, y - 1);
            bool down = IsFloor(x, y + 1);
            bool left = IsFloor(x - 1, y);
            bool right = IsFloor(x + 1, y);

            if ((left && right) || (!up && (left || right))) return "wall_up";
            if ((up && down) || (!left && (up || down))) return "wall_left";
            if (down) return "wall_down";
            if (up) return "wall_up";
            if (left) return "wall_left";
            if (right) return "wall_right";

            return "wall_up";
        }

        private string DetectWallVariant(int x, int y, string dir)
        {
            return (dir == "wall_left" || dir == "wall_right") && IsFloor(x, y + 1) ? "3" : "1";
        }

        private bool IsFloor(int x, int y)
        {
            if (y >= 0 && y < _mapData.Length && x >= 0 && x < _mapData[y].Length)
                return _mapData[y][x].StartsWith("F");
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            foreach (var tile in _tiles)
                tile.Draw(spriteBatch, camera);
        }
    }
}