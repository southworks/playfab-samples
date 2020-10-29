using UnityEngine;
using UnityEngine.UI;

public class Toggle : MonoBehaviour
{

    public bool isOn;
    public Color onColor;
    public Color offColor;
    public Image backgroundImage;
    public RectTransform toggle;
    public float handleOffset;
    public GameObject handle;
    private RectTransform handleTransform;
    private float onPosition;
    private float offPosition;
    private bool switching;
    private static float t;
    private static readonly float speed = 1;
    private Button toggleButton;

    public delegate void OnValueChangedHandler(bool newValue);
    public event OnValueChangedHandler OnValueChanged;

    public bool IsEnabled
    {
        get
        {
            return toggleButton != null && toggleButton.interactable;
        }
    }

    void Start()
    {
        InitializeUI();
        toggleButton = toggle.GetComponentInChildren<Button>();
        toggleButton.onClick.AddListener(Switch);
    }

    void Update()
    {
        if (switching)
        {
            UpdateUI();
            if (t > 1.0f)
            {
                switching = false;
                t = 0.0f;
                isOn = !isOn;
                OnValueChanged?.Invoke(isOn);
            }
        }
    }

    void Awake()
    {
        handleTransform = handle.GetComponent<RectTransform>();
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        float toggleSizeX = toggle.sizeDelta.x;
        offPosition = (toggleSizeX / 2) - (handleRect.sizeDelta.x / 2) - handleOffset;
        onPosition = offPosition * -1;
    }

    private void InitializeUI()
    {
        backgroundImage.color = isOn ? onColor : offColor;
        handleTransform.localPosition = isOn ? new Vector3(onPosition, 0f, 0f) : new Vector3(offPosition, 0f, 0f);
    }

    private void UpdateUI()
    {
        t += speed * Time.deltaTime;
        backgroundImage.color = isOn ? Color.Lerp(onColor, offColor, t) : Color.Lerp(offColor, onColor, t);
        handleTransform.localPosition = isOn ? MoveHandle(onPosition, offPosition) : MoveHandle(offPosition, onPosition);
    }

    private Vector3 MoveHandle(float startPosition, float endPosition)
    {
        return new Vector3(Mathf.Lerp(startPosition, endPosition, t += speed * Time.deltaTime), 0f, 0f);
    }

    public void Switch()
    {
        switching = true;
    }

    public void SetEnabled(bool value)
    {
        toggleButton.interactable = value;
    }
}
