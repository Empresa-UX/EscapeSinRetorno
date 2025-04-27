namespace ShadowSky.WorldObjects.Vegetation;

public class Vine : WorldObject
{
    public Vine()
    {
        Name = "Vine";
        TextureName = "World/Vegetation/Vine";
    }

    public override void OnInteract()
    {
        // Ejemplo: Cortar una enredadera para obtener cuerda
        Console.WriteLine("You harvested a vine and obtained rope material!");
    }
}
