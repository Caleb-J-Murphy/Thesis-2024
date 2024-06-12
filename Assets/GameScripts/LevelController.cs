using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public string sceneName;
    public void NextLevel()
    {
        SceneManager.LoadScene(sceneName);
    }
}
