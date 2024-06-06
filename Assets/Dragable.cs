using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragable : MonoBehaviour
{
    public delegate void DragEndedDelegate(Dragable dragableObject);

    public DragEndedDelegate dragEndedCallback;

    private bool isDragged = false;
    private Vector3 mouseDragStartPosition;
    private Vector3 spriteDragStartPosition;
    private Vector3 objectDragStartPosition;


    private void OnMouseDown()
    {
        isDragged = true;
        mouseDragStartPosition = Input.mousePosition;
        objectDragStartPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        if (isDragged)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = objectDragStartPosition.z; // Maintain the same z position
            transform.position = mouseWorldPosition;
        }
    }

    private void OnMouseUp()
    {
        isDragged = false;
        dragEndedCallback(this);
    }
}