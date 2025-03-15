using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class PuzzleController : MonoBehaviour
{
    private void Start()
    {
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
                cube.GetComponent<MeshRenderer>().enabled = true;
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

        _actuator.RotateSlice(_sliceCubes, dragVector);
    }


    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private PuzzleActuator _actuator;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private GameObject _touchCube;
    private GameObject[] _sliceCubes;
    private bool _shuffle;
}
