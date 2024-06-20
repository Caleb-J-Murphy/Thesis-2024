using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    private Dictionary<string, float> levelTimes = new Dictionary<string, float>();
    private float startTime;
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
    }

    public void MoveToNextLevel(string nextLevelName)
    {
        Debug.Log("Changing Level");
        SceneManager.LoadScene(nextLevelName);
        Debug.Log("Move to next level");
    }

    public float GetTimeSpentInLevel(string levelName)
    {
        return levelTimes.ContainsKey(levelName) ? levelTimes[levelName] : 0f;
    }

    public void ResetLevelTimes()
    {
        levelTimes.Clear();
    }
}
