using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : Entity
{
    private List<string> inventory;

    public List<string> getInventory() {
        return inventory;
    }

    public override string getName() {
        return "interactable";
    }
}
