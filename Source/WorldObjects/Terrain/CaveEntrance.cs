using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Terrain;

public class CaveEntrance : WorldObject
{
    public CaveEntrance(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Cave Entrance";
        TextureName = "World/Terrain/CaveEntrance"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You entered the cave...");
    }
}
