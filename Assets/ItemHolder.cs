using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Determines what the input is holding as an item
public class ItemHolder : MonoBehaviour
{
    public GameObject item;
    public GameObject input;

    void Update()
    {
        if (item != null && item.tag == "For_Loop")
        {
            for(int i = 1; i <= 5; i++) {
                Vector3 newPosition = transform.position + (Vector3.right * i); // Offset the new position by one unit to the right
                Instantiate(input, newPosition, Quaternion.identity, transform);

                // Need to add this to the PlayerController inputs list
                // Take into account the order of things as well
            }
            
        }
    }
}
