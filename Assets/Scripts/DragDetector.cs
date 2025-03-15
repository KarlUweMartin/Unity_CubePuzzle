using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DragDetector : MonoBehaviour
{
    public UnityEvent<Vector3> OnDrag = new();

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

        if (Input.GetMouseButton(0))
        {
            _liveInputPosition = Input.mousePosition;
        }
        else if (Input.touchCount > 1)
        {
            _liveInputPosition = Input.GetTouch(0).position;
        }

        if (isDragging)
        {
            DetectDirection();
        }
    }

    private void DetectDirection()
    {
        var dragDistance = Vector2.Distance(_startPosition, _liveInputPosition);
        if (dragDistance > _dragThreshhold)
        {
            Ray ray = Camera.main.ScreenPointToRay(_liveInputPosition);
            if (Physics.Raycast(ray, out var hit))
            {
                isDragging = false;
                _secondHit = hit.point;
                var rawDragVector = (_secondHit - _firstHit).normalized;

                Debug.DrawRay(_firstHit, rawDragVector * 2, Color.green, 5);

                // Find the closest global direction
                Vector3 globalDragVector = GetClosestGlobalDirection(rawDragVector);
                Debug.DrawRay(_firstHit, globalDragVector * 5, Color.blue, 5);

                OnDrag.Invoke(globalDragVector);
            }
        }
    }

    private Vector3 GetClosestGlobalDirection(Vector3 dragVector)
    {
        Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

        return directions
            .OrderByDescending(dir => Vector3.Dot(dragVector, dir))
            .First();
    }

    private void StartDrag(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out var hit))
        {
            isDragging = true;
            _startPosition = position;
            _firstHit = hit.point;
        }
    }

    private void EndDrag(Vector2 position)
    {
        isDragging = false;
    }
  
    private Vector2 _startPosition, _liveInputPosition;

    private Vector3 _firstHit, _secondHit;
    private bool isDragging = false;

    private int _dragThreshhold = 25; 

}
