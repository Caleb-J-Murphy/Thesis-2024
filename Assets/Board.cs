using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Entity> objects = new List<Entity>();

    public void step() {
        //Check that the there are heros on a mine that is a activated
        List<Mine> mines = getEntities<Mine>();
        List<Hero> heroes = getEntities<Hero>();

        checkPlayerHitMines(mines, heroes);
    }

    public void AddEntity(Entity entity, Vector2 position) {
        Hero specificEntity = entity as Hero;
        if (specificEntity != null) {
            specificEntity.board = this;
        }
        entity.setPosition(position);
        objects.Add(entity);
    }

    private void checkPlayerHitMines(List<Mine> mines, List<Hero> heroes)
    {
        foreach (Mine mine in mines)
        {
            if (mine.isActivated)
            {
                foreach (Hero hero in heroes)
                {
                    if (hero.getPosition() == mine.getPosition())
                    {
                        mine.Trigger(hero); // Assuming Trigger method takes a Hero parameter
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


}
