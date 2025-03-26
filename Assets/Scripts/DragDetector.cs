using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static LogicModel;

public class DragDetector : MonoBehaviour
{
    public UnityEvent<Vector3> OnDragStart = new();
    public UnityEvent<float> OnDragUpdate = new();
    public UnityEvent OnDragEnd = new();

    void Update()
    {
        if (IsShuffeling) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartDrag(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            EndDrag();
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            EndDrag();
        }

        if (Input.GetMouseButton(0))
        {
            _liveInputPosition = Input.mousePosition;
        }
        else if (Input.touchCount > 1)
        {
            _liveInputPosition = Input.GetTouch(0).position;
        }

        if (_isDragging)
        {
            DetectDrag();
        }

        if (_dragStarted) 
        {
            var dragAmount = (_initialDistanceToScreenEdge - DistanceToScreenEdge(_liveInputPosition, _dragVector2D)) * .25f;

            if (Mathf.Abs(dragAmount) > 47)
            {
                EndDrag();
            }
            else 
            {
                OnDragUpdate.Invoke(dragAmount);
            }
        }
    }

    private void DetectDrag()
    {
        _dragDistance = Vector2.Distance(_startPosition, _liveInputPosition);

        if (!_dragStarted && _dragDistance > _dragThreshhold)
        {
            _dragVector2D = _liveInputPosition - _startPosition;
            _initialDistanceToScreenEdge = DistanceToScreenEdge(_liveInputPosition, _dragVector2D);

            Ray ray = Camera.main.ScreenPointToRay(_liveInputPosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _dragStarted = true;
                _secondHit = hit.point;
                var rawDragVector = (_secondHit - _firstHit).normalized;

                if (hit.transform.parent != null)
                {
                    var localDragVector = hit.transform.parent.InverseTransformDirection(rawDragVector);
                    var localDragVectorNormalized = GetClosestLocalDirection(localDragVector);
                    var localAxisAlignedDragVector = hit.transform.parent.TransformDirection(localDragVectorNormalized);

                    OnDragStart.Invoke(localAxisAlignedDragVector);
                }
            }
        }
    }

    private void StartDrag(Vector2 position)
    {
        var ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out var hit))
        {
            _isDragging = true;
            _startPosition = position;
            _firstHit = hit.point;
        }
    }

    private void EndDrag()
    {
        _isDragging = false;
        _dragStarted = false;
        _dragDistance = 0;

        OnDragEnd.Invoke();
    }

    private Vector3 GetClosestLocalDirection(Vector3 localDirection)
    {
        Vector3[] localDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        return localDirections.OrderBy(dir => Vector3.Angle(localDirection, dir)).First();
    }

    public float DistanceToScreenEdge(Vector2 screenPoint, Vector2 direction)
    {
        direction.Normalize();

        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var tMax = float.MaxValue;

        if (direction.x != 0)
        {
            var t1 = (0 - screenPoint.x) / direction.x;
            var t2 = (screenWidth - screenPoint.x) / direction.x;
            tMax = Mathf.Min(tMax, Mathf.Max(t1, t2));
        }

        if (direction.y != 0)
        {
            var t3 = (0 - screenPoint.y) / direction.y;
            var t4 = (screenHeight - screenPoint.y) / direction.y;
            tMax = Mathf.Min(tMax, Mathf.Max(t3, t4));
        }

        return tMax;
    }

    private Vector2 _startPosition, _liveInputPosition, _dragVector2D;
    private Vector3 _firstHit, _secondHit;
    private bool _isDragging = false;
    private bool _dragStarted = false;

    private int _dragThreshhold = 12;
    private float _dragDistance, _initialDistanceToScreenEdge;
}
