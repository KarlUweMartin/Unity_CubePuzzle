using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public static class LogicModel 
{
    static public UnityEvent<int> OnStepCountChanged = new();
    static public UnityEvent<Moves> OnStepAdded = new();
    static public UnityEvent<Moves> OnStepBack = new();


    public static bool AnySteps() => _puzzleSteps.Count > 0;

    public static bool TryGetLastMove(out Moves lastMove)
    {
        lastMove = new();
        if (_puzzleSteps.Count <= 0)
        {
            return false;
        }
        else 
        {
            lastMove = _puzzleSteps[_puzzleSteps.Count - 1];
            return true;
        }
    }

    public static void AddMove(Moves move) 
    {
        _puzzleSteps.Add(move);

        OnStepAdded.Invoke(move);
        OnStepCountChanged.Invoke(_puzzleSteps.Count);
    }

    public static void MoveBack()
    {
        if (!_puzzleSteps.Any()) return;

        var lastStep = _puzzleSteps[_puzzleSteps.Count - 1];
        _puzzleSteps.Remove(lastStep);

        OnStepBack.Invoke(lastStep);
        OnStepCountChanged.Invoke(_puzzleSteps.Count);
    }

    public static void ClearMoves()
    {
        _puzzleSteps.Clear();

        OnStepCountChanged.Invoke(_puzzleSteps.Count);
    }

    public enum Slices
    {
        Left_X,
        Middle_X,
        Right_X,

        Up_Y,
        Equator_Y,
        Down_Y,

        Front_Z,
        Standing_Z,
        Back_Z,
    }

    public enum Moves
    {
        U, D, R, L, F, B, X, Y, Z, Xw, Yw, Zw, Uw, Dw, Rw, Lw, Fw, Bw,
    }

    public enum Faces
    {
        Front, Back, Left, Right, Top, Bottom
    }


    private static List<Moves> _puzzleSteps = new();
}
