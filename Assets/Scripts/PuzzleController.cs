using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static DragDetector;
using static LogicModel;


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
            var face = DetectFace(Input.mousePosition);

            if (face.Item1 != null) 
            {
                GetCorrespondingSlices(face.Item1, face.Item2);
                if(!_possibleSlices.Any()) return;

                foreach (var slice in _possibleSlices) 
                {
                    var sliceCubes = _generator.SliceGroups[slice];
                    foreach (var cube in sliceCubes)
                    {
                        //cube.transform.DOScale(.8f, .15f);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            if (!_possibleSlices.Any()) return;

            foreach (var slice in _possibleSlices)
            {
                var sliceCubes = _generator.SliceGroups[slice];
                foreach (var cube in sliceCubes)
                {
                    //cube.transform.DOScale(1f, .3f);
                }
            }

            _possibleSlices.Clear();
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
            _actuator.Move(Moves.D, .5f);
        }
    } 

    public void RandomRotation()
    {
        System.Random random = new System.Random();
        Moves[] moves = (Moves[])Enum.GetValues(typeof(Moves));
        Moves rndMove = moves[random.Next(moves.Length)];
        _actuator.Move(rndMove, .1f);
    }

    private void HandleDrag(Vector3 direction)
    {
        Debug.Log(direction);
    }

    private (GameObject, Faces) DetectFace(Vector2 inputPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 normalInCameraSpace = Camera.main.transform.InverseTransformDirection(hit.normal);

            Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
            Faces[] faces = { Faces.Right, Faces.Top, Faces.Back, Faces.Left, Faces.Bottom, Faces.Front };

            int bestIndex = 0;
            float maxDot = float.NegativeInfinity;

            for (int i = 0; i < axes.Length; i++)
            {
                float dot = Vector3.Dot(normalInCameraSpace, axes[i]);
                float negDot = Vector3.Dot(normalInCameraSpace, -axes[i]);

                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestIndex = i;
                }

                if (negDot > maxDot)
                {
                    maxDot = negDot;
                    bestIndex = i + 3;
                }
            }

            Faces face = faces[bestIndex];

            Debug.Log($"Clicked on face: {face}");
            return (hit.transform.gameObject, face);
        }

        return (null, Faces.Front);
    }

    private void GetCorrespondingSlices(GameObject cube, Faces clickedFace)
    {
        _possibleSlices.Clear();

        foreach (var entry in _generator.SliceGroups)
        {
            if (entry.Value.Contains(cube))
            {
                _possibleSlices.Add(entry.Key);
            }
        }

        switch (clickedFace) 
        {
            case Faces.Front: _possibleSlices.Remove(Slices.Front_Z); break;
            case Faces.Back: _possibleSlices.Remove(Slices.Back_Z); break;
            case Faces.Left: _possibleSlices.Remove(Slices.Left_X); break;
            case Faces.Right: _possibleSlices.Remove(Slices.Right_X); break;
            case Faces.Bottom: _possibleSlices.Remove(Slices.Down_Y); break;
            case Faces.Top: _possibleSlices.Remove(Slices.Up_Y); break;
        }

        Debug.Log($"Corresponding slices: {string.Join(",", _possibleSlices)}");
    }

    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private PuzzleActuator _actuator;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private List<Slices> _possibleSlices = new();
    private bool _shuffle;
}
