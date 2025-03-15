using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.UI;
using static LogicModel;

public class UserInterface : MonoBehaviour
{
    private void Start()
    {
        PlayerSettings.iOS.deferSystemGesturesMode = SystemGestureDeferMode.All;

        //OnStepAdded.AddListener(HandleNewStep);
        //OnStepBack.AddListener(HandleStepBack);
        OnStepCountChanged.AddListener(HandleStepCountChanged);
    }

    private void HandleStepBack(KeyValuePair<Slices, bool> step)
    {
        var last = _stepEntries.Last();
        Destroy(last);
        _stepEntries.Remove(last);
    }

    private void HandleStepCountChanged(int arg0)
    {
        if (arg0 <= 0) 
        {
            // DELETE ALL
        }
    }

    private void HandleNewStep(KeyValuePair<Slices, bool> step)
    {
        var entry = Instantiate(_stepEntryPrefab, _entriesList);

        var text = step.Value ? step.Key.ToString() : "- "+step.Key.ToString();

        entry.GetComponent<TextMeshProUGUI>().text = text;
        _stepEntries.Add(entry);
    }

    [SerializeField] private Transform _entriesList;
    [SerializeField] private GameObject _stepEntryPrefab;
    [SerializeField] private List<GameObject> _stepEntries = new();

    [SerializeField] private Button _revertButton;
}
