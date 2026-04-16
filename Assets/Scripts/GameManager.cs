using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode;

    [Header("UI")]
    [SerializeField] private TMP_Text startEventText;
    [SerializeField] private TMP_Text endEventText;
    [SerializeField] private TMP_Text guessEventText;
    [SerializeField] private Slider guessSlider;
    [SerializeField] private Slider answerSlider;
    [SerializeField] private TMP_Text correctAnswerText;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text submitButtonText;
    [SerializeField] private TMP_Text scoreText;

    private struct HistoricalEvent
    {
        public string Name;
        public int Year;
    }

    private List<HistoricalEvent> _events = new List<HistoricalEvent>();
    private int _correctAnswer;
    private bool _awaitingContinue;
    private int _cumulativeScore;

    private void Start()
    {
        LoadEvents();
        _cumulativeScore = 0;
        UpdateScoreDisplay();
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

        // Calculate the correct answer as a 0-100 integer percentage
        float span = b.Year - a.Year;
        _correctAnswer = span > 0 ? Mathf.RoundToInt((guessEvent.Year - a.Year) / span * 100f) : 0;

        // Reset UI for new round
        startEventText.text = a.Name;
        endEventText.text = b.Name;
        guessEventText.text = guessEvent.Name;

        guessSlider.value = 50;
        guessSlider.interactable = true;

        answerSlider.value = 0;
        answerSlider.gameObject.SetActive(false);

        correctAnswerText.text = string.Empty;
        correctAnswerText.gameObject.SetActive(false);

        submitButtonText.text = "Submit";
        _awaitingContinue = false;

        if (debugMode)
            Debug.Log($"Round started — Start: {a.Name} ({a.Year}), End: {b.Name} ({b.Year}), Guess: {guessEvent.Name} ({guessEvent.Year}) | Correct: {_correctAnswer}");
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {_cumulativeScore}";
    }

    // Wire this to the submit/continue button's OnClick event
    public void OnSubmitButtonPressed()
    {
        if (_awaitingContinue)
        {
            StartRound();
        }
        else
        {
            guessSlider.interactable = false;

            answerSlider.value = _correctAnswer;
            answerSlider.gameObject.SetActive(true);

            int playerGuess = Mathf.RoundToInt(guessSlider.value);
            int distance = Mathf.Abs(playerGuess - _correctAnswer);
            int roundScore = (100 - distance) * (100 - distance);
            _cumulativeScore += roundScore;
            UpdateScoreDisplay();

            correctAnswerText.text = $"Correct Answer:\n{_correctAnswer}";
            correctAnswerText.gameObject.SetActive(true);

            submitButtonText.text = "Continue";
            _awaitingContinue = true;

            if (debugMode)
                Debug.Log($"Player guessed {playerGuess}, correct answer was {_correctAnswer}, round score: {roundScore}, total: {_cumulativeScore}");
        }
    }
}

