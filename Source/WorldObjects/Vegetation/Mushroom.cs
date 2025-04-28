using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Vegetation;

public class Mushroom : WorldObject
{
    public Mushroom(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Mushroom";
        TextureName = "World/Vegetation/Mushroom";
    }

    public override void OnInteract(Player player)
    {
        // Ejemplo: Recoger un hongo (puede ser útil o venenoso en futuro)
        Console.WriteLine("You collected a mushroom.");
    }
}
