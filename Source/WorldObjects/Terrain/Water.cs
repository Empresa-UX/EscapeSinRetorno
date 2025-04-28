using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Terrain;

public class Water : WorldObject
{
    public Water(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Water";
        TextureName = "World/Terrain/Water"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You filled your bottle with water!");
    }
}
