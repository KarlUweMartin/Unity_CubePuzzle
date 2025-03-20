using DG.Tweening;
using System;
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

        if (Input.GetKey(KeyCode.O))
        {
            _generator.ResetCube(); 
        }
    } 

    public void Randomize()
    {
        /*
        var rnd = new System.Random();
        var moves = (Slices[])Enum.GetValues(typeof(Slices));
        var chosenSlice = moves[rnd.Next(moves.Length)];
        var negative = rnd.Next(2) == 0;

        switch (chosenSlice)
        {
            case Slices.Right_X:
            case Slices.Left_X:
            case Slices.Middle_X:
                RotateSlice(chosenSlice, negative ? Vector3.left : Vector3.left, .1f);
                return;

            case Slices.Down_Y:
            case Slices.Up_Y:
            case Slices.Equator_Y:
                RotateSlice(chosenSlice, negative ? Vector3.down : Vector3.up, .1f);
                return;

            case Slices.Standing_Z:
            case Slices.Back_Z:
            case Slices.Front_Z:
                RotateSlice(chosenSlice, negative ? Vector3.back : Vector3.forward, .1f);
                return;
        }
        */
    }

    private void StartDrag(Vector3 dragVector)
    {
        Debug.Log("Drag start: " + dragVector);

        if (_touchCube == null || dragVector == Vector3.zero) return;
        _dragVector = dragVector;

        _sliceCubes = _generator.GetSliceCubes(_touchCube, dragVector, _touchPoint);
        if (_pivot == null)
        {
            _pivot = new GameObject("Rotation Pivot");
            _pivot.transform.SetParent(transform);
        }
        _pivot.transform.localRotation = Quaternion.identity;
        _pivot.transform.position = Vector3.zero;

        foreach (var cube in _sliceCubes) 
        {
            cube.transform.SetParent(_pivot.transform);
        }
    }

    private void UpdateDrag(float rotationAngle)
    {
        Debug.Log("Drag amount: " + rotationAngle);

        if(_sliceCubes == null || _dragVector == Vector3.zero) return;

        RotateAlongDrag(_sliceCubes, _dragVector, rotationAngle);
    }

    private void EndDrag()
    {
        Debug.Log("End drag");

        if (_sliceCubes == null) return;

        // Extract the local rotation Euler angles
        Vector3 eulerAngles = _pivot.transform.localEulerAngles;

        // Normalize angles to be within the range of -180 to 180
        eulerAngles.x = (eulerAngles.x > 180) ? eulerAngles.x - 360 : eulerAngles.x;
        eulerAngles.y = (eulerAngles.y > 180) ? eulerAngles.y - 360 : eulerAngles.y;
        eulerAngles.z = (eulerAngles.z > 180) ? eulerAngles.z - 360 : eulerAngles.z;

        // Determine the dominant axis (the one with the highest absolute value)
        float absX = Mathf.Abs(eulerAngles.x);
        float absY = Mathf.Abs(eulerAngles.y);
        float absZ = Mathf.Abs(eulerAngles.z);

        Vector3 alignedEuler = Vector3.zero;

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

        Debug.Log(alignedEuler);

        // Apply the rotation using DOTween
        _pivot.transform.DOLocalRotate(alignedEuler, .15f).OnComplete(() => 
        {
            foreach (var cube in _sliceCubes)
            {
                cube.transform.SetParent(_pivot.transform.parent);
            }

            _dragVector = Vector3.zero;
            _touchCube = null;
            _sliceCubes = null;
        });
    }

    void RotateAlongDrag(GameObject[] sliceCubes, Vector3 dragVector, float angle)
    {
        Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        var axis = directions.OrderByDescending(dir => Vector3.Dot(dragVector, dir)).First();

        _pivot.transform.localEulerAngles = new Vector3(axis.y, axis.x, axis.z) * angle;
    }

    public void RotateSlice(GameObject[] sliceCubes, Vector3 direction, float speed = .5f)
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


    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private GameObject _pivot;
    private GameObject _touchCube;
    private GameObject[] _sliceCubes;
    private Vector3 _touchPoint, _dragVector;

    private bool _shuffle;
}
