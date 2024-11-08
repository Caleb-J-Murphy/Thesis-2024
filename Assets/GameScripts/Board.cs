using Amazon.Runtime.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Entity> objects = new List<Entity>();

    public GameObject winScreen;
    public InputProcessor inputProcessor;

    public void AddEntity(Entity entity, Vector2 position) {
        Hero specificEntity = entity as Hero;
        if (specificEntity != null) {
            specificEntity.board = this;
        }
        entity.setPosition(position);
        objects.Add(entity);
    }

    public void UpdateBoard() {
        checkPlayerHitMines();
        hasWon();
    }

    /*
     * Called when the user presses the player button
     * This is used for random events that get set each time they user presses play
     */
    public void startPlay()
    {
        //Update the signs
        List<Sign> signs = getEntities<Sign>();
        foreach (Sign sign in signs)
        {
            //Set the random direction
            sign.currentDirection = sign.GetDirection();
        }
        updateMines();
    }

    private void updateMines()
    {
        List<Sign> signs = getEntities<Sign>();
        List<Mine> mines = getEntities<Mine>();

        foreach (Mine mine in mines)
        {
            bool isDeactivated = false;

            foreach (Sign sign in signs)
            {
                var minePos = mine.getPosition();
                var signPos = sign.getPosition();

                if (minePos.y == signPos.y)
                {
                    if ((minePos.x >= signPos.x && sign.currentDirection == "Right") ||
                        (minePos.x <= signPos.x && sign.currentDirection == "Left"))
                    {
                        isDeactivated = true;
                        break;
                    }
                }
                else if (minePos.x == signPos.x)
                {
                    if ((minePos.y >= signPos.y && sign.currentDirection == "Up") ||
                        (minePos.y <= signPos.y && sign.currentDirection == "Down"))
                    {
                        isDeactivated = true;
                        break;
                    }
                }
            }

            mine.isActivated = !isDeactivated;
        }
    }

    public void checkPlayerPickup()
    {
        List<Collectable> collectables = getEntities<Collectable>();
        List<Hero> heroes = getEntities<Hero>();
        foreach (Collectable collectable in collectables) {
            foreach (Hero hero in heroes) {
                if (hero.getPosition() == collectable.getPosition() && !collectable.IsCollected()) {
                    hero.AddToInventory(collectable);
                    collectable.Pickup();
                }
            }
        }
    }

    public void checkPlayerHitMines()
    {
        List<Mine> mines = getEntities<Mine>();
        List<Hero> heroes = getEntities<Hero>();
        foreach (Mine mine in mines)
        {
            if (mine.isActivated)
            {
                foreach (Hero hero in heroes)
                {
                    if (hero.getPosition() == mine.getPosition())
                    {
                        mine.Trigger(hero);
                    }
                }
            }
        }
    }

    public List<T> getEntities<T>() where T : Entity
    {
        List<T> entities = new List<T>();
        foreach (Entity entity in objects)
        {
            T specificEntity = entity as T;
            if (specificEntity != null)
            {
                entities.Add(specificEntity);
            }
        }
        return entities;
    }

    public List<Entity> getEntitisAt(Vector2 pos) {
        List<Entity> entities = new List<Entity>();
        foreach(Entity entity in objects) {
            if (entity.getPosition() == pos) {
                entities.Add(entity);
            }
        }
        return entities;
    }

    public virtual bool hasWon() {
        return false;
    }

    protected void setWinScreen(bool win) {
        if (!winScreen) {
            Debug.LogError("Win screen not set on the board");
            return;
        }
        winScreen.SetActive(win);
    }

    public void Reset() {
        List<Entity> objects = getEntities<Entity>();
        for (int i = 0; i < objects.Count; i++) {
            objects[i].Reset();
        }
    }

    public virtual int getStars()
    {
        return 3;
    }


    protected int numberOfLinesUsed()
    {
        return inputProcessor.GetCode().Split("\n").Length;
        
    }

    protected bool usedLoop()
    {
        return inputProcessor.GetCode().Contains("while(") || inputProcessor.GetCode().Contains("for(");
    }

    protected bool usedIfStatement()
    {
        return inputProcessor.GetCode().Contains("if(");
    }

    protected bool usedStatement()
    {
        return inputProcessor.GetCode().Contains("if(");
    }

    protected int getNumberCoins()
    {
        List<Hero> heros = getEntities<Hero>();
        int numCoins = 0;
        foreach (Hero hero in heros)
        {
            while (hero.TakeFromInventory("coin") != null)
            {
                numCoins++;
            }
        }
        return numCoins;
    }

    public virtual string GetSign()
    {
        List<Hero> heros = getEntities<Hero>();
        List<Sign> signs = getEntities<Sign>();
        foreach (Hero hero in heros)
        {
            foreach (Sign sign in signs)
            {
                if (hero.getPosition() == sign.getPosition())
                {
                    return sign.currentDirection;
                }
            }
        }
        return null;
    }

    public void PrintEntities()
    {
        List<Entity> entities = getEntities<Entity>();
        foreach (Entity entity in entities)
        {
            Debug.Log($"\t{entity.getName()}");
        }
    }

}
