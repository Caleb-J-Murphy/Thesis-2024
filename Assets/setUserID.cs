using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class setUserID : MonoBehaviour
{
    [SerializeField] LevelController levelController;

    [SerializeField] private TextMeshProUGUI userIDText;
    [SerializeField] private int userID;

    // Start is called before the first frame update
    void Start()
    {
        if (userIDText != null)
        {
            userIDText.text = "UserID = " + userID;
        }
        else
        {
            Debug.LogError("userIDText is not assigned in the Inspector.");
        }
    }

    public void updateUserID()
    {
        if (!levelController) {
            Debug.LogError("level controller cannot be found");
            return;
        }
        levelController.AddUser(userID);
    }
}
