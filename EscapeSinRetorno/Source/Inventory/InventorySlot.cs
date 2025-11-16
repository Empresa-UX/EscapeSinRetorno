namespace EscapeSinRetorno.Source.Inventory
{
    public class InventorySlot
    {
        public Item Item;
        public int Count;
        public bool IsEmpty => Item == null || Count <= 0;
    }
}