using System.Collections.Generic;
using UnityEngine;
using static LogicModel;

public class PuzzleGenerator : MonoBehaviour
{
    private void Start()
    {
        Generate(false);
    }

    public void Save()
    {
        var container = new CubeDataContainer();
        for(int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.name.StartsWith("C_")) continue;

            container.MasterRotation = transform.rotation.eulerAngles;
            container.Cubes.Add(new CubeData
            {
                Position = child.position,
                Rotation = child.rotation.eulerAngles,
                Name = child.name
            });
        }

        var json = JsonUtility.ToJson(container, true);
        PlayerPrefs.SetString("PuzzleSave", json);

        var subText = "";
#if UNITY_WEBGL
        subText = "(if your browser allows it)";
#endif

        _info.Show("Puzzle saved", subText);
    }

    public void Load()
    {
        var json = PlayerPrefs.GetString("PuzzleSave", "");
        if(string.IsNullOrEmpty(json))
        {
            var subText = "Hit 'Save' to save the puzzle.";
#if UNITY_WEBGL
            subText = "Hit 'Save' to store the puzzle in your browsers cache.";
#endif

            _info.Show("No data", subText);
            return;
        }

        _info.Show("Loading...", "", false);
        _camSwipe.SwipeOut(() => {
            IsAnimating = true;
           var container = JsonUtility.FromJson<CubeDataContainer>(json);
           transform.eulerAngles = container.MasterRotation;
           foreach (var cube in container.Cubes)
           {
               var c = GameObject.Find(cube.Name);
               if (c != null)
               {
                   c.transform.position = cube.Position;
                   c.transform.eulerAngles = cube.Rotation;
               }
               else Debug.LogError($"Cube not found: {cube.Name} at position {cube.Position} with rotation {cube.Rotation}");
           }

           _camSwipe.SwipeIn(() => IsAnimating = true);
           _info.Hide();
       });
    }

    public void Generate(bool reset = true)
    {
        IsShuffeling = false;

        if (reset)
        {
            _camSwipe.SwipeOut(Generate);
        }
        else Generate();

        void Generate()
        {
            foreach (var c in _cubes) 
            {
                Destroy(c);
            }
            Initiate();
            _camSwipe.SwipeIn();
        }
    }

    public GameObject[] GetSliceCubes(GameObject singleCube, Vector3 dragVector, Vector3 touchPoint)
    {
        var localPos = singleCube.transform.position;
        var parentPos = singleCube.transform.parent.position;

        Ray ray = new(localPos, dragVector);
        var p2 = ray.GetPoint(2f);

        Vector3 p3 = singleCube.transform.parent.position;
        if ((localPos.x == 0 && localPos.y == 0) || (localPos.x == 0 && localPos.z == 0) || (localPos.y == 0 && localPos.z == 0)) // Center cubes
        {
            p3 = singleCube.transform.parent.position;
        }
        else
        {
            var distanceToCenter = Vector3.Distance(localPos, parentPos);
            if (distanceToCenter > 1.4f && distanceToCenter < 1.5f)
            {
                if (Physics.Raycast(ray, out var hit))
                {
                    Ray ray2 = new(hit.transform.position, dragVector);
                    if (Physics.Raycast(ray2, out var hit2)) // Middle cube with 2 neighbours in the drag direction
                    {
                        ray = new(parentPos, dragVector);
                        p3 = ray.GetPoint(1);
                    }
                    else // Middle cube with 1 neighbour in the drag direction
                    {
                        p3 = GetAxisCube(singleCube, dragVector, touchPoint).transform.position;
                    }
                }
                else // Middle cube with no neighbour in the drag direction
                {
                    ray = new(parentPos, dragVector);
                    p3 = ray.GetPoint(1);
                }
            }
            else if (distanceToCenter > 1.7f) // Corner cubes
            {
                p3 = GetAxisCube(singleCube, dragVector, touchPoint).transform.position;
            }
        }

        var slicePlane = new Plane(singleCube.transform.position, p2, p3);

        var axisProbe = new GameObject("AxisProbe");
        axisProbe.transform.position = singleCube.transform.position;
        axisProbe.transform.up = slicePlane.normal;
        axisProbe.transform.localScale = new Vector3(25f, 0.1f, 25f);
        axisProbe.AddComponent<BoxCollider>().isTrigger = true;

        var hitColliders = Physics.OverlapBox(
            axisProbe.transform.position,
            axisProbe.transform.lossyScale / 2,
            axisProbe.transform.rotation
        );

        Destroy(axisProbe);

        var cubes = new List<GameObject>();
        foreach (var collider in hitColliders)
        {
            if (collider.gameObject.CompareTag("SingleCube"))
            {
                cubes.Add(collider.gameObject);
            }
        }

        if (cubes.Count < 8 || cubes.Count > 9) return null;

        return cubes.ToArray();
    }


    public GameObject[] RandomSlice()
    {
        var rnd = new System.Random();

        var axisProbe = new GameObject("AxisProbe");
        axisProbe.transform.SetParent(transform);
        axisProbe.transform.localPosition = new Vector3(rnd.Next(-1, 1), rnd.Next(-1, 1), rnd.Next(-1, 1));
        axisProbe.transform.localEulerAngles = new Vector3(rnd.Next(-1, 1) * 90, rnd.Next(-1, 1) * 90, rnd.Next(-1, 1) * 90);
        axisProbe.transform.localScale = new Vector3(6f, 0.1f, 6f);
        axisProbe.AddComponent<BoxCollider>().isTrigger = true;

        var hitColliders = Physics.OverlapBox(
            axisProbe.transform.position,
            axisProbe.transform.lossyScale / 2,
            axisProbe.transform.rotation
        );

        Destroy(axisProbe);

        var cubes = new List<GameObject>();
        foreach (var collider in hitColliders)
        {
            if (collider.gameObject.CompareTag("SingleCube"))
            {
                cubes.Add(collider.gameObject);
            }
        }

        return cubes.ToArray();
    }


    private GameObject GetAxisCube(GameObject cube, Vector3 dragVector, Vector3 touchPoint)
    {

        var dragVectors = new[] { dragVector, -dragVector };
        List<GameObject> alignedCenterCubes = new();
        foreach (var direction in dragVectors)
        {
            if (Physics.Raycast(cube.transform.parent.position, direction, out var hit))
            {
                alignedCenterCubes.Add(hit.collider.gameObject);
            }
        }

        var x = Mathf.RoundToInt(cube.transform.localPosition.x);
        var y = Mathf.RoundToInt(cube.transform.localPosition.y);
        var z = Mathf.RoundToInt(cube.transform.localPosition.z);
        var centerCubes = new List<GameObject>();
        foreach (var otherCube in _cubes) 
        {
            if (otherCube == null) continue;

            if (otherCube.transform.localPosition == new Vector3(x, 0, 0) ||
                otherCube.transform.localPosition == new Vector3(0, y, 0) ||
                (otherCube.transform.localPosition == new Vector3(0, 0, z)))
            {
                if(alignedCenterCubes.Contains(otherCube)) continue;
                centerCubes.Add(otherCube);
            }
        }

        float longestDistance = 0;
        GameObject axisCube = null; 
        foreach (var centerCube in centerCubes) 
        {
            var dist = Vector3.Distance(touchPoint, centerCube.transform.position);
            if (dist > longestDistance) 
            {
                longestDistance = dist;
                axisCube = centerCube;
            }
        }

        return axisCube;
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

                    var cube = Instantiate(_cubePrefab, transform);
                    cube.tag = "SingleCube";
                    cube.transform.localPosition = new Vector3(x - 1, y - 1, z - 1);
                    cube.name = $"C_{x}{y}{z}";

                    AddFaceColors(cube, x, y, z);
                    _cubes[x, y, z] = cube;
                }
            }
        }
    }

    private void AddFaceColors(GameObject cube, int x, int y, int z)
    {
        float offset = 0.45f;

        if (x == 2) CreateFace(cube, new Vector3(offset, 0, 0), Quaternion.Euler(0, -90, 0), FaceColors[0]);
        if (x == 0) CreateFace(cube, new Vector3(-offset, 0, 0), Quaternion.Euler(0, 90, 0), FaceColors[1]);

        if (y == 2) CreateFace(cube, new Vector3(0, offset, 0), Quaternion.Euler(90, 0, 0), FaceColors[2]);
        if (y == 0) CreateFace(cube, new Vector3(0, -offset, 0), Quaternion.Euler(-90, 0, 0), FaceColors[3]);

        if (z == 2) CreateFace(cube, new Vector3(0, 0, offset), Quaternion.Euler(0, 180, 0), FaceColors[4]);
        if (z == 0) CreateFace(cube, new Vector3(0, 0, -offset), Quaternion.Euler(0, 0, 0), FaceColors[5]);
    }

    private void CreateFace(GameObject parent, Vector3 localPosition, Quaternion rotation, Color color)
    {
        var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.transform.SetParent(parent.transform);
        face.transform.localPosition = localPosition;
        face.transform.localRotation = rotation;
        face.transform.localScale = Vector3.one * 0.9f;

        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        face.GetComponent<MeshRenderer>().material = mat;
        Destroy(face.GetComponent<Collider>());
    }

    private GameObject[,,] _cubes = new GameObject[3, 3, 3];


    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private CamSwipe _camSwipe;
    [SerializeField] private InfoText _info;
}
