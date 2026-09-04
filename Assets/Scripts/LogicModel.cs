using System.Collections.Generic;
using UnityEngine.Events;

public static class LogicModel
{
    static public bool IsAnimating 
    {
        get => _isAnimating;
        set
        {
            if (_isAnimating != value) 
            {
                if (!value) 
                {
                    OnAnimationComplete.Invoke();
                }
            }
            _isAnimating = value;

        }
    }

    static public bool IsShuffeling { get; set; } = false;
    static public UnityEvent OnAnimationComplete = new();
    private static bool _isAnimating = false;
}

[System.Serializable]
public struct CubeData
{
    public UnityEngine.Vector3 Position;
    public UnityEngine.Vector3 Rotation;
    public string Name;
}

[System.Serializable]
public class CubeDataContainer
{
    public UnityEngine.Vector3 MasterRotation;
    public List<CubeData> Cubes = new();
}