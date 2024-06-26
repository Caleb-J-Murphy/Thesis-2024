using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    public struct LevelStatistics
    {
        public float TimeStart;
        public float TimeEnd;
        public int RunAttempts;
    }

    [SerializeField] private Dictionary<string, LevelStatistics> levelStatistics = new Dictionary<string, LevelStatistics>();
    private string currentLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelStatistics levelStat;
        //Update the time for the loaded amount
        if (levelStatistics.TryGetValue(currentLevel, out levelStat))
        {
            //Anything that needs to be set at the end
            levelStat.TimeEnd = Time.time;
        }


        currentLevel = scene.name;
        if (levelStatistics.TryGetValue(currentLevel, out levelStat))
        {
            //This means we are going to the same level twice hmmmm
        }
        else
        {
            //Any intial things that need to be set.
            LevelStatistics newLevelStat = new LevelStatistics();
            newLevelStat.TimeStart = Time.time;
            newLevelStat.RunAttempts = 0;
            levelStatistics[currentLevel] = newLevelStat;
        }
        Debug.Log(levelStatistics);
    }

    public void addRunAttempt()
    {
        LevelStatistics levelStat;
        if (levelStatistics.TryGetValue(currentLevel, out levelStat))
        {
            Debug.Log("Just added an run attempt");
            levelStat.RunAttempts++;
        }
        else
        {
            Debug.LogError("This scene does not exist");
        }
    }

    public void MoveToNextLevel(string nextLevelName)
    {        
        SceneManager.LoadScene(nextLevelName);
    }
}
