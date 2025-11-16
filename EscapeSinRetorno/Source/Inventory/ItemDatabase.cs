using System.Collections.Generic;
using EscapeSinRetorno.Source.Items;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Inventory
{
    public static class ItemDatabase
    {
        public static readonly Dictionary<string, Item> Items = new();

        public static void Load(ContentManager content)
        {
            Items.Clear();

            // ⚠️ Asumo que tus PNG están en Content/Items/...
            // y que los nombres son EXACTOS:
            // main_key.png, cyan_key.png, purple_key.png, red_key.png,
            // watter_bottle.png, potion_life.png, meal.png

            // Llaves
            var mainKey = new Item("main_key", "Llave principal", false, 1, ItemType.Key);
            mainKey.Icon = content.Load<Texture2D>("Items/main_key");
            Items[mainKey.Id] = mainKey;

            var cyanKey = new Item("cyan_key", "Llave cian", false, 1, ItemType.Key);
            cyanKey.Icon = content.Load<Texture2D>("Items/cyan_key");
            Items[cyanKey.Id] = cyanKey;

            var purpleKey = new Item("purple_key", "Llave púrpura", false, 1, ItemType.Key);
            purpleKey.Icon = content.Load<Texture2D>("Items/purple_key");
            Items[purpleKey.Id] = purpleKey;

            var redKey = new Item("red_key", "Llave roja", false, 1, ItemType.Key);
            redKey.Icon = content.Load<Texture2D>("Items/red_key");
            Items[redKey.Id] = redKey;

            // Consumibles
            var water = new Item("watter_bottle", "Botella de agua", true, 5, ItemType.Water);
            water.Icon = content.Load<Texture2D>("Items/watter_bottle");
            Items[water.Id] = water;

            var potion = new Item("potion_life", "Poción de vida", true, 10, ItemType.Potion);
            potion.Icon = content.Load<Texture2D>("Items/potion_life");
            Items[potion.Id] = potion;

            var meal = new Item("meal", "Comida", true, 10, ItemType.Food);
            meal.Icon = content.Load<Texture2D>("Items/meal");
            Items[meal.Id] = meal;
        }
    }
}
