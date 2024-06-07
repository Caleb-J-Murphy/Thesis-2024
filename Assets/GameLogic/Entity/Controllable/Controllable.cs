using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : Entity
{
    private List<Collectable> inventory = new List<Collectable>();

    

    public override string getName() {
        return "interactable";
    }

    public List<Collectable> getInventory() {
        return inventory;
    }

    public void AddToInventory(Collectable item) {
        inventory.Add(item);
    }

    public Collectable TakeFromInventory(string itemName) {
        foreach (Collectable item in inventory) {
            if (item.getName() == itemName) {
                inventory.Remove(item);
                return item;
            }
        }
        return null;
    }

    public abstract void moveUp();

    public abstract void moveDown();

    public abstract void moveLeft();

    public abstract void moveRight();

}
