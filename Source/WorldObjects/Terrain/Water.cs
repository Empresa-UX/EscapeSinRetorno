using System;

namespace ShadowSky.WorldObjects.Terrain;

public class Water : WorldObject
{
    public Water()
    {
        Name = "Water";
        TextureName = "World/Terrain/Water"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You filled your bottle with water!");
    }
}
