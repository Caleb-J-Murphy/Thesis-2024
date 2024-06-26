using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
<<<<<<< Updated upstream
    public static LevelController Instance { get; private set; }

    private Dictionary<string, float> levelTimes = new Dictionary<string, float>();
    private float startTime;
    private string currentLevel;

    private void Awake()
    {
=======
<<<<<<< Updated upstream
    public string sceneName;
    public void NextLevel()
    {
        SceneManager.LoadScene(sceneName);
=======
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
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        if (!string.IsNullOrEmpty(currentLevel))
        {
            float timeSpent = Time.time - startTime;
            if (levelTimes.ContainsKey(currentLevel))
            {
                levelTimes[currentLevel] += timeSpent;
            }
            else
            {
                levelTimes[currentLevel] = timeSpent;
            }
        }

        currentLevel = scene.name;
        startTime = Time.time;
        Debug.Log(levelTimes);
=======
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
>>>>>>> Stashed changes
    }

    public void MoveToNextLevel(string nextLevelName)
    {
<<<<<<< Updated upstream
        Debug.Log("Changing Level");
        SceneManager.LoadScene(nextLevelName);
        Debug.Log("Move to next level");
=======
        
        SceneManager.LoadScene(nextLevelName);
        
        
>>>>>>> Stashed changes
    }

    public float GetTimeSpentInLevel(string levelName)
    {
<<<<<<< Updated upstream
        return levelTimes.ContainsKey(levelName) ? levelTimes[levelName] : 0f;
    }

    public void ResetLevelTimes()
    {
        levelTimes.Clear();
=======
        return levelStatistics.ContainsKey(levelName) ? levelStatistics[levelName].TimeEnd - levelStatistics[levelName].TimeStart : 0f;
    }

    public void ResetLevelStatistics()
    {
        levelStatistics.Clear();
>>>>>>> Stashed changes
>>>>>>> Stashed changes
    }
}
