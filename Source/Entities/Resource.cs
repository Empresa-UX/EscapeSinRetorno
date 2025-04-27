using Microsoft.Xna.Framework; // Necesario para trabajar con Vector2

namespace ShadowSky.Entities
{
    public class Resource
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public Vector2 Position { get; set; }

        public Resource(string name, int quantity, Vector2 position)
        {
            Name = name;
            Quantity = quantity;
            Position = position; // Almacena la posición del recurso en el mundo
        }

        public void Collect()
        {
            Quantity--; // Disminuye la cantidad del recurso cuando el jugador lo recoge
        }
    }
}
