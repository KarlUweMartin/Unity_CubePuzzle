using DG.Tweening;
using UnityEngine;
using static LogicModel;

public class PuzzleActuator : MonoBehaviour
{

    private void Start()
    {
        DOTween.Init();
    }

    public void RotateSlice(GameObject[] sliceCubes, Vector3 direction, float speed = .5f)
    {
        if (IsAnimating) return;
        IsAnimating = true;


        if (_pivot == null)
        {
            _pivot = new GameObject();
        }
        _pivot.transform.SetParent(transform);
        _pivot.transform.position = Vector3.zero;
        _pivot.transform.localRotation = Quaternion.identity;

        foreach (var cube in sliceCubes)
        {
            cube.transform.SetParent(_pivot.transform);
        };

        _pivot.transform.DOComplete();
        _pivot.transform.DOLocalRotate(direction * 90, speed, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                foreach (var cube in sliceCubes)
                {
                    cube.transform.SetParent(transform);
                }

                IsAnimating = false;
            });
    }

    private GameObject _pivot;

    [SerializeField] private PuzzleGenerator _generator;
}