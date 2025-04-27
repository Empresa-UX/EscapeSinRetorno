using Microsoft.Xna.Framework; // Necesario para trabajar con Vector2
using System.Collections.Generic; // Necesario para usar List

namespace ShadowSky.Entities
{
    public class Colossus
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public Player Player { get; set; } // Asegúrate de que esta propiedad esté bien definida

        public Colossus(string name, Vector2 position, Player player)
        {
            Name = name;
            Position = position;
            Player = player; // Se asigna el jugador a la propiedad
        }

        public void Interact()
        {
            // Lógica para interactuar con el coloso
        }
    }
}
