using System.Collections.Generic;
using UnityEngine;
using static LogicModel;

public class PuzzleGenerator : MonoBehaviour
{
    public Dictionary<Slices, List<GameObject>> SliceGroups = new();

    private void Start()
    {
        ResetCube();
        OnAnimationComplete.AddListener(UpdateFaces);
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

                    GameObject cube = Instantiate(_cubePrefab, transform);
                    cube.transform.localPosition = new Vector3(x - 1, y - 1, z - 1);
                    cube.name = $"C_{x}{y}{z}";

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

    private void UpdateFaces()
    {
        SliceGroups.Clear();
        foreach (Slices face in System.Enum.GetValues(typeof(Slices)))
        {
            SliceGroups[face] = new List<GameObject>();
        }

        Vector3 forward = (Camera.main.transform.position - transform.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        foreach (var cube in _cubes)
        {
            if (cube == null) continue;

            Vector3 pos = cube.transform.position;
            Vector3 localPos = transform.InverseTransformPoint(pos);

            int x = Mathf.RoundToInt(localPos.x);
            int y = Mathf.RoundToInt(localPos.y);
            int z = Mathf.RoundToInt(localPos.z);

            if (x == -1) SliceGroups[Slices.Left_X].Add(cube);
            if (x == 0) SliceGroups[Slices.Middle_X].Add(cube);
            if (x == 1) SliceGroups[Slices.Right_X].Add(cube);

            if (y == 1) SliceGroups[Slices.Up_Y].Add(cube);
            if (y == 0) SliceGroups[Slices.Equator_Y].Add(cube);
            if (y == -1) SliceGroups[Slices.Down_Y].Add(cube);

            if (z == -1) SliceGroups[Slices.Front_Z].Add(cube);
            if (z == 0) SliceGroups[Slices.Standing_Z].Add(cube);
            if (z == 1) SliceGroups[Slices.Back_Z].Add(cube);
        }
    }

    private GameObject[,,] _cubes = new GameObject[3, 3, 3];
    private Color[] _faceColors = { Color.white, Color.yellow, Color.blue, Color.green, Color.red, new Color(1.0f, 0.5f, 0.0f) };
    [SerializeField] private GameObject _cubePrefab;
}
