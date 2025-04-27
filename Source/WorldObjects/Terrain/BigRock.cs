using System;

namespace ShadowSky.WorldObjects.Terrain;

public class BigRock : WorldObject
{
    public BigRock()
    {
        Name = "Big Rock";
        TextureName = "World/Terrain/BigRock"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("The big rock is too heavy to move...");
    }
}
