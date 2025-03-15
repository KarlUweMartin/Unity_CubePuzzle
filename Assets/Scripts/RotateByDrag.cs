using UnityEngine;

public class RotateByDrag : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 20f;
    [SerializeField] private bool _localX, _localY, _invertX, _invertY = false;

    private Vector2 _lastMousePosition;
    private bool _isDragging = false;

    void Update()
    {
        if (!_isDragging && IsPointerOverCollider()) return; 

        HandleTouchInput();
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverCollider()) return; 

            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) 
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePosition;
            RotateObject(delta);
            _lastMousePosition = Input.mousePosition;
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

                _lastMousePosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - _lastMousePosition;
                RotateObject(delta);
                _lastMousePosition = touch.position;
            }
        }
    }

    void RotateObject(Vector2 delta)
    {
        Vector3 worldUp = _invertX ? Vector3.down : Vector3.up;
        Vector3 worldRight = _invertY ? -Camera.main.transform.right : Camera.main.transform.right;

        transform.Rotate(worldUp, delta.x * _rotationSpeed * Time.deltaTime, _localX ? Space.Self : Space.World);
        transform.Rotate(worldRight, delta.y * _rotationSpeed * Time.deltaTime, _localY ? Space.Self : Space.World);
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
