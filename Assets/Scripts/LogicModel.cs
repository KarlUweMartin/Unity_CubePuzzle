using UnityEngine.Events;
public static class LogicModel
{
    static public UnityEvent<int> OnStepCountChanged = new();

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
    static public UnityEvent OnAnimationComplete = new();
    private static bool _isAnimating = false;
}
