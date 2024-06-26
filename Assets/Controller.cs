using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private List<(string, string)> controlQueue = new List<(string, string)>();

    private bool isProcessing = false;

    public void Start() {
        control();
    }

    public void addControl(string functionName, string functionVariable) {
        controlQueue.Add((functionName, functionVariable));
    }

    public void control() {
        StartCoroutine(ProcessControlQueue());
    }

    private IEnumerator ProcessControlQueue() {
        while (true) {
            //Wait until there is something to process
            yield return new WaitUntil(() => controlQueue.Count > 0);
            //Wait for the previous process to be completed
            yield return new WaitUntil(() => !isProcessing);
            isProcessing = true;

            var tuple = controlQueue[0];
            string functionName = tuple.Item1;
            string functionVariable = tuple.Item2;
            controlQueue.RemoveAt(0);

            if (functionName == "moveUp") {
                int number;
                if (int.TryParse(functionVariable, out number)) {
                    yield return StartCoroutine(MoveUpCoroutine(number));
                } else {
                    Debug.LogError("Invalid number format for moveUp command: " + functionVariable);
                }
            } else if (functionName == "moveDown") {
                int number;
                if (int.TryParse(functionVariable, out number)) {
                    yield return StartCoroutine(MoveDownCoroutine(number));
                } else {
                    Debug.LogError("Invalid number format for moveDown command: " + functionVariable);
                }
            } else if (functionName == "moveRight") {
                int number;
                if (int.TryParse(functionVariable, out number)) {
                    yield return StartCoroutine(MoveRightCoroutine(number));
                } else {
                    Debug.LogError("Invalid number format for moveRight command: " + functionVariable);
                }
            } else if (functionName == "moveLeft") {
                int number;
                if (int.TryParse(functionVariable, out number)) {
                    yield return StartCoroutine(MoveLeftCoroutine(number));
                } else {
                    Debug.LogError("Invalid number format for moveLeft command: " + functionVariable);
                }
            } else if (functionName == "lookUp") {
                yield return StartCoroutine(LookUpCoroutine());
            } else if (functionName == "lookDown") {
                yield return StartCoroutine(LookDownCoroutine());
            } else if (functionName == "lookRight") {
                yield return StartCoroutine(LookRightCoroutine());
            } else if (functionName == "lookLeft") {
                yield return StartCoroutine(LookLeftCoroutine());
            }

            isProcessing = false;
        }
    }

    private IEnumerator MoveUpCoroutine(int number) {
        for (int i = 0; i < number; i++) {
            transform.Translate(Vector3.up);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator MoveRightCoroutine(int number) {
        for (int i = 0; i < number; i++) {
            transform.Translate(Vector3.right);
            yield return new WaitForSeconds(0.5f);
        }
    }
    private IEnumerator MoveLeftCoroutine(int number) {
        for (int i = 0; i < number; i++) {
            transform.Translate(Vector3.left);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator MoveDownCoroutine(int number) {
        for (int i = 0; i < number; i++) {
            transform.Translate(Vector3.down);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator LookUpCoroutine() {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LookRightCoroutine() {
        transform.rotation = Quaternion.Euler(0, 0, -90);
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator LookLeftCoroutine() {
        transform.rotation = Quaternion.Euler(0, 0, 90);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LookDownCoroutine() {
        transform.rotation = Quaternion.Euler(0, 0, 180);
        yield return new WaitForSeconds(0.5f);
    }

}
