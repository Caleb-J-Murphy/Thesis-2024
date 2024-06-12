using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    private Vector2 position;

    private Vector2 originalPosition;

    public abstract string getName();

    public bool Equals(Entity obj) {
        if (obj == null) {
            return false;
        }
        if (obj.getName() == this.getName()) {
            return true;
        }
        return false;
    }

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

    public float getDistance(Vector2 position) {
        float deltaX = getPosition().x - position.x;
        float deltaY = getPosition().y - position.y;
        return Mathf.Sqrt(Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaY, 2));
    }

    public void setOrigin(Vector2 position) {
        originalPosition = position;
    }

    public Vector2 getOrigin() {
        return originalPosition;
    }
}
