namespace ShadowSky.WorldObjects.Vegetation;

public class Mushroom : WorldObject
{
    public Mushroom()
    {
        Name = "Mushroom";
        TextureName = "World/Vegetation/Mushroom";
    }

    public override void OnInteract()
    {
        // Ejemplo: Recoger un hongo (puede ser útil o venenoso en futuro)
        Console.WriteLine("You collected a mushroom.");
    }
}
