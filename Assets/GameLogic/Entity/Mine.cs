using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Entity
{
    public bool isActivated = true;
    public int damage = 10;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Get the SpriteRenderer component of the first child GameObject
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found in any child GameObject.");
        }
    }

    public override string getName() {
        return "mine";
    }

    public void Activate()
    {
        isActivated = true;
        Debug.Log("Mine activated");
        spriteRenderer.color = Color.red;
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
            Deactivate();
            //Somehow need to change the color of the sprite beneath it to be blue
            spriteRenderer.color = Color.blue;
        }
    }
}
