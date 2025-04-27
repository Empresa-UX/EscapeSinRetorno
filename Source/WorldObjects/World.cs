using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ShadowSky.Entities;
using ShadowSky.WorldTiles;
using ShadowSky.WorldTiles.Ground;
using ShadowSky.WorldTiles.Water;
using ShadowSky.WorldTiles.Road;

namespace ShadowSky.World
{
    public class World
    {
        public const int TileSize = 32;

        private Tile[,] tiles;
        private int width;
        private int height;

        private Texture2D grassTexture;
        private Texture2D dirtTexture;
        private Texture2D stoneTexture;
        private Texture2D waterTexture;
        private Texture2D pathTexture;

        public List<Colossus> Colossuses { get; set; }
        public List<Resource> Resources { get; set; }

        public World(int width, int height)
        {
            this.width = width;
            this.height = height;
            tiles = new Tile[width, height];

            Colossuses = new List<Colossus>();
            Resources = new List<Resource>();
        }

        public void LoadContent(ContentManager content)
        {
            // 🚀 Cargamos todas las texturas necesarias
            grassTexture = content.Load<Texture2D>("Images/World/Tiles/32/GrassTile");
            dirtTexture = content.Load<Texture2D>("Images/World/Tiles/32/DirtTile");
            stoneTexture = content.Load<Texture2D>("Images/World/Tiles/32/StoneTile");
            waterTexture = content.Load<Texture2D>("Images/World/Tiles/32/WaterTile");
            pathTexture = content.Load<Texture2D>("Images/World/Tiles/32/PathTile");

            // 🔥 Una vez cargadas, generamos el mundo
            GenerateWorld();
            InitializeWorld();
        }

private void GenerateWorld()
{
    Random random = new Random();

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            int tileType = random.Next(0, 5); // 0 a 4

            switch (tileType)
            {
                case 0:
                    tiles[x, y] = new GrassTile();
                    break;
                case 1:
                    tiles[x, y] = new DirtTile();
                    break;
                case 2:
                    tiles[x, y] = new StoneTile();
                    break;
                case 3:
                    tiles[x, y] = new WaterTile();
                    break;
                case 4:
                    tiles[x, y] = new PathTile();
                    break;
            }
        }
    }
}


        private void InitializeWorld()
        {
            Player player = new Player(new Vector2(100, 100));

            Colossus colossus = new Colossus("Colossus1", new Vector2(200, 200), player);
            AddColossus(colossus);

            Resource resource = new Resource("Wood", 10, new Vector2(150, 150));
            AddResource(resource);
        }

        public void AddColossus(Colossus colossus)
        {
            Colossuses.Add(colossus);
        }

        public void AddResource(Resource resource)
        {
            Resources.Add(resource);
        }

        public void Draw()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int posX = x * TileSize;
                    int posY = y * TileSize;

                    tiles[x, y].Draw(posX, posY);
                }
            }
        }
    }
}
