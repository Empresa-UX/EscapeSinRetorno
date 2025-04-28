using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Terrain;

public class BigRock : WorldObject
{
    public BigRock(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Big Rock";
        TextureName = "World/Terrain/BigRock"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("The big rock is too heavy to move...");
    }
}
