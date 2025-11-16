using EscapeSinRetorno.Source.Items;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Inventory
{
    public class Item
    {
        public string Id { get; }
        public string Name { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
        public ItemType Type { get; }

        public Texture2D Icon { get; set; }

        public Item(string id, string name, bool stackable, int maxStack, ItemType type)
        {
            Id = id;
            Name = name;
            Stackable = stackable;
            MaxStack = maxStack;
            Type = type;
        }
    }
}