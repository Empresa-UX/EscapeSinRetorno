using ShadowSky.WorldObjects; // Para poder usar WorldObject

namespace ShadowSky.WorldObjects.Vegetation
{
    public class Bush : WorldObject
    {
        public Bush()
        {
            Name = "Bush";
            TextureName = "World/Vegetation/Bush"; // Ruta de la imagen en Content
        }

        public override void OnInteract()
        {
            // Ejemplo: Al interactuar con un arbusto, obtienes bayas
            Console.WriteLine("You collected some berries from the bush!");
        }
    }
}
