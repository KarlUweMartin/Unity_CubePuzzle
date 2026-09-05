using DG.Tweening;
using TMPro;
using UnityEngine;

public class InfoText : MonoBehaviour
{
    public void Show(string text, string subText = "", bool autoHide = true)
    {
        _grp.DOKill();
        _grp.alpha = 0;
        _text.text = text;
        _textSub.text = subText;
      
        _grp.DOFade(1, .2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            if (autoHide) 
            {  
                Hide(3f);
            }
        });
    }

    public void Hide(float delay = 0f)
    {
        _grp.DOKill();
        _grp.DOFade(0, .2f).SetEase(Ease.InOutCubic).SetDelay(delay).OnComplete(() =>
        {
            _grp.alpha = 0;
            _text.text = "";
            _textSub.text = "";
        });
    }


    [SerializeField] private TextMeshProUGUI _text, _textSub;
    [SerializeField] private CanvasGroup _grp;
}
