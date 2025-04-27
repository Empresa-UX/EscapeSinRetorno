using System.Collections.Generic; // Necesario para usar Dictionary

public class Inventory
{
    private Dictionary<string, int> items = new();

    public void AddItem(string itemName, int amount)
    {
        if (items.ContainsKey(itemName))
            items[itemName] += amount;
        else
            items[itemName] = amount;
    }
}
