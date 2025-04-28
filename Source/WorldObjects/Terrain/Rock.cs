using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Terrain;

public class Rock : WorldObject
{
    public Rock(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Rock";
        TextureName = "World/Terrain/Rock"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You collected some stone from the rock!");
    }
}
