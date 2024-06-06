using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{ 

    private const string ROTATE_CLOCKWISE = "Rotate_CW";
    private const string ROTATE_ANTICLOCKWISE = "Rotate_ACW";
    private const string MOVE_FORWARD = "Move_Forward";

    public float stepTime = 1f;
    //Order matters here
    public List<GameObject> inputs;
    public GameObject player;

    private Vector3 playStartPos;
    private Quaternion playStartRot;
    // Start is called before the first frame update
    void Start()
    {
        playStartPos = player.transform.position;
        playStartRot = player.transform.rotation;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Enter key is pressed
            Debug.Log("Enter key is pressed");
            Play();
        }
    }

    // Update is called once per frame
    public void Play()
    {
        StartCoroutine(PlayInputs());
    }

    private IEnumerator PlayInputs()
    {
        player.transform.position = playStartPos;
        player.transform.rotation = playStartRot;
        foreach (var input in inputs)
        {
            //Wait for next step
            yield return new WaitForSeconds(stepTime);
            string? move = getMove(input);
            if (move == null) {
                continue;
            }
            if (move == ROTATE_CLOCKWISE)
            {
                player.transform.Rotate(0, 0, -90);
            }
            else if (move == ROTATE_ANTICLOCKWISE)
            {
                player.transform.Rotate(0, 0, 90);
            }
            else if (move == MOVE_FORWARD)
            {
                player.transform.Translate(Vector3.up);
            }
            
        }
    }

    private string getMove(GameObject move) {
        ItemHolder itemHolder = move.GetComponent<ItemHolder>();
        if (itemHolder == null || itemHolder.item == null)
        {
            return null;
        }
        return itemHolder.item.tag;
    }
}
