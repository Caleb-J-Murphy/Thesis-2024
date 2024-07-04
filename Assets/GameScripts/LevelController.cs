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
        //Update the time for the loaded amount
        if (currentLevel != null)
        {
            endLevel(currentLevel);
        }

        LevelStatistics levelStat;
        currentLevel = scene.name;
        if (levelStatistics.TryGetValue(currentLevel, out levelStat))
        {
            //This means we are going to the same level twice hmmmm
            Debug.LogError("We are back to the same level we have already been to????");
        }
        else
        {
            startLevel(currentLevel);
        }
    }

    private void printLevelStats()
    {
        foreach (var entry in levelStatistics)
        {
            string level = entry.Key;
            LevelStatistics stats = entry.Value;

            Debug.Log($"Level: {level}");
            Debug.Log($"\tStart Time: {stats.TimeStart}");
            Debug.Log($"\tEnd Time: {stats.TimeEnd}");
            Debug.Log($"\tRun Attempts: {stats.RunAttempts}");
        }
    }

    public void addRunAttempt()
    {
        LevelStatistics levelStat;
        if (levelStatistics.TryGetValue(currentLevel, out levelStat))
        {
            levelStat.RunAttempts++;
            levelStatistics[currentLevel] = levelStat;
        }
        else
        {
            Debug.LogError("This scene does not exist");
        }
    }

    private void startLevel(string levelName)
    {
        LevelStatistics newLevelStat = new LevelStatistics();
        newLevelStat.RunAttempts = 0;
        newLevelStat.TimeStart = Time.time;
        levelStatistics[levelName] = newLevelStat;
    }

    private void endLevel(string levelName)
    {
        LevelStatistics levelStat;
        if (levelStatistics.TryGetValue(levelName, out levelStat))
        {
            levelStat.TimeEnd = Time.time;
            levelStatistics[levelName] = levelStat;
        } else
        {
            Debug.LogError($"Cannot find the current level: {levelName}");
        }
    }

    public void MoveToNextLevel(string nextLevelName)
    {        
        SceneManager.LoadScene(nextLevelName);
    }
}
