using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CubePuzzleGenerator : MonoBehaviour
{
    public Dictionary<Slices, List<GameObject>> FaceGroups = new();

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

    public GameObject CubePrefab;

    private void Start()
    {
        DOTween.Init();
        Initiate();
    }

    public void ResetCube()
    {
        foreach (var c in _cubes)
        {
            Destroy(c);
        }
        Initiate();
    } 

    private void Initiate()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (x == 1 && y == 1 && z == 1) continue;

                    GameObject cube = Instantiate(CubePrefab, transform);
                    cube.transform.localPosition = new Vector3(x - 1, y - 1, z - 1);
                    cube.name = $"Cubie_{x}{y}{z}";

                    AddFaceColors(cube, x, y, z);
                    _cubes[x, y, z] = cube;
                }
            }
        }
        UpdateFaces();
    }

    private void AddFaceColors(GameObject cube, int x, int y, int z)
    {
        float offset = 0.45f;

        if (x == 2) CreateFace(cube, new Vector3(offset, 0, 0), Quaternion.Euler(0, -90, 0), _faceColors[0]);
        if (x == 0) CreateFace(cube, new Vector3(-offset, 0, 0), Quaternion.Euler(0, 90, 0), _faceColors[1]);

        if (y == 2) CreateFace(cube, new Vector3(0, offset, 0), Quaternion.Euler(90, 0, 0), _faceColors[2]);
        if (y == 0) CreateFace(cube, new Vector3(0, -offset, 0), Quaternion.Euler(-90, 0, 0), _faceColors[3]);

        if (z == 2) CreateFace(cube, new Vector3(0, 0, offset), Quaternion.Euler(0, 180, 0), _faceColors[4]);
        if (z == 0) CreateFace(cube, new Vector3(0, 0, -offset), Quaternion.Euler(0, 0, 0), _faceColors[5]);
    }

    private void CreateFace(GameObject parent, Vector3 localPosition, Quaternion rotation, Color color)
    {
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.transform.SetParent(parent.transform);
        face.transform.localPosition = localPosition;
        face.transform.localRotation = rotation;
        face.transform.localScale = Vector3.one * 0.9f;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        face.GetComponent<MeshRenderer>().material = mat;
        Destroy(face.GetComponent<Collider>());
    }

    public void UpdateFaces()
    {
        FaceGroups.Clear();
        foreach (Slices face in System.Enum.GetValues(typeof(Slices)))
        {
            FaceGroups[face] = new List<GameObject>();
        }

        foreach (var cube in _cubes)
        {
            if (cube == null) continue;
            Vector3 pos = cube.transform.localPosition;
            int x = Mathf.RoundToInt(pos.x + 1);
            int y = Mathf.RoundToInt(pos.y + 1);
            int z = Mathf.RoundToInt(pos.z + 1);

            if (x == 0) FaceGroups[Slices.Left_X].Add(cube);
            if (x == 1) FaceGroups[Slices.Middle_X].Add(cube);
            if (x == 2) FaceGroups[Slices.Right_X].Add(cube);

            if (y == 2) FaceGroups[Slices.Up_Y].Add(cube);
            if (y == 1) FaceGroups[Slices.Equator_Y].Add(cube);
            if (y == 0) FaceGroups[Slices.Down_Y].Add(cube);

            if (z == 0) FaceGroups[Slices.Front_Z].Add(cube);
            if (z == 1) FaceGroups[Slices.Standing_Z].Add(cube);
            if (z == 2) FaceGroups[Slices.Back_Z].Add(cube);
        }
    }

    public void RotateFace(Slices face, bool clockwise)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        List<GameObject> cubesToRotate = FaceGroups[face];
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
        _audio.PlayOneShot(_in);
        _pivot.transform.DOLocalRotate(localAxis * targetAngle, 0.5f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // Restore cube parents
                foreach (var cube in cubesToRotate)
                {
                    cube.transform.SetParent(transform);
                }

                UpdateFaces();
                _isAnimating = false;
                _audio.PlayOneShot(_out);
            });

        Debug.Log(face + " " + targetAngle);
    }

    private GameObject _pivot;
    private GameObject[,,] _cubes = new GameObject[3, 3, 3];
    private Color[] _faceColors = { Color.white, Color.yellow, Color.blue, Color.green, Color.red, new Color(1.0f, 0.5f, 0.0f) };
    private bool _isAnimating = false;

    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _in, _out;

}