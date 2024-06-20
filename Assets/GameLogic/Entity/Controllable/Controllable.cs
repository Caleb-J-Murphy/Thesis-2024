using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class Controllable : Entity
{
    private Dictionary<Collectable, int> inventory = new Dictionary<Collectable, int>();
    public event Action<Dictionary<Collectable, int>> OnInventoryChanged;
    
    public Dictionary<Collectable, int> Inventory
    {
        get { return inventory; }
        private set
        {
            inventory = value;
            OnInventoryChanged?.Invoke(inventory);
        }
    }

    public override string getName() {
        return "controllable";
    }

    public Dictionary<Collectable, int> getInventory() {
        return inventory;
    }

    public void AddToInventory(Collectable item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]++;
        }
        else
        {
            inventory[item] = 1;
        }
        OnInventoryChanged?.Invoke(inventory);
    }

    public Collectable TakeFromInventory(string itemName)
    {
        foreach (Collectable item in inventory.Keys) {
            if (item.getName() == itemName) {
                if (inventory[item] > 0)
                {
                    inventory[item]--;
                    if (inventory[item] == 0)
                    {
                        inventory.Remove(item);
                    }
                    OnInventoryChanged?.Invoke(inventory);
                    Debug.Log($"Found the {itemName} in inventory");
                    return item;
                }
                Debug.LogError("The item held a negative value in the inventory somehow");
                break;
            }
        }
        return null;
    }

    public abstract void moveUp();

    public abstract void moveDown();

    public abstract void moveLeft();

    public abstract void moveRight();

    public override void Reset() {
        Debug.Log($"Reseting position of player to ${originalPosition}");
        base.Reset();
        inventory = new Dictionary<Collectable, int>();
    }

}
