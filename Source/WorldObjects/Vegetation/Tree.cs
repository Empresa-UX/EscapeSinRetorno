using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Vegetation;

public class Tree : WorldObject
{
    public Tree(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Tree";
        TextureName = "World/Vegetation/Tree";
    }

    public override void OnInteract(Player player)
    {
        // Ejemplo: Cortar madera
        Console.WriteLine("You chopped the tree and obtained wood!");
    }
}
