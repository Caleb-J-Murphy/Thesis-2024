using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Entity
{
    public bool isActivated;
    public int damage;

    public bool isBlownUp;

    public Mine()
    {
        isActivated = false;
        damage = 10; // Default damage value, you can change it as needed.
    }

    public override string getName() {
        return "mine";
    }

    public void Activate()
    {
        isActivated = true;
        Debug.Log("Mine activated");
    }

    public void Deactivate()
    {
        isActivated = false;
        Debug.Log("Mine deactivated");
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
        Debug.Log("Mine damage set to: " + damage);
    }

    public void Trigger(Hero hero)
    {
        if (isActivated)
        {
            Debug.Log("Mine triggered, dealing " + damage + " damage!");
            hero.TakeDamage(damage);
        }
    }
}
