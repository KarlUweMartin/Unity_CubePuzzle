using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static LogicModel;

public class PuzzleController : MonoBehaviour
{
    private void Start()
    {
        DOTween.Init();
        _shuffleButton.onValueChanged.AddListener((v) => { _shuffle = v; });
        _dragDetector.OnDragStart.AddListener(StartDrag);
        _dragDetector.OnDragUpdate.AddListener(UpdateDrag);
        _dragDetector.OnDragEnd.AddListener(EndDrag);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _touchCube = hit.collider.gameObject;
                _touchPoint = hit.point;
            }
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            //
        }

        if (Input.touchCount > 0)
        {
            // TODO: Implement the same stuff for touch!
        }

        if (_shuffle)
        {
            Randomize();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Randomize();
        }
    } 

    public void Randomize()
    {
        if (IsAnimating) return;

        var rnd = new System.Random();
        var directions = new Vector3[] { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.back, Vector3.forward };

        _touchCube = _generator.RandomCube();
        _localDragDirection = directions[rnd.Next(0, directions.Length)];

        var camPos = Camera.main.transform.position;
        var ray = new Ray(camPos, (_touchCube.transform.position - camPos) * 5);
        if (Physics.Raycast(ray, out var hit)) 
        {
            _touchPoint = hit.point;
            Debug.Log(hit.transform.name);
        }


        Debug.DrawRay(camPos, (_touchCube.transform.position - camPos) * 5, Color.red, 1);

        if (_localDragDirection != Vector3.zero && _touchCube != null && _touchPoint != Vector3.zero)
        {
            if (_pivot == null)
            {
                _pivot = new GameObject("Rotation Pivot");
                _pivot.transform.SetParent(transform);
            }
            _pivot.transform.localRotation = Quaternion.identity;
            _pivot.transform.position = Vector3.zero;

            _sliceCubes = _generator.GetSliceCubes(_touchCube, _localDragDirection, _touchPoint);
            UpdateDrag(46);
            EndDrag();
        }

    }

    private void StartDrag(Vector3 dragVector)
    {
        if (_touchCube == null || dragVector == Vector3.zero) return;

        _sliceCubes = _generator.GetSliceCubes(_touchCube, dragVector, _touchPoint);
        if (_pivot == null)
        {
            _pivot = new GameObject("Rotation Pivot");
            _pivot.transform.SetParent(transform);
        }
        _pivot.transform.localRotation = Quaternion.identity;
        _pivot.transform.position = Vector3.zero;

        var sameX = true;
        var sameY = true;
        var sameZ = true;
        var referencePosition = RoundVector(_sliceCubes[0].transform.localPosition);

        foreach (var cube in _sliceCubes) 
        {
            var pos = RoundVector(cube.transform.localPosition);
            if (pos.x != referencePosition.x) sameX = false;
            if (pos.y != referencePosition.y) sameY = false;
            if (pos.z != referencePosition.z) sameZ = false;

            cube.transform.SetParent(_pivot.transform);
        }

        if(sameX) 
        {
            _localDragDirection = Vector3.up;
        }
        else if (sameY)
        {
            _localDragDirection = Vector3.right;
        }
        else if (sameZ)
        {
            _localDragDirection = Vector3.back;
        }
    }

    private void UpdateDrag(float rotationAngle)
    {
        if(_sliceCubes == null || _localDragDirection == Vector3.zero) return;

        RotateAlongDrag(_sliceCubes, _localDragDirection, rotationAngle);
    }

    private void EndDrag()
    {
        if (_sliceCubes == null) return;

        var eulerAngles = _pivot.transform.localEulerAngles;
        eulerAngles.x = (eulerAngles.x > 180) ? eulerAngles.x - 360 : eulerAngles.x;
        eulerAngles.y = (eulerAngles.y > 180) ? eulerAngles.y - 360 : eulerAngles.y;
        eulerAngles.z = (eulerAngles.z > 180) ? eulerAngles.z - 360 : eulerAngles.z;

        var absX = Mathf.Abs(eulerAngles.x);
        var absY = Mathf.Abs(eulerAngles.y);
        var absZ = Mathf.Abs(eulerAngles.z);

        var alignedEuler = Vector3.zero;

        if (absX > absY && absX > absZ)
        {
            alignedEuler = new Vector3(Mathf.Round(eulerAngles.x / 90) * 90, 0, 0);
        }
        else if (absY > absX && absY > absZ)
        {
            alignedEuler = new Vector3(0, Mathf.Round(eulerAngles.y / 90) * 90, 0);
        }
        else
        {
            alignedEuler = new Vector3(0, 0, Mathf.Round(eulerAngles.z / 90) * 90);
        }

        _pivot.transform.DOLocalRotate(alignedEuler, .15f).OnComplete(() => 
        {
            foreach (var cube in _sliceCubes)
            {
                cube.transform.SetParent(_pivot.transform.parent);
                cube.transform.localPosition = RoundVector(cube.transform.localPosition);
            }

            _localDragDirection = Vector3.zero;
            _touchCube = null;
            _sliceCubes = null;
            IsAnimating = false;
        });
    }

    private void RotateAlongDrag(GameObject[] sliceCubes, Vector3 dragVector, float amount)
    {
        IsAnimating = true;

        Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        var axis = directions.OrderByDescending(dir => Vector3.Dot(dragVector, dir)).First();

        _pivot.transform.localEulerAngles = new Vector3(axis.y, axis.x, axis.z) * amount;
    }

    private void RotateSlice(GameObject[] sliceCubes, Vector3 direction, float speed = .5f)
    {
        if (IsAnimating) return;
        IsAnimating = true;

        if (_pivot == null)
        {
            _pivot = new GameObject();
        }
        _pivot.transform.SetParent(transform);
        _pivot.transform.localRotation = Quaternion.identity;
        _pivot.transform.position = Vector3.zero;
        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(_pivot.transform);
        };

        _pivot.transform.DOComplete();
        _pivot.transform.DORotate(direction * 90, speed)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                foreach (var cube in sliceCubes)
                {
                    cube.transform.SetParent(transform);
                }
                IsAnimating = false;
            });
    }

    private Vector3 RoundVector(Vector3 value) => new Vector3(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));

    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private GameObject _pivot;
    private GameObject _touchCube;
    private GameObject[] _sliceCubes;
    private Vector3 _touchPoint, _localDragDirection;

    private bool _shuffle;
}
