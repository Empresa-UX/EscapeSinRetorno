namespace ShadowSky.WorldObjects.Vegetation;

public class Flower : WorldObject
{
    public Flower()
    {
        Name = "Flower";
        TextureName = "World/Vegetation/Flower";
    }

    public override void OnInteract()
    {
        // Ejemplo: Recoger flores
        Console.WriteLine("You picked a beautiful flower!");
    }
}
