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
        private Tile[,] _tiles;
        private readonly Dictionary<string, Texture2D> _tileTextures = new();
        private string[][] _mapData;
        public Vector2? PlayerStartPosition { get; private set; } = null;

        private HashSet<Point> _wallPositions = new(); // NUEVO


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
            for (int i = 1; i <= 16; i++)
                _tileTextures[$"F{i}"] = content.Load<Texture2D>($"Tiles/floors/floor_{i}");

            _tileTextures["N"] = content.Load<Texture2D>("Tiles/nothing");

            for (int i = 1; i <= 9; i++)
                _tileTextures[$"LR{i}"] = content.Load<Texture2D>($"Tiles/walls/wall_left_right/wall_{i}");

            for (int i = 1; i <= 3; i++)
                _tileTextures[$"UD{i}"] = content.Load<Texture2D>($"Tiles/walls/wall_up_down/wall_{i}");
        }

        private void LoadMapFromFile(string relativePath)
        {
            using var stream = TitleContainer.OpenStream(relativePath);
            using var reader = new StreamReader(stream);

            var lines = new List<string>();
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine());

            int expectedCols = 80;
            int rows = lines.Count;

            _mapData = new string[rows][];

            for (int y = 0; y < rows; y++)
            {
                var rawLine = lines[y].Trim(); // Elimina espacios o saltos
                var tokens = rawLine.Split(',', StringSplitOptions.None);

                _mapData[y] = tokens;
            }
        }
        private void BuildTileInstances()
        {
            int width = _mapData[0].Length;
            int height = _mapData.Length;
            _tiles = new Tile[width, height];

            Dictionary<int, int> floorCounters = new();
            _wallPositions.Clear();

            // 🔁 PRIMERA PASADA — pisos, jugador, y marcas de muros
            for (int y = 0; y < height; y++)
            {
                floorCounters[y] = 1;
                for (int x = 0; x < width; x++)
                {
                    var layers = new List<Texture2D>();
                    string code = _mapData[y][x].Trim().ToUpper();

                    if (code == "F")
                    {
                        int frame = floorCounters[y]++;
                        if (floorCounters[y] > 16) floorCounters[y] = 1;
                        layers.Add(_tileTextures[$"F{frame}"]);
                    }
                    else if (code == "P")
                    {
                        layers.Add(_tileTextures["F1"]);
                        PlayerStartPosition = new Vector2(x * _tileSize, y * _tileSize);
                    }
                    else if (code == "W")
                    {
                        _wallPositions.Add(new Point(x, y)); // 🧱 Marcar solo
                    }

                    if (layers.Count > 0)
                    {
                        var pos = new Vector2(x * _tileSize, y * _tileSize);
                        _tiles[x, y] = new Tile(layers, pos);
                    }
                }
            }

            // 🔁 SEGUNDA PASADA — generar los muros con contexto
            foreach (var point in _wallPositions)
            {
                int x = point.X;
                int y = point.Y;

                string wallKey = GetWallTextureKey(x, y);
                if (_tileTextures.TryGetValue(wallKey, out var tex))
                {
                    Vector2 pos = new Vector2(x * _tileSize, y * _tileSize);
                    _tiles[x, y] = new Tile(new List<Texture2D> { tex }, pos);
                }
            }

            int totalTiles = 0;
            for (int y = 0; y < _tiles.GetLength(1); y++)
            {
                for (int x = 0; x < _tiles.GetLength(0); x++)
                {
                    if (_tiles[x, y] != null)
                        totalTiles++;
                }
            }
        }

        private string GetWallTextureKey(int x, int y)
        {
            bool up = _wallPositions.Contains(new Point(x, y - 1));
            bool down = _wallPositions.Contains(new Point(x, y + 1));
            bool left = _wallPositions.Contains(new Point(x - 1, y));
            bool right = _wallPositions.Contains(new Point(x + 1, y));

            // 🌆 MUROS HORIZONTALES (fila) = wall_up_down (UD)
            if ((left || right) && !up && !down)
            {
                if (left && right) return "UD1";   // medio
                if (!left && right) return "UD3";  // inicio (izquierda)
                if (left && !right) return "UD2";  // fin (derecha)
            }

            // 🏢 MUROS VERTICALES (columna) = wall_left_right (LR)
            if ((up || down) && !left && !right)
            {
                if (up && down) return "LR1";     // medio
                if (!up && down) return "LR3";    // inicio (arriba)
                if (up && !down) return "LR2";    // fin (abajo)
            }

            // 🔀 ESQUINAS
            if (up && right && !left && !down) return "LR4";  // ↘ arriba→derecha
            if (up && left && !right && !down) return "LR5";  // ↙ arriba→izquierda
            if (down && right && !left && !up) return "LR6";  // ↗ abajo→derecha
            if (down && left && !right && !up) return "LR7";  // ↖ abajo→izquierda

            // ┻ ┳ T-splits verticales
            if (up && left && right && !down) return "LR9"; // T ⊤
            if (down && left && right && !up) return "LR8"; // T ⊥

            // ┼ CRUZ completa
            if (up && down && left && right) return "LR1";

            // 🌐 FALLBACKS
            if ((up || down) && !(left || right)) return "LR1";
            if ((left || right) && !(up || down)) return "UD1";

            // 😶 Si está solo
            return "LR1";
        }

        private bool IsWall(int x, int y)
        {
            return IsValid(x, y) && _mapData[y][x] == "W";
        }

        private bool IsValid(int x, int y)
        {
            return y >= 0 && y < _mapData.Length && x >= 0 && x < _mapData[y].Length;
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
                    if (IsValid(x, y) && _tiles[x, y] != null)
                    {
                        _tiles[x, y].Draw(spriteBatch, camera);
                    }
                    // no más DrawNothingTile aquí
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Vector2 camera, int screenWidth, int screenHeight)
        {
            if (!_tileTextures.TryGetValue("N", out var tex))
                return;

            int tileDrawSize = _tileSize * 1;

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
            int leftTile = (int)(position.X / (_tileSize));
            int rightTile = (int)((position.X + width) / (_tileSize));
            int topTile = (int)(position.Y / (_tileSize));
            int bottomTile = (int)((position.Y + height) / (_tileSize));

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (!IsValid(x, y)) continue;
                    string tileCode = _mapData[y][x];
                    if (tileCode == "W" || tileCode == "D") return true;
                }
            }
            return false;
        }
    }
}