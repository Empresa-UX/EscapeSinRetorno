using System;

namespace ShadowSky.WorldObjects.Interactables;

public class Campfire : WorldObject
{
    public Campfire()
    {
        Name = "Campfire";
        TextureName = "World/Interactables/Campfire"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You warmed yourself by the campfire.");
    }
}
