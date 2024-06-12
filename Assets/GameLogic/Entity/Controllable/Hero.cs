using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Controllable
{
    public Board board;
    private int health;
    private int stamina;
    private Vector2[] directions = new Vector2[]
    {
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, -1)
    };

    public void Initialise(int initialHealth, int initialStamina)
    {
        health = initialHealth;
        stamina = initialStamina;
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
        } else {
            Debug.Log("You hit a wall...");
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
        } else {
            Debug.Log("You hit a wall");
        }
    }

    public void turnLeft() {
        transform.Rotate(0, 0, 90); // Rotate counterclockwise by 90 degrees
        Debug.Log("Turn Left");
    }

    public void turnRight() {
        transform.Rotate(0, 0, -90); // Rotate clockwise by 90 degrees
        Debug.Log("Turn Right");
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


    public void TakeDamage(int dmg)
    {
        health -= dmg;
    }

    public int getHealth() {
        return health;
    }

    public int getStamina() {
        return stamina;
    }
}
