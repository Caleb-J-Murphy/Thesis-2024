using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public List<UIDragHandler> draggableUI;

    public bool isUIBeingDragged() {
        foreach (UIDragHandler handler in draggableUI) {
            if (handler.getDragging()) {
                return true;
            }
        }
        return false;
    }
}
