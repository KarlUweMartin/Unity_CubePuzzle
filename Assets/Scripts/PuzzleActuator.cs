using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static LogicModel;

public class PuzzleActuator : MonoBehaviour
{

    private void Start()
    {
        DOTween.Init();
    }

    public void Move(Moves move)
    {
        var moveMap = new Dictionary<Moves, (Slices, bool)>
        {
            { Moves.U, (Slices.Up_Y, true) },
            { Moves.Uw, (Slices.Up_Y, false) },

            { Moves.D, (Slices.Down_Y, false) },
            { Moves.Dw, (Slices.Down_Y, true) },

            { Moves.R, (Slices.Right_X, true) },
            { Moves.Rw, (Slices.Right_X, false) },

            { Moves.L, (Slices.Left_X, true) },
            { Moves.Lw, (Slices.Left_X, false) },

            { Moves.F, (Slices.Front_Z, false) },
            { Moves.Fw, (Slices.Front_Z, true) },

            { Moves.B, (Slices.Back_Z, false) },
            { Moves.Bw, (Slices.Back_Z, true) },

            { Moves.X, (Slices.Middle_X, false) },
            { Moves.Xw, (Slices.Middle_X, true) },

            { Moves.Y, (Slices.Equator_Y, true) },
            { Moves.Yw, (Slices.Equator_Y, false) },

            { Moves.Z, (Slices.Standing_Z, true) },
            { Moves.Zw, (Slices.Standing_Z, false) }
        };

        if (moveMap.TryGetValue(move, out var rotation))
        {
            RotateFace(rotation.Item1, rotation.Item2);
        }
    }

    private void RotateFace(Slices face, bool clockwise)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        List<GameObject> cubesToRotate = _generator.FaceGroups[face];
        if (_pivot == null)
        {
            _pivot = new GameObject("Pivot");
        }
        _pivot.transform.SetParent(transform);
        _pivot.transform.position = Vector3.zero;
        _pivot.transform.localRotation = Quaternion.identity;

        foreach (var cube in cubesToRotate)
        {
            cube.transform.SetParent(_pivot.transform);
        }

        Vector3 localAxis = face switch
        {
            Slices.Right_X => Vector3.right,
            Slices.Middle_X => Vector3.left,
            Slices.Left_X => Vector3.left,
            Slices.Up_Y => Vector3.up,
            Slices.Equator_Y => Vector3.up,
            Slices.Down_Y => Vector3.down,
            Slices.Front_Z => Vector3.forward,
            Slices.Standing_Z => Vector3.forward,
            Slices.Back_Z => Vector3.back,
            _ => Vector3.up
        };

        var targetAngle = clockwise ? 90f : -90f;

        _pivot.transform.DOComplete();
        _pivot.transform.DOLocalRotate(localAxis * targetAngle, 0.5f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                foreach (var cube in cubesToRotate)
                {
                    cube.transform.SetParent(transform);
                }

                _generator.UpdateFaces();
                _isAnimating = false;
            });

        Debug.Log(face + " " + targetAngle);
    }

    private GameObject _pivot;
    private bool _isAnimating = false;

    [SerializeField] private PuzzleGenerator _generator;
}