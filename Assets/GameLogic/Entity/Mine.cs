using Amazon.Runtime.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Entity
{
    InputProcessor inputProcessor = InputProcessor.Instance;
    public bool isActivated = true;
    public int damage = 10;

    private SpriteRenderer spriteRenderer;

    public Sprite activated;
    public Sprite deActivated;

    void Awake()
    {
        // Get the SpriteRenderer component of the first child GameObject
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found in any child GameObject.");
        }
    }

    private void Update()
    {
        if (isActivated)
        {
            if (spriteRenderer != null && activated != null)
            {
                spriteRenderer.sprite = activated;
            } else {
                Debug.LogError("SpriteRenderer or sprite not assigned for the mine");
            }
        } else
        {
            if (spriteRenderer != null && deActivated != null)
            {
                spriteRenderer.sprite = deActivated;
            }
            else
            {
                Debug.LogError("SpriteRenderer or sprite not assigned for the mine");
            }
        }
    }

    public override string getName() {
        return "mine";
    }

    public void Activate()
    {
        isActivated = true;
        if (spriteRenderer != null && activated != null)
        {
            spriteRenderer.sprite = activated;
        } else {
            Debug.LogError("SpriteRenderer or sprite not assigned for the mine");
        }
    }

    public void Deactivate()
    {
        isActivated = false;
        if (spriteRenderer != null && deActivated != null)
        {
            spriteRenderer.sprite = deActivated;
        } else {
            Debug.LogError("SpriteRenderer or sprite not assigned for the mine");
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void Trigger(Hero hero)
    {
        if (isActivated)
        {
            Debug.Log("Mine triggered, dealing " + damage + " damage!");
            Debug.LogError("You can't go over a spike, you will die");
            inputProcessor.setPlay(false);
        }
    }
}
