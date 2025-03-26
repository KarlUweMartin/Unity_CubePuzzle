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
        _shuffleButton.onValueChanged.AddListener((value) => { IsShuffeling = value; });
        _resetButton.onClick.AddListener(ResetPuzzle);
        _dragDetector.OnDragStart.AddListener(StartDrag);
        _dragDetector.OnDragUpdate.AddListener(UpdateDrag);
        _dragDetector.OnDragEnd.AddListener(EndDrag);
    }

    private void Update()
    {
        if (!IsShuffeling) 
        {
            var inputPos = Vector2.zero;
            if (Input.GetMouseButtonDown(0))
            {
                inputPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            { 
                inputPos = Input.GetTouch(0).position;
            }

            var ray = Camera.main.ScreenPointToRay(inputPos);
            if (Physics.Raycast(ray, out var hit))
            {
                _touchCube = hit.collider.gameObject;
                _touchPoint = hit.point;
            }
        }
        else
        {
            Randomize();
        }
    }

    public void Randomize()
    {
        if (IsAnimating) return;

        _sliceCubes = _generator.RandomSlice();
        RotateSliceRandomly(_sliceCubes);
    }

    private void StartDrag(Vector3 dragVector)
    {
        if (_touchCube == null || dragVector == Vector3.zero) return;
        _sliceCubes = _generator.GetSliceCubes(_touchCube, dragVector, _touchPoint);

        HandlePivot();

        var commonAxis = GetCommonAxis(_sliceCubes);
        _localDragDirection = new Vector3(commonAxis.y, commonAxis.x, commonAxis.z);
    }

    private void UpdateDrag(float dragAmount)
    {
        if (_sliceCubes == null || _localDragDirection == Vector3.zero) 
        {
            Debug.LogWarning("This shouldn't happen...");
            return; 
        }

        RotateAlongDrag(_sliceCubes, _localDragDirection, dragAmount);
    }

    private void EndDrag()
    {
        if (_sliceCubes == null) return;

        var eulerAngles = _pivot.transform.localEulerAngles;

        var alignedEuler = Vector3.zero;
        if (eulerAngles.x != 0) 
        {
            alignedEuler = RoundVector(new Vector3(Mathf.Round(eulerAngles.x / 90) * 90, 0, 0));
        }

        if (eulerAngles.y != 0)
        {
            alignedEuler = RoundVector(new Vector3(0, Mathf.Round(eulerAngles.y / 90) * 90, 0));
        }

        if (eulerAngles.z != 0)
        {
            alignedEuler = RoundVector(new Vector3(0, 0, Mathf.Round(eulerAngles.z / 90) * 90));
        }

        _pivot.transform.DOLocalRotate(alignedEuler, .15f, RotateMode.FastBeyond360).OnComplete(HandleComplete);

        void HandleComplete()
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
        }
    }

    private void RotateAlongDrag(GameObject[] sliceCubes, Vector3 dragVector, float dragAmount)
    {
        IsAnimating = true;

        Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        var axis = directions.OrderByDescending(dir => Vector3.Dot(dragVector, dir)).First();

        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(_pivot.transform);
        };

        _pivot.transform.localEulerAngles = new Vector3(axis.y, axis.x, axis.z) * dragAmount;
    }

    private void RotateSliceRandomly(GameObject[] sliceCubes)
    {
        if (IsAnimating) return;
        IsAnimating = true;

        HandlePivot();

        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(_pivot.transform);
        };

        var commonAxis = GetCommonAxis(sliceCubes);

        _pivot.transform.DOComplete();
        _pivot.transform.DOLocalRotate(commonAxis * 90, .2f).OnComplete(() =>
        {
            foreach (var cube in sliceCubes)
            {
                cube.transform.SetParent(transform);
            }
            IsAnimating = false;
        });
    }

    private Vector3 GetCommonAxis(GameObject[] sliceGroup) 
    {
        var sameX = true;
        var sameY = true;
        var sameZ = true;
        var referencePosition = RoundVector(sliceGroup[0].transform.localPosition);

        foreach (var cube in sliceGroup)
        {
            var pos = RoundVector(cube.transform.localPosition);
            if (pos.x != referencePosition.x) sameX = false;
            if (pos.y != referencePosition.y) sameY = false;
            if (pos.z != referencePosition.z) sameZ = false;
        }

        if (sameX)
        {
            return Vector3.right;
        }
        else if (sameY)
        {
            return Vector3.up;
        }
        else if (sameZ)
        {
            return Vector3.back;
        }
        else 
        {
            Debug.LogWarning("This shouldn't happen...");
            return Vector3.zero; 
        }
    }

    private void HandlePivot() 
    {
        if (_pivot == null)
        {
            _pivot = new GameObject("Rotation Pivot");
            _pivot.transform.SetParent(transform);
            _pivot.transform.SetSiblingIndex(0);
        }
        _pivot.transform.localRotation = Quaternion.identity;
        _pivot.transform.position = Vector3.zero;
    }

    private void ResetPuzzle()
    {
        if (_pivot != null) 
        {
            _pivot.transform.DOComplete();
        }

        IsShuffeling = false;
        IsAnimating = false;
        _shuffleButton.SetIsOnWithoutNotify(false);

        _generator.Generate();
    }

    private Vector3 RoundVector(Vector3 value) => new(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));

    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private GameObject _pivot;
    private GameObject _touchCube;
    private GameObject[] _sliceCubes;
    private Vector3 _touchPoint, _localDragDirection;
}
