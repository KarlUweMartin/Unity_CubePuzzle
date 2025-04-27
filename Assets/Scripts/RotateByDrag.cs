using UnityEngine;

public class RotateByDrag : MonoBehaviour
{
    private void Start()
    {
        _cam = Camera.main;
        _worldUp = _invertX ? -_cam.transform.up : _cam.transform.up;
        _worldRight = _invertY ? -_cam.transform.right : _cam.transform.right;
    }

    private void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (!IsPointerOverCollider() && Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0)) 
        {
            _isDragging = false;
        }
        else if(_isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePosition;
            RotateObject(delta);
            _lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (!IsPointerOverCollider() && touch.phase == TouchPhase.Began)
            {
                _isDragging = true;
                _lastMousePosition = touch.position;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                _isDragging = false;
            }
            else if(_isDragging && touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - _lastMousePosition;
                RotateObject(delta);
                _lastMousePosition = touch.position;
            }
        }
    }

    private void RotateObject(Vector2 delta)
    {
        transform.Rotate(_worldUp, delta.x * _rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(_worldRight, delta.y * _rotationSpeed * Time.deltaTime, Space.World);
    }

    private bool IsPointerOverCollider()
    {
        Ray ray;
        if (Input.touchCount > 0)
        {
            ray = _cam.ScreenPointToRay(Input.GetTouch(0).position);
        }
        else
        {
            ray = _cam.ScreenPointToRay(Input.mousePosition);
        }

        return Physics.Raycast(ray, out var hit);
    }

    [SerializeField] private float _rotationSpeed = 20f;
    [SerializeField] private bool _invertX, _invertY = false;

    private Camera _cam;
    private Vector3 _worldUp, _worldRight;
    private Vector2 _lastMousePosition;
    private bool _isDragging = false;
}
