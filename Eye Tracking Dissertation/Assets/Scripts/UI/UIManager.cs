using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.Gaming;
using UnityEngine.EventSystems;

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
    public RectTransform reticle;
    public RectTransform sniperScope;
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

    // Tobii stuff
    [Header("Tobii")]
    public RectTransform centreInput;
    public RectTransform topInput;
    public RectTransform leftInput;
    public RectTransform rightInput;
    public RectTransform bottomInput;
    private GazePoint gp;
    private GraphicRaycaster gr;
    private bool searchingForInput;
    private bool edgeInputReceived;
    private bool initialCentreInputReceived;
    private bool secondCentreInputRecieved;
    private List<RaycastResult> objectsHitTobii = new List<RaycastResult>();
    private enum Directions
    {
        top,
        left,
        right,
        bottom
    }
    private Directions directionLooked;


    void Start()
    {
        hudCentreSize = HUDCentre.sizeDelta;
        currentCentreSize = HUDCentre.sizeDelta *= 1.2f;
        HUDCentre.gameObject.SetActive(false);
        hudMessageConfirm.onClick.AddListener(CloseHUDMessage);
        gr = GetComponent<GraphicRaycaster>();
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

        if ((isHUDActive || hudMessage) && pc.isEyeTracking && searchingForInput)
        {
            gp = TobiiAPI.GetGazePoint();
            CheckGazePointPos();
        }
    }

    #region Lerp HUD Centre

    public void OpenHUDCentre()
    {
        isHUDActive = true;

        if (pc.isEyeTracking)
        {
            searchingForInput = true;
            initialCentreInputReceived = secondCentreInputRecieved = edgeInputReceived = false;
        }
        else
        {
#if UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
        }

        StopCoroutine("LerpHUDCentreClosed");
        currentCentreSize = HUDCentre.sizeDelta;
        StartCoroutine("LerpHUDCentreOpen");
    }

    IEnumerator LerpHUDCentreOpen()
    {
        HUDCentre.gameObject.SetActive(true);
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
        if (pc.isEyeTracking)
        {
            searchingForInput = false;
        }

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
        if (pc.isEyeTracking) searchingForInput = false;
        HUDCentre.gameObject.SetActive(false);
    }

    #endregion

    #region HUD Message

    public void OpenHUDMessage(string messBody)
    {
        GameManager.gm.pc.StoppedFiring();
        GameManager.gm.pc.LeaveSniperFOV();
        hudMessagePanel.gameObject.SetActive(true);
        hudMessageBody.text = messBody;
        hudMessage = true;

        if (pc.isEyeTracking)
        {
            searchingForInput = true;
            initialCentreInputReceived = secondCentreInputRecieved = edgeInputReceived = false;
        }
        else
        {
#if UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
        }
    }

    public void CloseHUDMessage()
    {
        hudMessage = false;
        searchingForInput = false;
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
    #endregion

    #region Tobii

    void CheckGazePointPos()            // credit: https://answers.unity.com/questions/844158/how-do-you-perform-a-graphic-raycast.html
    {
        PointerEventData cursor = new PointerEventData(EventSystem.current)
        {
            position = gp.Screen
        };
        // This section prepares a list for all objects hit with the raycast
        EventSystem.current.RaycastAll(cursor, objectsHitTobii);
        if (objectsHitTobii.Count > 0)
        {
            if (!initialCentreInputReceived)
            {
                for (int i = 0; i < objectsHitTobii.Count; i++)
                {
                    if (objectsHitTobii[i].gameObject == centreInput.gameObject)    // if player has looked at centre input piece
                    {
                        initialCentreInputReceived = true;
                    }
                }
            }
            else if (!edgeInputReceived)
            {
                for (int i = 0; i < objectsHitTobii.Count; i++)
                {
                    if (objectsHitTobii[i].gameObject == topInput.gameObject)    // if player has looked at top input piece
                    {
                        edgeInputReceived = true;
                        directionLooked = Directions.top;
                    }
                    else if (objectsHitTobii[i].gameObject == leftInput.gameObject)    // if player has looked at left input piece
                    {
                        edgeInputReceived = true;
                        directionLooked = Directions.left;
                    }
                    else if (objectsHitTobii[i].gameObject == rightInput.gameObject)    // if player has looked at right input piece
                    {
                        edgeInputReceived = true;
                        directionLooked = Directions.right;
                    }
                    else if (objectsHitTobii[i].gameObject == bottomInput.gameObject)    // if player has looked at bottom input piece
                    {
                        edgeInputReceived = true;
                        directionLooked = Directions.bottom;
                    }
                }
            }
            else if (!secondCentreInputRecieved)
            {
                for (int i = 0; i < objectsHitTobii.Count; i++)
                {
                    if (objectsHitTobii[i].gameObject == centreInput.gameObject)    // if player has looked at centre input piece
                    {
                        if (hudMessage) // if a hud message is open, close after these inputs
                        {
                            CloseHUDMessage();
                        }
                        else    // otherwise choose weapon
                        {
                            InputSelectedTobii();
                        }
                    }
                }
            }

            objectsHitTobii.Clear();
        }
    }

    public void InputSelectedTobii()
    {
        switch (directionLooked)
        {
            case Directions.top:
                break;
            case Directions.left:
                pc.SwitchWeapon(ControlPC.WeaponType.AssaultRifle);
                CloseHUDCentre();
                break;
            case Directions.right:
                pc.SwitchWeapon(ControlPC.WeaponType.Launcher);
                CloseHUDCentre();
                break;
            case Directions.bottom:
                pc.SwitchWeapon(ControlPC.WeaponType.Sniper);
                CloseHUDCentre();
                break;
        }
    }

    #endregion
}
