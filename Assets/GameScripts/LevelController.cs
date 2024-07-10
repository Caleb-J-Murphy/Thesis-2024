using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using System.Linq;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine.Windows;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    public struct Attempt
    {
        public float timeStart;
        public float timeEnd;
        public int stars;
        public bool completed;
        public string errors;
        public string code;
    }

    [SerializeField] private Dictionary<int, Dictionary<string, List<Attempt>>> statistics = new Dictionary<int, Dictionary<string, List<Attempt>>>();
    [SerializeField] private int currentUserID;
    [SerializeField] private TextAsset textAsset;

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
    /*
     * Called when creating a new user at the beginning of the game
     */
    public void AddUser(int userID)
    {
        //Check that the user does not exist
        if(statistics.ContainsKey(userID))
        {
            Debug.Log("User already exists");
        } else
        {
            statistics[userID] = new Dictionary<string, List<Attempt>>();
        }

        currentUserID = userID;
    }

    /*
     * Called when starting a new level and a new attempt
     */
    public void StartNewAttempt()
    {
        //Check the user exists
        if(!statistics.ContainsKey(currentUserID))
        {
            Debug.LogError($"User {currentUserID} does not exist or level {currentLevel} exists");
            return;
        }
        if(statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"Level {currentLevel} exists, wrong function called");
            return;
        }
        //Add a new attempt
        Attempt newAttempt = new Attempt();
        newAttempt.timeStart = Time.time;

        statistics[currentUserID][currentLevel] = new List<Attempt>
        {
            newAttempt
        };
    }
    
    /*
     * Called when starting a new attempt on the same level
     */
    public void StartAttempt()
    {
        //Check the user exists
        if (!statistics.ContainsKey(currentUserID) || !statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"User {currentUserID} does not exist or level {currentLevel} does not exist");
            return;
        }
        //Finish old attempt
        Attempt oldAttempt = statistics[currentUserID][currentLevel].Last();
        oldAttempt.timeEnd = Time.time;
        //Add a new attempt
        Attempt newAttempt = new Attempt();
        newAttempt.timeStart = Time.time;
        statistics[currentUserID][currentLevel].Add(newAttempt);
    }

    /*
     * Called to end the current attempt time
     */
    public void EndAttempt()
    {
        //Finish old attempt
        List<Attempt> attemptsList = statistics[currentUserID][currentLevel];
        if (attemptsList.Count == 0)
        {
            Debug.LogError($"No attempts found for user {currentUserID} on level {currentLevel}");
            return;
        }

        // Retrieve the index of the last attempt
        int lastIndex = attemptsList.Count - 1;
        Attempt attempt = attemptsList[lastIndex];

        // Update the code
        attempt.timeEnd = Time.time;

        // Update the attempt in the list
        attemptsList[lastIndex] = attempt;

        //Now we need to write this to a file
        WriteStatisticsToCsv("Assets/RecordedResults.txt");
    }

    private void WriteStatisticsToCsv(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write the header
            writer.WriteLine("UserID,Level,TimeStart,TimeEnd,Stars,Completed,Errors,Code");

            // Iterate through the statistics dictionary and write the data
            foreach (var userEntry in statistics)
            {
                int userID = userEntry.Key;
                foreach (var levelEntry in userEntry.Value)
                {
                    string level = levelEntry.Key;
                    foreach (var attempt in levelEntry.Value)
                    {
                        writer.WriteLine($"{userID},{level},{attempt.timeStart},{attempt.timeEnd},{attempt.stars},{attempt.completed},{attempt.errors},{attempt.code}");
                        Debug.Log($"{userID},{level},{attempt.timeStart},{attempt.timeEnd},{attempt.stars},{attempt.completed},{attempt.errors},{attempt.code}");
                    }
                }
            }
        }

        Debug.Log($"Statistics written to {filePath}");
    }  

    /*
     * Called if the level is completed successfully by the player
     */
    public void Completed()
    {
        if (!statistics.ContainsKey(currentUserID) || !statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"User or level does not exist: {currentUserID} -- {currentLevel}");
            return;
        }
        List<Attempt> attemptsList = statistics[currentUserID][currentLevel];
        if (attemptsList.Count == 0)
        {
            Debug.LogError($"No attempts found for user {currentUserID} on level {currentLevel}");
            return;
        }

        // Retrieve the index of the last attempt
        int lastIndex = attemptsList.Count - 1;
        Attempt attempt = attemptsList[lastIndex];

        // Update the code
        attempt.completed = true;

        // Update the attempt in the list
        attemptsList[lastIndex] = attempt;
    }

    /*
     * Called if the player successfully finishes the level and it sets the number of stars achieved
     */
    public void SetStars(int starCount)
    {
        if (!statistics.ContainsKey(currentUserID) || !statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"User or level does not exist: {currentUserID} -- {currentLevel}");
            return;
        }
        if (starCount > 3 || starCount < 0) {
            Debug.LogError($"Cant have a star count above 3 or below 0: {starCount}");
            return;
        }
        List<Attempt> attemptsList = statistics[currentUserID][currentLevel];
        if (attemptsList.Count == 0)
        {
            Debug.LogError($"No attempts found for user {currentUserID} on level {currentLevel}");
            return;
        }

        // Retrieve the index of the last attempt
        int lastIndex = attemptsList.Count - 1;
        Attempt attempt = attemptsList[lastIndex];

        // Update the code
        attempt.stars = starCount;

        // Update the attempt in the list
        attemptsList[lastIndex] = attempt;

        Debug.Log($"Added the star count: {starCount}");
    }

    public void SetError(string error)
    {
        if (!statistics.ContainsKey(currentUserID) || !statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"User or level does not exist: {currentUserID} -- {currentLevel}");
            return;
        }
        List<Attempt> attemptsList = statistics[currentUserID][currentLevel];
        if (attemptsList.Count == 0)
        {
            Debug.LogError($"No attempts found for user {currentUserID} on level {currentLevel}");
            return;
        }

        // Retrieve the index of the last attempt
        int lastIndex = attemptsList.Count - 1;
        Attempt attempt = attemptsList[lastIndex];

        // Update the code
        attempt.errors = error;

        // Update the attempt in the list
        attemptsList[lastIndex] = attempt;
    }

    public void SetCode(string code)
    {
        if (!statistics.ContainsKey(currentUserID) || !statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"User or level does not exist: {currentUserID} -- {currentLevel}");
            return;
        }
        List<Attempt> attemptsList = statistics[currentUserID][currentLevel];
        if (attemptsList.Count == 0)
        {
            Debug.LogError($"No attempts found for user {currentUserID} on level {currentLevel}");
            return;
        }

        // Retrieve the index of the last attempt
        int lastIndex = attemptsList.Count - 1;
        Attempt attempt = attemptsList[lastIndex];

        // Update the code
        //Remove the new lines, from the code and instead just replace it with a comma
        code = code.Replace("\r\n", "-").Replace("\n", "-").Replace(",","comma");
        attempt.code = code;

        // Update the attempt in the list
        attemptsList[lastIndex] = attempt;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /*
     * Called when a new level is loaded by the player/
     */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentLevel != null && currentLevel != "" && !statistics.ContainsKey(currentUserID))
        {
            Debug.LogError("User does not exist");
            return;
        }
        currentLevel = scene.name;

        if (currentLevel == "HomeScreen")
        {
            return;
        }
        
        if (statistics[currentUserID].ContainsKey(currentLevel))
        {
            //We have been to this level before...
            StartAttempt();
        } else
        {
            StartNewAttempt();
        }
    }

    public void restartLevel()
    {
        if (!statistics.ContainsKey(currentUserID))
        {
            Debug.LogError($"User {currentUserID} does not exist");
            return;
        }
        if (!statistics[currentUserID].ContainsKey(currentLevel))
        {
            Debug.LogError($"Level {currentLevel} does not exist");
            return;
        }
        StartAttempt();

    }

    public void MoveToNextLevel(string nextLevelName)
    {
        if (currentLevel != "HomeScreen")
        {
            EndAttempt();
        }
        SceneManager.LoadScene(nextLevelName);
    }
}
