using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class SliderLabel : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private TMP_Text _label;

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
        UpdateLabel(slider.value);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        _label.text = Mathf.RoundToInt(value).ToString();
    }
}
