using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Interactables;

public class FishingSpot : WorldObject
{
    public FishingSpot(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Fishing Spot";
        TextureName = "World/Interactables/FishingSpot"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You caught a fish!");
    }
}
