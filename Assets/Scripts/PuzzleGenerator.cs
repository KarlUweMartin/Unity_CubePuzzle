using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LogicModel;

public class PuzzleGenerator : MonoBehaviour
{
    private void Start()
    {
        ResetCube();
    }

    public void ResetCube()
    {
        IsAnimating = false;
        foreach (var c in _cubes)
        {
            Destroy(c);
        }
        Initiate();
    }

    public GameObject RandomCube() 
    {
        var rnd = new System.Random().Next(0, _cubes.Length-1);
        int i = 0;

        foreach (var cube in _cubes) 
        {
            if (cube == null) continue;

            i++;
            if(i == rnd) return cube;
        }

        return _cubes[1,1,1];
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

    private GameObject[,,] _cubes = new GameObject[3, 3, 3];
    private Color[] _faceColors = { Color.white, Color.yellow, Color.blue, Color.green, Color.red, new Color(1.0f, 0.5f, 0.0f) };
    [SerializeField] private GameObject _cubePrefab;
}
