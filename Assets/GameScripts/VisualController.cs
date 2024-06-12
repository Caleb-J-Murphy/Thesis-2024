using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VisualController : MonoBehaviour
{
    public GameObject gameHolderVisual; // Reference to the visual child object

    private Vector3 offset;
    private bool isDragging = false;
    private Camera mainCamera;
    private Collider2D gameHolderCollider;
    private float zoomSpeed = 0.5f;
    private float minScale = 0.5f;
    private float maxScale = 2.0f;

    void Start()
    {
        mainCamera = Camera.main;
        gameHolderCollider = GetComponent<Collider2D>();
        if (gameHolderVisual == null)
        {
            Debug.LogError("GameHolderVisual is not set. Please assign it in the inspector.");
        }
    }

    void Update()
    {
        HandleDragging();
        HandleZooming();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                offset = gameHolderVisual.transform.position - GetMouseWorldPosition();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + offset;
            gameHolderVisual.transform.position = Vector3.Lerp(gameHolderVisual.transform.position, targetPosition, 0.1f);
        }
    }

    private void HandleZooming()
    {
        if (IsMouseOver())
        {
            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            if (scrollData != 0.0f)
            {
                Vector3 newScale = gameHolderVisual.transform.localScale + Vector3.one * scrollData * zoomSpeed;
                newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
                newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
                gameHolderVisual.transform.localScale = newScale;
            }
        }
    }

    private bool IsMouseOver()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;
        return gameHolderCollider.OverlapPoint(mouseWorldPosition);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10.0f;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}