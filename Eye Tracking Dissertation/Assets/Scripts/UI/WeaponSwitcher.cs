using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSwitcher : MonoBehaviour
{
    public UIManager uiManager;
    public RectTransform panelChild;
    private Button thisButton;
    private EventTrigger eTrigger;
    private bool isFading = true;
    private bool hasSelectScaled;
    public float fadeTime = .25f;
    public float selectGrowTime = .2f;
    public ControlPC.WeaponType thisWeapon;


    void Start()
    {
        thisButton = GetComponent<Button>();
        eTrigger = GetComponent<EventTrigger>();
        AddEventTriggers();
    }

    private void AddEventTriggers()
    {
        // Add mouse over trigger
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => { ScaleOnMouseOver(); });
        eTrigger.triggers.Add(entry);

        // Add mouse exit trigger
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((eventData) => { ScaleOnMouseExit(); });
        eTrigger.triggers.Add(entry);

        // Add mouse click trigger
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => { WeaponSelected(); });
        eTrigger.triggers.Add(entry);
    }

    public void ScaleOnMouseOver()
    {
        if (!hasSelectScaled)
        {
            // TODO mouse-over audio
            StartCoroutine("GrowButton");
            StopCoroutine("ShrinkButton");
        }
    }

    IEnumerator GrowButton()
    {
        bool finishedScale = false;
        float timer = 0;
        float progress = 0;
        Vector3 newSize = new Vector3(1.2f, 1.2f, 1.2f);

        while (!finishedScale)
        {
            timer += Time.deltaTime;
            progress = timer / selectGrowTime;
            panelChild.localScale = Vector3.Lerp(Vector3.one, newSize, progress);

            if (progress >= 1) finishedScale = hasSelectScaled = true;
            yield return null;
        }
    }

    public void ScaleOnMouseExit()
    {
        if (hasSelectScaled)
        {
            StartCoroutine("ShrinkButton");
            StopCoroutine("GrowButton");
        }
    }

    IEnumerator ShrinkButton()
    {
        bool finishedScale = false;
        float timer = 0;
        float progress = 0;
        Vector3 oldSize = panelChild.localScale;

        while (!finishedScale)
        {
            timer += Time.deltaTime;
            progress = timer / selectGrowTime;
            panelChild.localScale = Vector3.Lerp(oldSize, Vector3.one, progress);

            if (progress >= 1)
            {
                finishedScale = true;
                hasSelectScaled = false;
            }
            yield return null;
        }
    }

    public void WeaponSelected()
    {
        uiManager.CloseHUDCentre();
        uiManager.pc.SwitchWeapon(thisWeapon);
    }
}
