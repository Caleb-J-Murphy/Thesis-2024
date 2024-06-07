using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
Used to control the game
Hold all interactable objects to be then queried
*/
public class GameController : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject heroPrefab;
    public GameObject minePrefab;
    public GameObject gemPrefab;
    public GameObject doorPrefab;


    private InputProcessor inputProcessor;
    void Start() {
        inputProcessor = GetComponent<InputProcessor>();

        if (inputProcessor == null)
        {
            Debug.LogError("InputProcessor component not found!");
        }
    }

    public void Initialise(string map, out Board board, out Dictionary<string, Entity> entities, out Dictionary<string, Action<string>> entityFunctions) {
        board = CreateBoardFromMap(map);
        entities = createEntities(board);
        entityFunctions = createEntityFunctions();

        if (entities == null)
        {
            Debug.LogError("Entities dictionary is null!");
        }

        if (entityFunctions == null)
        {
            Debug.LogError("EntityFunctions dictionary is null!");
        }
    }

    public Board CreateBoardFromMap(string map)
    {
        GameObject boardGM = new GameObject("Board");
        boardGM.AddComponent<Board>();
        Board board = boardGM.GetComponent<Board>();

        string[] lines = map.Split('\n');
        int height = lines.Length;

        for (int y = 0; y < height; y++)
        {
            string line = lines[y].Trim();
            for (int x = 0; x < line.Length; x++)
            {
                char cell = line[x];
                Vector3 position = new Vector3(x, -y, 0); // Assuming 2D grid with y-axis inverted

                if (cell == 'W')
                {
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                    board.AddEntity(wall.GetComponent<Wall>(), new Vector2(x, -y));
                }
                else if (cell == 'M')
                {
                    CreateMine(board, position, x, -y);
                }
                else if (cell == 'H')
                {
                    GameObject hero = Instantiate(heroPrefab, position, Quaternion.identity);
                    hero.GetComponent<Hero>().Initialise(100, 50);
                    board.AddEntity(hero.GetComponent<Hero>(), new Vector2(x, -y));
                } else if (cell == 'G')
                {
                    GameObject gem = Instantiate(gemPrefab, position, Quaternion.identity);
                    board.AddEntity(gem.GetComponent<Gem>(), new Vector2(x, -y));
                } else if (cell == 'D')
                {
                    GameObject door = Instantiate(doorPrefab, position, Quaternion.identity);
                    board.AddEntity(door.GetComponent<Door>(), new Vector2(x, -y));
                }
            }
        }

        return board;
    }

    private void CreateMine(Board board, Vector2 position, int x, int y) {
        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity);
        int damage = 5;
        mine.GetComponent<Mine>().Activate();
        mine.GetComponent<Mine>().SetDamage(damage);
        board.AddEntity(mine.GetComponent<Mine>(), new Vector2(x, y));
    }

    public Dictionary<string, Entity> createEntities(Board board) {
        List<Entity> entityList = board.getEntities<Entity>();

        Dictionary<string, Entity> entities = new Dictionary<string, Entity>();
        foreach (var entity in entityList)
        {
            if (entity.getName() == "hero" && entity is Hero heroEntity) {
                heroEntity.board = board;
            }
            if (entity.getName() == "door" && entity is Door doorEntity) {
                doorEntity.setBoard(board);
            }
            entities[entity.getName()] = entity;
        }

        // foreach (var kvp in entities)
        // {
        //     // Debug.Log($"Entity Name: {kvp.Key}, Entity Type: {kvp.Value.GetType()}");
        // }
        return entities;
    }

    public Dictionary<string, Action<string>> createEntityFunctions() {
        Dictionary<string, Action<string>> entityFunctions = new Dictionary<string, Action<string>>()
        {
            { "moveForward",    (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.moveForward(int.Parse(param))) },
            { "moveUp",         (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.moveUp()) },
            { "moveDown",       (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.moveDown()) },
            { "moveRight",       (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.moveRight()) },
            { "moveLeft",       (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.moveLeft()) },
            { "turnRight",      (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.turnRight()) },
            { "turnLeft",       (param) => inputProcessor.ExecuteEntityFunction("hero", hero => 
                hero.turnLeft()) },
            { "useDoor",       (param) => inputProcessor.ExecuteEntityFunctionDoor("door", door => 
                door.useDoor()) },
        };
        return entityFunctions;
    }
}
