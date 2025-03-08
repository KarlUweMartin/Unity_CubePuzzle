using UnityEngine;
using UnityEngine.Events;

public class DragDetector : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;

    public enum DragDirections
    {
        Left, Right, Up, Down
    }

    public UnityEvent<DragDirections> OnDrag = new();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Mouse Start Drag
        {
            StartDrag(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) // Touch Start Drag
        {
            StartDrag(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButtonUp(0) && isDragging) // Mouse End Drag
        {
            EndDrag(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) // Touch End Drag
        {
            EndDrag(Input.GetTouch(0).position);
        }
    }

    private void StartDrag(Vector2 position)
    {
        startPos = position;
        isDragging = true;
    }

    private void EndDrag(Vector2 position)
    {
        if (!isDragging) return;

        endPos = position;
        DetectDragDirection();
        isDragging = false;
    }

    private void DetectDragDirection()
    {
        Vector2 dragVector = endPos - startPos;

        if (dragVector.magnitude < 50f) // Ignore very small drags
            return;

        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
        {
            if (dragVector.x > 0)
                OnDrag.Invoke(DragDirections.Right);
            else
                OnDrag.Invoke(DragDirections.Left);
        }
        else
        {
            if (dragVector.y > 0)
                OnDrag.Invoke(DragDirections.Up);
            else
                OnDrag.Invoke(DragDirections.Down);
        }
    }
}
