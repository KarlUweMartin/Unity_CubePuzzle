using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 20f;
    private Vector2 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        if (!isDragging && IsPointerOverCollider()) return; 

        HandleTouchInput();
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverCollider()) return; 

            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) 
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            RotateObject(delta);
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverCollider()) return; 

                lastMousePosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastMousePosition;
                RotateObject(delta);
                lastMousePosition = touch.position;
            }
        }
    }

    void RotateObject(Vector2 delta)
    {
        Vector3 worldRight = Camera.main.transform.right;
        Vector3 worldUp = Vector3.up;

        transform.Rotate(worldUp, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(worldRight, delta.y * rotationSpeed * Time.deltaTime, Space.World);
    }

    bool IsPointerOverCollider()
    {
        Ray ray;
        if (Input.touchCount > 0)
        {
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        return Physics.Raycast(ray, out RaycastHit hit);
    }
}
