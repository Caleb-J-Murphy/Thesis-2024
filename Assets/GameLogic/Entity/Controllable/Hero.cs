using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Hero : Controllable
{
    public Board board;

    private int maxHealth = 15;
    private int health;
    private int initialHealth;

    public event Action<int> OnHealthChanged;

    public int Health
    {
        get { return health; }
        private set
        {
            health = value;
            OnHealthChanged?.Invoke(health);
        }
    }

    private int maxStamina = 15;
    private int stamina;
    private int initialStamina;

    public event Action<int> OnStaminaChanged;

    public int Stamina
    {
        get { return stamina; }
        private set
        {
            stamina = value;
            OnStaminaChanged?.Invoke(stamina);
        }
    }

    private Vector2[] directions = new Vector2[]
    {
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, -1)
    };

    public void Initialise(int initialHealth, int initialStamina)
    {
        Health = initialHealth;
        this.initialHealth = initialHealth;
        Stamina = initialStamina;
        this.initialStamina = initialStamina;
        
        maxHealth = initialHealth;
        maxStamina = initialStamina;


        //Temporary and just for testing, will remove
        Coin coin = new Coin();
        AddToInventory(coin);
    }

    public override void Reset() {
        base.Reset();
        Initialise(initialHealth, initialStamina);
        
        OnHealthChanged?.Invoke(health);
    }

    public override string getName() {
        return "hero";
    }

    private bool isWallAtDirection(Vector2 direction) {
        Vector2 pos = getPosition() + direction;
        List<Entity>entities = board.getEntitisAt(pos);
        foreach (Entity ent in entities) {
            if (ent is Wall) {
                return true;
            }
        }
        return false;
    }
    
    public override void moveUp() {
        if (!isWallAtDirection(Vector2.up)) {
            setPosition(getPosition() + Vector2.up);
        }
        
    }

    public override void moveDown() {
        if (!isWallAtDirection(Vector2.down)) {
            setPosition(getPosition() + Vector2.down);
        }
    }

    public override void moveLeft() {
        if (!isWallAtDirection(Vector2.left)) {
            setPosition(getPosition() + Vector2.left);
        }
    }

    public override void moveRight() {
        if (!isWallAtDirection(Vector2.right)) {
            setPosition(getPosition() + Vector2.right);
        }
    }

    public void turnLeft() {
        transform.Rotate(0, 0, 90); // Rotate counterclockwise by 90 degrees
    }

    public void turnRight() {
        transform.Rotate(0, 0, -90); // Rotate clockwise by 90 degrees
    }

    public void moveForward(int steps)
    {
        Debug.Log("Running moveForward");
        for (int i = 0; i < steps; i++) {
            moveUp();
        }
        Debug.Log($"Just moved to position: {getPosition()}");
    }

    public bool isTouchingWall()
    {
        Debug.Log($"Checking touching wall = {isTouching<Wall>()}");

        return isTouching<Wall>();
    }

    public bool isTouchingMine()
    {
        return isTouching<Mine>();
    }

    private bool isTouching<T>() where T : Entity
    {
        foreach (Vector2 dir in directions)
        {
            if (board == null) Debug.LogError("Board has not been set within the hero");
            List<Entity> entities = board.getEntitisAt(getPosition() + dir);
            foreach (Entity entity in entities)
            {
                T specificEntity = entity as T;
                if (specificEntity != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void PickUpItem()
    {
        board.checkPlayerPickup();
    }


    public void TakeDamage(int damage)
    {
        Health = Mathf.Max(0, Health - damage);
    }

    public void UseStamina(int amount)
    {
        Stamina = Mathf.Max(0, Stamina - amount);
    }

    public void Heal(int amount)
    {
        Health = Mathf.Min(maxHealth, Health + amount);
    }

    public void RegainStamina(int amount)
    {
        Stamina = Mathf.Min(maxStamina, Stamina + amount);
    }

    public int getHealth() {
        return Health;
    }

    public int getStamina() {
        return Stamina;
    }

    public int getMaxHealth() {
        return maxHealth;
    }

    public int getMaxStamina() {
        return maxStamina;
    }
    
}
