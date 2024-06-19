using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private int DoorID;

    public Sprite openDoor;
    public Sprite closedDoor;

    private Board board;
    public override string getName() {
        return "door";
    }

    public void Initialise(int DoorID) {
        this.DoorID = DoorID;
    }

    public int getID() {
        return DoorID;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Door other = (Door)obj;
        return getID().Equals(other.getID());
    }

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

    private Board getBoard() {
        return board;
    }

    public void setBoard(Board board) {
        this.board = board;
    }

    private bool unlocked;

    public bool isUnlocked() {
        return unlocked;
    }

    public void setUnlocked(bool unlocked) {
        this.unlocked = unlocked;
    }

    private void OpenDoor() {
        if (spriteRenderer != null && openDoor != null)
        {
            spriteRenderer.sprite = openDoor;
        } else {
            Debug.LogError("SpriteRenderer or sprite not assigned for the door");
        }
        setUnlocked(true);
    }

    private void LockDoor() {
        if (spriteRenderer != null && closedDoor != null)
        {
            spriteRenderer.sprite = closedDoor;
        } else {
            Debug.LogError("SpriteRenderer or sprite not assigned for the door");
        }
        setUnlocked(false);
    }

    public void useDoor()
    {
        List<Hero> heros = getBoard().getEntities<Hero>();
        foreach (Hero hero in heros)
        {
            if (getDistance(hero.getPosition()) <= 1.1)
            {
                // Check if the player has a key...
                Collectable key = hero.TakeFromInventory("key");
                if (!key)
                {
                    OpenDoor();
                    board.UpdateBoard();
                    return;
                }
                else if (!isUnlocked())
                {
                    LockDoor();
                    return;
                }
            }
        }
        return;
    }

    public override void Reset() {
        base.Reset();
        LockDoor();
    }
}
