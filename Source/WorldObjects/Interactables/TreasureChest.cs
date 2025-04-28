using System;
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Interactables;

public class TreasureChest : WorldObject
{
    public TreasureChest(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
    {
        Name = "Treasure Chest";
        TextureName = "World/Interactables/TreasureChest"; // Ruta de la imagen en Content
    }

    public override void OnInteract(Player player)
    {
        Console.WriteLine("You opened the chest and found some loot!");
    }
}
