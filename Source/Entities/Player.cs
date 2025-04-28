using System;
using Microsoft.Xna.Framework; // Necesario para trabajar con Vector2
using System.Collections.Generic; // Necesario para usar Dictionary

namespace ShadowSky.Entities
{
    public class Player
    {
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int Energy { get; set; }
        public Dictionary<string, int> Inventory { get; set; }

        public Player(Vector2 initialPosition)
        {
            Position = initialPosition;
            Health = 100;
            Energy = 100;
            Inventory = new Dictionary<string, int>(); // Aquí usamos Dictionary para almacenar los recursos
        }

        public void CollectResource(string resource)
        {
            if (Inventory.ContainsKey(resource))
                Inventory[resource]++;
            else
                Inventory.Add(resource, 1);
        }

        public void Move(Vector2 direction)
        {
            Position += direction; // Actualiza la posición del jugador
        }
    }
}
