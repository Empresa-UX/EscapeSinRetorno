using System;

namespace ShadowSky.WorldObjects.Interactables;

public class FishingSpot : WorldObject
{
    public FishingSpot()
    {
        Name = "Fishing Spot";
        TextureName = "World/Interactables/FishingSpot"; // Ruta de la imagen en Content
    }

    public override void OnInteract()
    {
        Console.WriteLine("You caught a fish!");
    }
}
