using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Vegetation;

public class Flower : WorldObject
{
    public Flower(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Flower";
        TextureName = "World/Vegetation/Flower";
    }

    public override void OnInteract(Player player)
    {
        // Ejemplo: Recoger flores
        Console.WriteLine("You picked a beautiful flower!");
    }
}
