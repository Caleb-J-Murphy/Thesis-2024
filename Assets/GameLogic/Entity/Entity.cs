using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    private Vector2 position;

    public abstract string getName();

    void Start () {
        position = transform.position;
    }

    public Vector2 getPosition() {
        return position;
    }

    public void setPosition(Vector2 newPosition) {
        position = newPosition;
        updateTransform();
    }

    private void updateTransform() {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
