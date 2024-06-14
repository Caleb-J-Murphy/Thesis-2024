using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectable : Entity
{

    private bool collected = false;
    private SpriteRenderer spriteRenderer;
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Collectable other = (Collectable)obj;
        return getName().Equals(other.getName());
    }

    public override int GetHashCode()
    {
        return getName().GetHashCode();
    }

    void Awake()
    {
        // Get the SpriteRenderer component of the first child GameObject
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found in any child GameObject.");
        }
    }

    public bool IsCollected() {
        return collected;
    }

    private void SetCollected(bool collected) {
        this.collected = collected;
    }

    public void Pickup()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        SetCollected(true);
    }

    public void Drop()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        SetCollected(false);
    }

    public override void Reset() {
        base.Reset();
        Drop();
    }
}
