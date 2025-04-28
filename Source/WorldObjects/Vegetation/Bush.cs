using System;
using ShadowSky.WorldObjects; // Para poder usar WorldObject
using ShadowSky.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldObjects.Vegetation
{
    public class Bush : WorldObject
    {
        public Bush(Texture2D texture, Vector2 position)
        : base(texture, position, isCollidable: true)
        {
            Name = "Bush";
            TextureName = "World/Vegetation/Bush"; // Ruta de la imagen en Content
        }

        public override void OnInteract(Player player)
        {
            // Ejemplo: Al interactuar con un arbusto, obtienes bayas
            Console.WriteLine("You collected some berries from the bush!");
        }
    }
}
