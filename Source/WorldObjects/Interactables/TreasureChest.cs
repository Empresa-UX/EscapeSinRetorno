using System;

namespace ShadowSky.WorldObjects.Interactables;

public class TreasureChest : WorldObject
{
    public TreasureChest()
    {
        Name = "Treasure Chest";
        TextureName = "World/Interactables/TreasureChest"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You opened the chest and found some loot!");
    }
}
