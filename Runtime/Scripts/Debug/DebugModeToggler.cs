using UnityEngine;
using TMPro;

public class DebugModeToggler : MonoBehaviour
{
    private int tapCount = 0;
    private float tapTimeLimit = 0.5f;
    private float lastTapTime = 0f;

    private float holdStartTime = 0f;
    private bool isHolding = false;
    private bool isDragging = false;

    public bool isDebugModeActive = false;

    [Header("Debug Menu Object's")]
    public GameObject Board;
    public GameObject RestartButton;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI FpsText;

    [Header("Fps Limiter")]
    public FPSLimit fpsLimit = FPSLimit.NoLimit;
    private float deltaTime = 0.0f;
    public enum FPSLimit
    {
        NoLimit = 0,
        Limit30 = 30,
        Limit60 = 60,
        Limit120 = 120,
        Limit240 = 240
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float currentFPS = 1.0f / deltaTime;
        FpsText.text = "Fps: " + Mathf.Ceil(currentFPS).ToString();

        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    HandleTap(timeSinceLastTap);
                    isHolding = true;
                    isDragging = false;
                    holdStartTime = Time.time;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    isDragging = true;
                    isHolding = false;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    isHolding = false;
                    isDragging = false;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                HandleTap(timeSinceLastTap);
                isHolding = true;
                isDragging = false;
                holdStartTime = Time.time;
            }

            lastTapTime = Time.time;
        }

        if (isHolding && !isDragging && Time.time - holdStartTime >= 2f)
        {
            DeactivateDebugMode();
            isHolding = false;
        }
    }

    private void HandleTap(float timeSinceLastTap)
    {
        if (timeSinceLastTap <= tapTimeLimit)
        {
            tapCount++;
        }
        else
        {
            tapCount = 1;
        }

        if (tapCount >= 3)
        {
            ToggleDebugMode();
            tapCount = 0;
        }
    }

    void Start()
    {
        SetFPSLimit(fpsLimit);
    }

    public void SetFPSLimit(FPSLimit limit)
    {
        Application.targetFrameRate = (int)limit;
    }

    private void ToggleDebugMode()
    {
        isDebugModeActive = !isDebugModeActive;
        if (isDebugModeActive)
        {
            debugText.enabled = true;
            FpsText.enabled = true;
            Board.SetActive(true);
            RestartButton.SetActive(true);
        }
    }

    private void DeactivateDebugMode()
    {
        isDebugModeActive = false;
        debugText.enabled = false;
        FpsText.enabled = false;
        Board.SetActive(false);
        RestartButton.SetActive(false);
    }
}
