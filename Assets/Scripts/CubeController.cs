using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CubePuzzleGenerator;
using static DragDetector;


public class CubeController : MonoBehaviour
{
    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private CubePuzzleGenerator _gen;
    [SerializeField] private DragDetector _drag;
    private Slices[] _possibleSlices;


    private void Start()
    {
        _drag.OnDrag.AddListener(HandleDrag);

        _shuffleButton.onValueChanged.AddListener((v) => { _shuffle = v; });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detects mouse click
        {
            DetectObject(Input.mousePosition);
        }
        else if (Input.touchCount > 0) // Detects touch input
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                DetectObject(touch.position);
            }
        }

        if (_shuffle)
        {
            RandomizeFaceRotation(.5f);
        }

        if (Input.GetKey(KeyCode.O))
        {
            _gen.ResetCube();
        }
    }

    void DetectObject(Vector2 inputPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // Ignore UI clicks
        }

        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            _possibleSlices = GetCorrespondingFaces(hit.collider.gameObject);
            foreach (var f in _possibleSlices) 
            {
                Debug.Log(f);
            }
        } else _possibleSlices = null;
    }

    private void HandleDrag(DragDirections direction)
    {
        if (_possibleSlices == null) return;

        if (direction == DragDirections.Up || direction == DragDirections.Down)
        {
            foreach (var cube in _possibleSlices) 
            {
                if (cube == Slices.Right_X || cube == Slices.Left_X || cube == Slices.Back_Z || cube == Slices.Standing_Z || cube == Slices.Middle_X) 
                {
                    _gen.RotateFace(cube, direction == DragDirections.Up);
                }
            }
        }
        else if (direction == DragDirections.Left || direction == DragDirections.Right) 
        {
            foreach (var cube in _possibleSlices)
            {
                if (cube == Slices.Up_Y || cube == Slices.Down_Y || cube == Slices.Equator_Y)
                {
                    _gen.RotateFace(cube, direction == DragDirections.Left);
                }
            }
        }
        
    }

    public void RandomizeFaceRotation(float speed)
    {
        System.Random random = new System.Random();
        Slices[] faces = (Slices[])Enum.GetValues(typeof(Slices));
        Slices randomFace = faces[random.Next(faces.Length)];
        bool randomDirection = random.Next(2) == 0;
        _gen.RotateFace(randomFace, randomDirection);
    }

    private Slices[] GetCorrespondingFaces(GameObject cube)
    {
        _gen.UpdateFaces();

        List<Slices> possibleSlices = new();
        foreach (var entry in _gen.FaceGroups)
        {
            if (entry.Value.Contains(cube))
            {
                possibleSlices.Add(entry.Key);
            }
        }

        return possibleSlices.ToArray();
    }

    private bool _shuffle;
}
