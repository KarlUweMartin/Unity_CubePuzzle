using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static LogicModel;

public class PuzzleController : MonoBehaviour
{
    private void Start()
    {
        DOTween.Init();
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

    public void ResetPuzzle()
    {
        if (_pivot != null)
        {
            _pivot.transform.DOComplete();
        }

        IsShuffeling = false;
        IsAnimating = false;

        _generator.Generate();
    }

    private void StartDrag(Vector3 dragVector)
    {
        if (_touchCube == null || dragVector == Vector3.zero) return;
        _sliceCubes = _generator.GetSliceCubes(_touchCube, dragVector, _touchPoint);

        var pivot = HandlePivot();
        foreach (var cube in _sliceCubes)
        {
            cube.transform.SetParent(pivot.transform);
        };

        _localDragDirection = GetCommonAxis(_sliceCubes);

        // Translate 2D drag direction to 3D rotation axis
        if (_localDragDirection != Vector3.zero)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var axisWorld = transform.TransformDirection(GetCommonAxis(_sliceCubes));
                var screenCenter = cam.WorldToScreenPoint(pivot.transform.position);
                var screenAxisPoint = cam.WorldToScreenPoint(pivot.transform.position + axisWorld);
                var screenDragPoint = cam.WorldToScreenPoint(pivot.transform.position + dragVector);

                var screenAxis = screenAxisPoint - screenCenter;
                var screenDrag = screenDragPoint - screenCenter;

                var crossZ = screenDrag.x * screenAxis.y - screenDrag.y * screenAxis.x;
                var sign = Mathf.Approximately(crossZ, 0f) ? 1f : Mathf.Sign(crossZ);

                if (Mathf.Approximately(sign, 0f))
                {
                    var fallback = Vector3.Cross(cam.transform.forward, axisWorld);
                    sign = Mathf.Sign(Vector3.Dot(fallback, dragVector));
                    if (Mathf.Approximately(sign, 0f)) sign = 1f;
                }

                _localDragDirection = GetCommonAxis(_sliceCubes) * -sign;
            }
        }
    }

    private void UpdateDrag(float dragAmount)
    {
        if (_sliceCubes == null || _localDragDirection == Vector3.zero || _pivot == null) 
        {
            return; 
        }

        IsAnimating = true;
        _pivot.transform.localEulerAngles = _localDragDirection * dragAmount;
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

        _pivot.transform.DOLocalRotate(alignedEuler, .15f, RotateMode.FastBeyond360).OnComplete(() => 
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

    private void RotateSliceRandomly(GameObject[] sliceCubes)
    {
        if (IsAnimating) return;
        IsAnimating = true;

        var pivot = HandlePivot();
        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(pivot.transform);
        };

        var commonAxis = GetCommonAxis(sliceCubes);
        pivot.transform.DOComplete();
        pivot.transform.DOLocalRotate(commonAxis * 90, .2f).OnComplete(() =>
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
            return referencePosition.x > 0 ? Vector3.right : Vector3.left;
        }
        else if (sameY)
        {
            return referencePosition.y > 0 ? Vector3.up : Vector3.down;
        }
        else if (sameZ)
        {
            return referencePosition.z > 0 ? Vector3.forward : Vector3.back;
        }
        else
        {
            return Vector3.zero; 
        }
    }

    private Transform HandlePivot() 
    {
        if (_pivot == null)
        {
            _pivot = new GameObject("Rotation Pivot");
            _pivot.transform.SetParent(transform);
            _pivot.transform.SetSiblingIndex(0);
        }
        _pivot.transform.localRotation = Quaternion.identity;
        _pivot.transform.position = Vector3.zero;

        return _pivot.transform;
    }

    private Vector3 RoundVector(Vector3 value) => new(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));

    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private GameObject _pivot;
    private GameObject _touchCube;
    private GameObject[] _sliceCubes;
    private Vector3 _touchPoint, _localDragDirection;
}
