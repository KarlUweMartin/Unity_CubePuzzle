using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using static LogicModel;

public class CamSwipe : MonoBehaviour
{
    public void SwipeIn(UnityAction onComplete = null) 
    {
        transform.position = new Vector3(0, 10, 15);
        transform.DOMoveY(0, .6f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
            IsAnimating = false;
        });
    }

    public void SwipeOut(UnityAction onComplete = null)
    {
        transform.position = new Vector3(0, 0, 15);
        IsAnimating = true;
        transform.DOMoveY(10, 1.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}
