namespace ShadowSky.WorldObjects.Vegetation;

public class Tree : WorldObject
{
    public Tree()
    {
        Name = "Tree";
        TextureName = "World/Vegetation/Tree";
    }

    public override void OnInteract()
    {
        // Ejemplo: Cortar madera
        Console.WriteLine("You chopped the tree and obtained wood!");
    }
}
