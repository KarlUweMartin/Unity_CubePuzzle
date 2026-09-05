using System;
using UnityEngine;
using UnityEngine.UI;
using static LogicModel;

public class UiHandler : MonoBehaviour
{

    private void Start()
    {
        _shuffleToggle.onValueChanged.AddListener((value) => { IsShuffeling = value; });
        IsShuffelingChanged.AddListener(OnShuffleToggleChanged);
        IsAnimationChanged.AddListener(OnAnimatingChanged);

        _resetButton.onClick.AddListener(ResetPuzzle);
        _loadButton.onClick.AddListener(_gen.Load);
        _saveButton.onClick.AddListener(_gen.Save);
    }

    private void OnAnimatingChanged(bool animating)
    {
        _loadButton.interactable = !animating;
        _saveButton.interactable = !animating;
        _resetButton.interactable = !animating;
        if (!IsShuffeling)
        {
            _shuffleToggle.interactable = !animating;
        }
    }

    private void OnShuffleToggleChanged(bool isShuffling)
    {
        if (isShuffling)
        {
            _info.Show("Shuffeling ...", "", false);
        }
        else
        {
            _info.Hide();
        }

        _resetButton.interactable = !isShuffling;
        _saveButton.interactable = !isShuffling;
        _loadButton.interactable = !isShuffling;
    }

    private void ResetPuzzle()
    {
        _ctl.ResetPuzzle();
        _info.Show("Resetting puzzle", "I hope you saved...");
    }

    [SerializeField] private Toggle _shuffleToggle;
    [SerializeField] private Button _resetButton, _loadButton, _saveButton;
    [SerializeField] private InfoText _info;
    [SerializeField] private PuzzleController _ctl;
    [SerializeField] private PuzzleGenerator _gen;
}
