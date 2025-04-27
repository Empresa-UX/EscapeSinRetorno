using System;

namespace ShadowSky.WorldObjects.Terrain;

public class Rock : WorldObject
{
    public Rock()
    {
        Name = "Rock";
        TextureName = "World/Terrain/Rock"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You collected some stone from the rock!");
    }
}
