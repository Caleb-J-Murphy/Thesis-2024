using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    private GameObject gameControllerObj;
    //private GameController gameController;
    public GameObject inputField;
    private TMP_InputField textMeshProInputField; // Change the type to TMP_InputField


    void Start()
    {
        //gameControllerObj = this.gameObject;
        //gameController = gameControllerObj.GetComponent<GameController>();


        textMeshProInputField = inputField.GetComponent<TMP_InputField>(); // Get TMP_InputField component

        if (textMeshProInputField == null)
        {
            Debug.LogError("TMP_InputField component not found on this GameObject."); // Update error message
            return;
        }

        // Print the initial text to the console
        Debug.Log("Initial text: " + textMeshProInputField.text); // Update to textMeshProInputField.text



        
    }

    public void runScript() {
        //Need to reset the character in the original position.
        //gameController.ResetPos();

        Debug.Log("Checking Text Input");
        string text = RemoveSpaces(textMeshProInputField.text);
        if (text == "") {
            Debug.Log("Nothing was given");
            return;
        }
        text = text.Substring(0, text.Length - 1);
        string[] input = SplitTextByLines(text);
        //Handle the command
        foreach (string inputItem in input) {
            HandleCommand(inputItem);
        }
    }

    void HandleCommand(string command) {
        if (command.Length == 0) {
            Debug.Log("No command entered");
            return;
        }
        // if (!gameController.UseCommand(command)) {
        //     Debug.Log("Syntax error in code");
        //     textMeshProInputField.text = "Error in code";
        // }
    }

    string[] SplitTextByLines(string inputText)
    {
        // Split the input text by lines
        string[] lines = inputText.Split('\n');

        return lines;
    }

    string RemoveSpaces(string inputText)
    {
        return inputText.Replace(" ", "");
    }
}

