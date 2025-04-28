using ShadowSky.Entities;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Vegetation;

public class Vine : WorldObject
{
    public Vine(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Vine";
        TextureName = "World/Vegetation/Vine";
    }

    public override void OnInteract(Player player)
    {
        // Ejemplo: Cortar una enredadera para obtener cuerda
        Console.WriteLine("You harvested a vine and obtained rope material!");
    }
}
