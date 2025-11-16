using System;
using EscapeSinRetorno.Source.Items;

namespace EscapeSinRetorno.Source.Inventory
{
    public class PlayerInventory
    {
        // 8 columnas x 4 filas
        public InventorySlot[] Slots = new InventorySlot[32];

        public PlayerInventory()
        {
            for (int i = 0; i < Slots.Length; i++)
                Slots[i] = new InventorySlot();
        }

        public bool AddItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return true;
            if (!ItemDatabase.Items.TryGetValue(itemId, out var item)) return false;

            int remaining = amount;

            if (item.Stackable)
            {
                foreach (var slot in Slots)
                {
                    if (remaining == 0) break;
                    if (!slot.IsEmpty && slot.Item.Id == itemId && slot.Count < item.MaxStack)
                    {
                        int canTake = Math.Min(item.MaxStack - slot.Count, remaining);
                        slot.Count += canTake;
                        remaining -= canTake;
                    }
                }
            }

            foreach (var slot in Slots)
            {
                if (remaining == 0) break;
                if (slot.IsEmpty)
                {
                    int put = item.Stackable ? Math.Min(item.MaxStack, remaining) : 1;
                    slot.Item = item;
                    slot.Count = put;
                    remaining -= put;
                }
            }

            return remaining == 0;
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return true;

            int remaining = amount;
            int total = 0;
            foreach (var s in Slots)
                if (!s.IsEmpty && s.Item.Id == itemId)
                    total += s.Count;

            if (total < remaining) return false;

            for (int i = 0; i < Slots.Length && remaining > 0; i++)
            {
                var slot = Slots[i];
                if (slot.IsEmpty || slot.Item.Id != itemId) continue;

                int take = Math.Min(slot.Count, remaining);
                slot.Count -= take;
                remaining -= take;

                if (slot.Count <= 0)
                {
                    slot.Item = null;
                    slot.Count = 0;
                }
            }

            return true;
        }

        public bool UseAt(int index, Func<Item, bool> canUse, Action<Item> onUsed)
        {
            if (index < 0 || index >= Slots.Length) return false;
            var slot = Slots[index];
            if (slot.IsEmpty || slot.Item == null) return false;

            if (canUse != null && !canUse(slot.Item)) return false;

            slot.Count -= 1;
            var usedItem = slot.Item;
            if (slot.Count <= 0)
            {
                slot.Item = null;
                slot.Count = 0;
            }

            onUsed?.Invoke(usedItem);
            return true;
        }
    }
}