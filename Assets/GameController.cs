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

    LevelController levelController = LevelController.Instance;


    public GameObject wallPrefab;
    public GameObject heroPrefab;
    public GameObject minePrefab;
    public GameObject coinPrefab;
    public GameObject doorPrefab;
    public GameObject floorPrefab;
    public GameObject keyPrefab;

    public GameObject winScreen;
    public ProgressBar healthBar;
    public ProgressBar staminaBar;

    public String boardLevel;

    public Type boardLevelType;

    public TextAsset mapFile;


    public GameObject gameHolder;

    public InventoryVisualiser inventoryVisualiser;

    private InputProcessor inputProcessor;

    private List<Hero> heros;

    void Awake() {
        boardLevelType = Type.GetType(boardLevel);
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

    public void RunAttempt()
    {
        levelController.addRunAttempt();
    }

    public void Initialise(string map, out Board board, out Dictionary<string, Entity> entities, out Dictionary<string, Action<string>> entityFunctions) {
        board = CreateBoardFromMap(map);
        entities = createEntities(board);
        entityFunctions = createEntityFunctions();
        setupInventoryVisualisation(board);
        if (entities == null)
        {
            Debug.LogError("Entities dictionary is null!");
        }

        if (entityFunctions == null)
        {
            Debug.LogError("EntityFunctions dictionary is null!");
        }
    }

    public void setupInventoryVisualisation(Board board) {
        if (inventoryVisualiser == null) {
            Debug.LogError("Inventory Visualisation not set in the Game Controller");
            return;
        }
        inventoryVisualiser.setUpVisualisation(board);

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
                if(cell == '.')
                {
                    //This is just air but still spacing outside the walls.
                    continue;
                }else if (cell == 'W')
                {
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, gameHolder.transform);
                    board.AddEntity(wall.GetComponent<Wall>(), new Vector2(x, -y));
                }
                else {
                    GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, gameHolder.transform);
                    floor.transform.localPosition = position;
                    if (cell == 'M')
                    {
                        CreateMine(board, position, x, -y);
                    }
                    else if (cell == 'H')
                    {
                        board = CreateHero(board, position, x, -y);
                    } else if (cell == 'C')
                    {
                        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity, gameHolder.transform);
                        board.AddEntity(coin.GetComponent<Coin>(), new Vector2(x, -y));
                    } else if (cell == 'D')
                    {
                        GameObject door = Instantiate(doorPrefab, position, Quaternion.identity, gameHolder.transform);
                        board.AddEntity(door.GetComponent<Door>(), new Vector2(x, -y));
                    } else if (cell == 'K')
                    {
                        GameObject key = Instantiate(keyPrefab, position, Quaternion.identity, gameHolder.transform);
                        board.AddEntity(key.GetComponent<Key>(), new Vector2(x, -y));
                    }
                }
            }
        }
        heros = board.getEntities<Hero>();
        if (heros.Count > 0)
        {
            gameHolder.transform.position = new Vector3(heros[0].getPosition().x, heros[0].getPosition().y * -1, 0);
        }
        return board;
    }


    private Board CreateHero(Board board, Vector2 position, int x, int y) {
        GameObject hero = Instantiate(heroPrefab, position, Quaternion.identity, gameHolder.transform);
        Hero heroComponent = hero.GetComponent<Hero>();
        heroComponent.OnHealthChanged += updateHealthBar;
        heroComponent.OnStaminaChanged += updateStaminaBar;
        heroComponent.Initialise(15, 10);
        board.AddEntity(heroComponent, new Vector2(x, y));
        return board;
    }


    private void updateHealthBar(int health) {
        if (heros == null) {
            return;
        }
        foreach (Hero hero in heros) {
            healthBar.SetProgressPercent(health / hero.getMaxStamina());
        }
    }

    private void updateStaminaBar(int stamina) {
        if (heros == null) {
            return;
        }
        foreach (Hero hero in heros) {
            staminaBar.SetProgressPercent(stamina / hero.getMaxStamina());
        }
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
            //Hero Functions
            { "moveForward",    (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.moveForward(int.Parse(param))) },
            { "moveUp",         (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.moveUp()) },
            { "moveDown",       (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.moveDown()) },
            { "moveRight",       (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.moveRight()) },
            { "moveLeft",       (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.moveLeft()) },
            { "turnRight",      (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.turnRight()) },
            { "turnLeft",       (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero => 
                hero.turnLeft()) },
            { "pickUpItem",     (param) => inputProcessor.ExecuteEntityFunctionHero("hero", hero =>
                hero.PickUpItem()) },

            //Door Functions
            { "useDoor",       (param) => inputProcessor.ExecuteEntityFunctionDoor("door", door => 
                door.useDoor()) },
        };
        return entityFunctions;
    }
}
