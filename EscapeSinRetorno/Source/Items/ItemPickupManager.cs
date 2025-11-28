using System.Collections.Generic;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.Items
{
    public sealed class ItemPickupManager
    {
        private readonly List<ItemPickup> _pickups = new();

        public IReadOnlyList<ItemPickup> Pickups => _pickups;

        public void AddPickup(ItemPickup pickup)
        {
            if (pickup != null)
                _pickups.Add(pickup);
        }

        public void RemovePickup(ItemPickup pickup)
        {
            _pickups.Remove(pickup);
        }

        public void Update(
            Player player,
            KeyboardState prev,
            KeyboardState cur)
        {
            if (player == null || player.Inventory == null)
                return;

            bool JustPressed(Keys k) =>
                cur.IsKeyDown(k) && !prev.IsKeyDown(k);

            // Solo intentamos recoger si se presionó R
            if (!JustPressed(Keys.R))
                return;

            var hb = player.GetHitbox();

            // Recorremos de atrás hacia adelante porque podemos eliminar
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var p = _pickups[i];

                if (!hb.Intersects(p.Bounds))
                    continue;

                // Intentar agregar al inventario
                if (!ItemDatabase.Items.TryGetValue(p.ItemId, out var itemDef))
                    continue; // id inválido

                bool ok = player.Inventory.AddItem(p.ItemId, 1);
                if (ok)
                {
                    // Podés loguear en el chat si querés (si se lo pasás por parámetro)
                    // chat?.AddSystemMessage($"Recogiste {itemDef.Name}.");
                    _pickups.RemoveAt(i);
                }
                else
                {
                    // chat?.AddErrorMessage("Inventario lleno, no se pudo recoger el ítem.");
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var p in _pickups)
                p.Draw(sb);
        }

        public void ClearAll()
        {
            _pickups.Clear();
        }

        // Opcional para /near ver pickups
        public IEnumerable<ItemPickup> GetPickupsInRadius(Vector2 center, float radius)
        {
            float r2 = radius * radius;
            foreach (var p in _pickups)
            {
                if (Vector2.DistanceSquared(center, p.Position) <= r2)
                    yield return p;
            }
        }
    }
}
