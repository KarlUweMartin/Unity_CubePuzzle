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
            _generator.UpdateFaces();
            var face = DetectFace(Input.mousePosition);

            if (face.Item1 != null) 
            {
                _possibleSlices = GetCorrespondingSlices(face.Item1, face.Item2).ToList();

                foreach (var slice in _possibleSlices) 
                {
                    var sliceCubes = _generator.FaceGroups[slice];
                    foreach (var cube in sliceCubes)
                    {
                        cube.transform.DOScale(.8f, .15f).OnComplete(() =>
                        {
                            cube.transform.DOScale(1f, .3f);
                        });
                    }
                }
            }

        }
        else if (Input.touchCount > 0) // Detects touch input
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                var f = DetectFace(touch.position);
               // _> DO ME ALSO!
            }
        }

        if (_shuffle)
        {
            RandomRotation();
        }

        if (Input.GetKey(KeyCode.O))
        {
            _actuator.Move(Moves.D);
        }
    } 

    public void RandomRotation()
    {
        System.Random random = new System.Random();
        Moves[] moves = (Moves[])Enum.GetValues(typeof(Moves));
        Moves rndMove = moves[random.Next(moves.Length)];
        _actuator.Move(rndMove);
    }

    private void HandleDrag(DragDirections direction)
    {
        if (!_possibleSlices.Any()) return;

        if (direction == DragDirections.Up || direction == DragDirections.Down) 
        {
            if (_possibleSlices.Contains(Slices.Right_X)) 
            {
                
            }
        }

        if (direction == DragDirections.Left || direction == DragDirections.Right)
        {

        }

        _possibleSlices.Clear();
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

    private Slices[] GetCorrespondingSlices(GameObject cube, Faces clickedFace)
    {
        List<Slices> possibleSlices = new();

        foreach (var entry in _generator.FaceGroups)
        {
            if (entry.Value.Contains(cube))
            {
                possibleSlices.Add(entry.Key);
            }
        }

        /*
        var faceDirectionWorld = GetFaceNormal(clickedFace);
        var faceDirectionLocal = cube.transform.InverseTransformDirection(faceDirectionWorld);
        possibleSlices.RemoveAll(slice =>
            _puzzle.FaceGroups[slice].All(faceCube => IsMatchingLocalPosition(faceCube, faceDirectionLocal)));
        */

        Debug.Log($"Corresponding slices: {string.Join(",", possibleSlices)}");

        return possibleSlices.ToArray();
    }

    private Vector3 GetFaceNormal(Faces face)
    {
        return face switch
        {
            Faces.Back => Vector3.forward,
            Faces.Front => Vector3.back,
            Faces.Left => Vector3.left,
            Faces.Right => Vector3.right,
            Faces.Top => Vector3.up,
            Faces.Bottom => Vector3.down,
            _ => Vector3.zero
        };
    }

    private bool IsMatchingLocalPosition(GameObject cube, Vector3 faceDirectionLocal)
    {
        var localPos = cube.transform.localPosition;

        if (Mathf.Abs(faceDirectionLocal.x) > Mathf.Abs(faceDirectionLocal.y) &&
            Mathf.Abs(faceDirectionLocal.x) > Mathf.Abs(faceDirectionLocal.z))
        {
            return Mathf.RoundToInt(localPos.x) == Mathf.RoundToInt(faceDirectionLocal.x);
        }
        else if (Mathf.Abs(faceDirectionLocal.y) > Mathf.Abs(faceDirectionLocal.x) &&
                 Mathf.Abs(faceDirectionLocal.y) > Mathf.Abs(faceDirectionLocal.z))
        {
            return Mathf.RoundToInt(localPos.y) == Mathf.RoundToInt(faceDirectionLocal.y);
        }
        else
        {
            return Mathf.RoundToInt(localPos.z) == Mathf.RoundToInt(faceDirectionLocal.z);
        }
    }

    [SerializeField] private Toggle _shuffleButton;
    [SerializeField] private PuzzleActuator _actuator;
    [SerializeField] private PuzzleGenerator _generator;
    [SerializeField] private DragDetector _dragDetector;

    private List<Slices> _possibleSlices = new();
    private bool _shuffle;
}
