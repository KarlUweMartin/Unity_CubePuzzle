using System.Collections.Generic;
using UnityEngine;
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
                IsAnimationChanged.Invoke(value);
            }
            _isAnimating = value;

        }
    }

    static public bool IsShuffeling
    {
        get => _isShuffling;
        set
        {
            if (_isShuffling != value)
            {
                IsShuffelingChanged.Invoke(value);
            }
            _isShuffling = value;

        }
    }

    static public UnityEvent<bool> IsAnimationChanged = new();
    static public UnityEvent<bool> IsShuffelingChanged = new();

    static private bool _isAnimating = false;
    static private bool _isShuffling = false;

    static public Color[] FaceColors = {
        new Color32(0x4A, 0x48, 0x8A, 255),
        new Color32(0x5A, 0x71, 0x34, 255),
        new Color32(0xDB, 0xD9, 0xD1, 255),
        new Color32(0xF1, 0xAE, 0x18, 255),
        new Color32(0xC8, 0x64, 0x16, 255),
        new Color32(0xA8, 0x33, 0x22, 255)
    };
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