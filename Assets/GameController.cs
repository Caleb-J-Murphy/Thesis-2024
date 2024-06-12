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

    public GameObject winScreen;

    public String boardLevel;

    public Type boardLevelType;

    public TextAsset mapFile;


    public GameObject gameHolder;


    private InputProcessor inputProcessor;

    void Awake() {
        boardLevelType = Type.GetType(boardLevel);
        Debug.Log(boardLevelType);
        if (boardLevelType == null || !typeof(Board).IsAssignableFrom(boardLevelType))
        {
            Debug.LogError($"Invalid board type: {boardLevel}");
        }
    }

    void Start() {
        inputProcessor = GetComponent<InputProcessor>();

        if (inputProcessor == null)
        {
            Debug.LogError("InputProcessor component not found!");
        }

        if (!gameHolder) {
            Debug.LogError("Game holder not set");
        }
    }

    public string GetMap() {
        if (!mapFile) {
            Debug.LogError("No map file given");
            return string.Empty;
        }
        return mapFile.text;
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

    public Board CreateBoardWithComponent(Type boardType)
    {
        GameObject boardGM = new GameObject("Board");
        boardGM.transform.SetParent(gameHolder.transform);
        boardGM.transform.localPosition = Vector3.zero;

        Component component = boardGM.AddComponent(boardType);

        if (component == null)
        {
            Debug.LogError("Failed to add component");
            return null;
        }

        // Return the added component, casted to Board
        return component as Board;
    }

    public Board CreateBoardFromMap(string map)
    {
        Board board = CreateBoardWithComponent(boardLevelType);

        board.winScreen = winScreen;

        string[] lines = map.Split('\n');
        int height = lines.Length;

        for (int y = 0; y < height; y++)
        {
            string line = lines[y].Trim();
            for (int x = 0; x < line.Length; x++)
            {
                char cell = line[x];
                Vector3 position = new Vector3(x, -y, 0);
                if (cell == 'W')
                {
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, gameHolder.transform);
                    board.AddEntity(wall.GetComponent<Wall>(), new Vector2(x, -y));

                }
                else if (cell == 'M')
                {
                    CreateMine(board, position, x, -y);
                }
                else if (cell == 'H')
                {
                    GameObject hero = Instantiate(heroPrefab, position, Quaternion.identity, gameHolder.transform);
                    hero.GetComponent<Hero>().Initialise(100, 50);
                    board.AddEntity(hero.GetComponent<Hero>(), new Vector2(x, -y));
                } else if (cell == 'G')
                {
                    GameObject gem = Instantiate(gemPrefab, position, Quaternion.identity, gameHolder.transform);
                    board.AddEntity(gem.GetComponent<Gem>(), new Vector2(x, -y));
                } else if (cell == 'D')
                {
                    GameObject door = Instantiate(doorPrefab, position, Quaternion.identity, gameHolder.transform);
                    board.AddEntity(door.GetComponent<Door>(), new Vector2(x, -y));
                }
            }
        }

        return board;
    }

    private void CreateMine(Board board, Vector2 position, int x, int y) {
        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity, gameHolder.transform);
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
            entity.setOrigin(entity.getPosition());
            if (entity.getName() == "hero" && entity is Hero heroEntity) {
                heroEntity.board = board;
            }
            if (entity.getName() == "door" && entity is Door doorEntity) {
                doorEntity.setBoard(board);
            }
            entities[entity.getName()] = entity;
        }
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
