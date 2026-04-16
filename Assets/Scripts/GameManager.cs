using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode;

    [Header("UI")]
    [SerializeField] private TMP_Text startEventText;
    [SerializeField] private TMP_Text endEventText;
    [SerializeField] private TMP_Text guessEventText;

    private struct HistoricalEvent
    {
        public string Name;
        public int Year;
    }

    private List<HistoricalEvent> _events = new List<HistoricalEvent>();

    private void Start()
    {
        LoadEvents();
        StartRound();
    }

    private void LoadEvents()
    {
        TextAsset csv = Resources.Load<TextAsset>("Timeline - Entries");
        if (csv == null)
        {
            Debug.LogError("Could not load 'Timeline - Entries.csv' from Resources.");
            return;
        }

        string[] lines = csv.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            int commaIndex = line.LastIndexOf(',');
            if (commaIndex < 0) continue;

            string name = line.Substring(0, commaIndex).Trim();
            string yearStr = line.Substring(commaIndex + 1).Trim();

            if (int.TryParse(yearStr, out int year))
            {
                _events.Add(new HistoricalEvent { Name = name, Year = year });
            }
        }
    }

    public void StartRound()
    {
        if (_events.Count < 3)
        {
            Debug.LogError("Not enough events loaded to start a round (need at least 3).");
            return;
        }

        // Pick 3 unique indices
        List<int> indices = new List<int>();
        while (indices.Count < 3)
        {
            int candidate = Random.Range(0, _events.Count);
            if (!indices.Contains(candidate))
                indices.Add(candidate);
        }

        // Sort all three by year so the middle event is always the guess target
        List<HistoricalEvent> picked = new List<HistoricalEvent>
        {
            _events[indices[0]],
            _events[indices[1]],
            _events[indices[2]]
        };
        picked.Sort((x, y) => x.Year.CompareTo(y.Year));

        HistoricalEvent a = picked[0];
        HistoricalEvent guessEvent = picked[1];
        HistoricalEvent b = picked[2];

        startEventText.text = a.Name;
        endEventText.text = b.Name;
        guessEventText.text = guessEvent.Name;

        if (debugMode)
            Debug.Log($"Round started — Start: {a.Name} ({a.Year}), End: {b.Name} ({b.Year}), Guess: {guessEvent.Name} ({guessEvent.Year})");
    }
}

