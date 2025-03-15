using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static LogicModel;


public class PuzzleController : MonoBehaviour
{
    private void Start()
    {
        DOTween.Init();
        _shuffleButton.onValueChanged.AddListener((v) => { _shuffle = v; });
        _dragDetector.OnDrag.AddListener(HandleDrag);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _touchCube = hit.collider.gameObject;
            }
        }

        if (Input.GetMouseButtonUp(0)) 
        {

            foreach (var cube in _sliceCubes)
            {
                cube.transform.DOScale(1f, .05f);
            }
        }

        if (Input.touchCount > 0)
        {
            // TODO: Implement the same stuff for touch!
        }

        if (_shuffle)
        {
            RandomRotation();
        }

        if (Input.GetKey(KeyCode.O))
        {
            _generator.ResetCube(); 
        }
    } 

    public void RandomRotation()
    {
        /*System.Random random = new System.Random();
        Moves[] moves = (Moves[])Enum.GetValues(typeof(Moves));
        Moves rndMove = moves[random.Next(moves.Length)];
        _actuator.Move(rndMove, .1f);*/
    }

    private void HandleDrag(Vector3 dragVector)
    {
        if (_touchCube == null) return;
        _sliceCubes = _generator.GetSliceFromDirection(_touchCube, dragVector);

        if (_sliceCubes.Length == 0) return;

        foreach (var cube in _sliceCubes)
        {
            cube.transform.DOScale(1.1f, .05f);
        }

        var rotationDirection = Vector3.zero;
        if(dragVector == Vector3.up) rotationDirection = Vector3.left;
        if(dragVector == Vector3.down) rotationDirection = Vector3.right;
        if(dragVector == Vector3.left) rotationDirection = Vector3.down;
        if(dragVector == Vector3.right) rotationDirection = Vector3.up;
        if(dragVector == Vector3.forward) rotationDirection = Vector3.back;
        if(dragVector == Vector3.back) rotationDirection = Vector3.forward;

        if (rotationDirection == Vector3.zero) return;

        RotateSlice(_sliceCubes, rotationDirection);
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
        _pivot.transform.position = Vector3.zero;
        _pivot.transform.localRotation = Quaternion.identity;

        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(_pivot.transform);
        };

        _pivot.transform.DOComplete();
        _pivot.transform.DOLocalRotate(direction * 90, speed, RotateMode.LocalAxisAdd)
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
    private bool _shuffle;
}
