using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapController : MonoBehaviour
{
    public List<Transform> snapPoints;
    public List<Dragable> dragableObjects;
    public float snapRange = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Dragable dragable in dragableObjects) {
            dragable.dragEndedCallback = OnDragEnded;
        }
    }

    private void OnDragEnded(Dragable dragable)
    {
        float closestDistance = -1;
        Transform closestSnapPoint = null;

        foreach (Transform snapPoint in snapPoints)
        {
            float currentDistance = Vector2.Distance(dragable.transform.position, snapPoint.position);
            if (closestSnapPoint == null || currentDistance < closestDistance)
            {
                closestSnapPoint = snapPoint;
                closestDistance = currentDistance;
            }
        }

        if (closestSnapPoint != null && closestDistance <= snapRange)
        {
            //CHeck that the snappoint can hold an item
            ItemHolder itemHolder = closestSnapPoint.GetComponent<ItemHolder>();
            if (itemHolder == null){
                Debug.Log("Does not have an input controller");
                return;
            } 
            // Check whether the snappoint already as an item
            if (itemHolder.item != null) {
                Debug.Log("There is already an item here");
                return;
            }

            //Update item position and add item to the item holder
            dragable.transform.position = closestSnapPoint.position;
            itemHolder.item = dragable.gameObject;
        }
    }

}
