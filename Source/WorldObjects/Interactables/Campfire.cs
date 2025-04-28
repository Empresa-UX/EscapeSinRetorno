using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Interactables;

public class Campfire : WorldObject
{
    public Campfire(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Campfire";
        TextureName = "World/Interactables/Campfire";
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You warmed yourself by the campfire.");
    }
}
