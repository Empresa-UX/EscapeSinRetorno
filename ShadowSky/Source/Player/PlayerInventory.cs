using System.Collections.Generic;

namespace ShadowSky.Source.Player
{
    public class PlayerInventory
    {
        public List<string> Items { get; } = new List<string>();

        public void AddItem(string item)
        {
            Items.Add(item);
        }

        public bool HasItem(string item) => Items.Contains(item);
    }
}
