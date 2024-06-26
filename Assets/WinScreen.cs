using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private string nextLevelName;

    public void NextLevel()
    {
        LevelController levelController = LevelController.Instance;

        if (levelController != null) {
            levelController.MoveToNextLevel(nextLevelName);
        } else {
            Debug.LogError("LevelController instance not found!");
        }
    }


}
