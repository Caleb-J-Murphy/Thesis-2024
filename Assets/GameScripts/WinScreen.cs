using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    public LevelController levelController;

    void Start() {
        if (!levelController) {
            Debug.LogError("Level controller not set on win screen");
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            levelController.NextLevel();
        }
    }
}
