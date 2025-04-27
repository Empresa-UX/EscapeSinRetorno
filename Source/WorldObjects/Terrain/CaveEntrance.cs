using System;

namespace ShadowSky.WorldObjects.Terrain;

public class CaveEntrance : WorldObject
{
    public CaveEntrance()
    {
        Name = "Cave Entrance";
        TextureName = "World/Terrain/CaveEntrance"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You entered the cave...");
    }
}
