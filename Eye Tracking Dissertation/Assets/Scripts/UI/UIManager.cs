using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{

    public ControlPC pc;
    // UI Elements
    [Header("UI Elements")]
    public Slider health;
    public Slider shields;
    public RectTransform HUDCentre;
    private Vector2 hudCentreSize;
    private Vector2 currentCentreSize;
    public Image[] centrePieces;
    public RectTransform sniperReticle;
    public RectTransform hudMessagePanel;
    public Text hudMessageBody;
    public Button hudMessageConfirm;

    // UI Animation
    [Header("Animation")]
    public AnimationCurve openingCurve;
    public AnimationCurve closingCurve;
    public Color clearColour;

    // UI Animation Timing
    [Header("UI Animation Timing")]
    public float animTimeHUDCentre = .5f;
    [HideInInspector]
    public bool isHUDActive;
    [HideInInspector]
    public bool hudMessage;



    void Start()
    {
        hudCentreSize = HUDCentre.sizeDelta;
        currentCentreSize = HUDCentre.sizeDelta *= 1.2f;
        HUDCentre.gameObject.SetActive(false);
        hudMessageConfirm.onClick.AddListener(CloseHUDMessage);
    }

    void Update()
    {
        if (!hudMessage && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (isHUDActive)
            {
                CloseHUDCentre();
            }
            else
            {
                OpenHUDCentre();
            }
        }
    }

    public void OpenHUDCentre()
    {
        isHUDActive = true;
        StopCoroutine("LerpHUDCentreClosed");
        currentCentreSize = HUDCentre.sizeDelta;
        StartCoroutine("LerpHUDCentreOpen");
    }

    IEnumerator LerpHUDCentreOpen()
    {
        HUDCentre.gameObject.SetActive(true);
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
        if (!pc.isEyeTracking) pc.StoppedFiring();
        float timer = 0;
        float progress = 0;

        while (progress < 1)
        {
            timer += Time.deltaTime;
            progress = timer / animTimeHUDCentre;
            HUDCentre.sizeDelta = Vector2.Lerp(currentCentreSize, hudCentreSize, openingCurve.Evaluate(progress));
            foreach (Image item in centrePieces)
            {
                item.color = Color.Lerp(clearColour, Color.white, progress);
            }
            yield return null;
        }
    }

    public void CloseHUDCentre()
    {
        StopCoroutine("LerpHUDCentreOpen");
        currentCentreSize = HUDCentre.sizeDelta;
        StartCoroutine("LerpHUDCentreClosed");
    }

    IEnumerator LerpHUDCentreClosed()
    {
        Vector3 startPos = HUDCentre.localScale;
        float timer = 0;
        float progress = 0;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        while (progress < 1)
        {
            timer += Time.deltaTime;
            progress = timer / animTimeHUDCentre;
            currentCentreSize = HUDCentre.sizeDelta = Vector2.Lerp(currentCentreSize, hudCentreSize * 1.2f, openingCurve.Evaluate(progress));
            foreach (Image item in centrePieces)
            {
                item.color = Color.Lerp(Color.white, clearColour, progress);
            }
            yield return null;
        }
        isHUDActive = false;
        HUDCentre.gameObject.SetActive(false);
    }

    public void OpenHUDMessage(string messBody)
    {
        GameManager.gm.pc.StoppedFiring();
        GameManager.gm.pc.LeaveSniperFOV();
        hudMessagePanel.gameObject.SetActive(true);
        hudMessageBody.text = messBody;
        hudMessage = true;

#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
    }

    public void CloseHUDMessage()
    {
        hudMessage = false;
        hudMessagePanel.gameObject.SetActive(false);
        if (GameManager.gm.hudMessagesToBeDisplayed.Count > 0)
        {
            GameManager.gm.DisplayNextMessage();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
