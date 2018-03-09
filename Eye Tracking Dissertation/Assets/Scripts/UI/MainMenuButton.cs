using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Tobii.Gaming;

public class MainMenuButton : MonoBehaviour
{
    public bool isEyeTracking;
    public int buildIndex;
    Button thisButton;
    EventTrigger eTrigger;
    bool hasSelectScaled;
    public float selectGrowTime;
    public float selectShrinkTime;
    public RectTransform rectToScale;
    public AnimationCurve lerpingCurve;
    Vector3 smallSize;
    Vector3 bigSize = new Vector3(2, 2, 2);

    // Tobii stuff
    private bool beenLookedAt;
    private GazePoint gp;
    private GraphicRaycaster gr;
    private List<RaycastResult> objectsHitTobii = new List<RaycastResult>();


    void Start()
    {
        smallSize = rectToScale.localScale;
        thisButton = GetComponent<Button>();
        eTrigger = GetComponent<EventTrigger>();
        AddEventTriggers();
    }

    private void Update()
    {
        gp = TobiiAPI.GetGazePoint();
        PointerEventData cursor = new PointerEventData(EventSystem.current)
        {
            position = gp.Screen
        };
        EventSystem.current.RaycastAll(cursor, objectsHitTobii);

        if (!beenLookedAt)
        {
            if (objectsHitTobii.Count > 0)
            {
                for (int i = 0; i < objectsHitTobii.Count; i++)
                {
                    if (objectsHitTobii[i].gameObject == gameObject)    // if looking at this button
                    {
                        beenLookedAt = true;
                        ScaleOnMouseOver();
                    }
                }
            }
        }
        else
        {
            if (objectsHitTobii.Count > 0)
            {
                beenLookedAt = false;
                for (int i = 0; i < objectsHitTobii.Count; i++)
                {
                    if (objectsHitTobii[i].gameObject == gameObject)    // if looking at this button keep current scaled state
                    {
                        beenLookedAt = true;
                        break;
                    }
                }

                if (!beenLookedAt)  // if looking elsewhere after scaling, scale back to original size
                {
                    ScaleOnMouseExit();
                }
            }
        }

        objectsHitTobii.Clear();
    }

    private void AddEventTriggers()
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();

        if (!isEyeTracking)
        {
            // Add mouse over trigger
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { ScaleOnMouseOver(); });
            eTrigger.triggers.Add(entry);

            // Add mouse exit trigger
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) => { ScaleOnMouseExit(); });
            eTrigger.triggers.Add(entry);
        }

        // Add mouse click trigger
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => { LevelSelected(); });
        eTrigger.triggers.Add(entry);
    }
    public void ScaleOnMouseOver()
    {
        // TODO mouse-over audio
        StartCoroutine("GrowButton");
        StopCoroutine("ShrinkButton");
    }

    IEnumerator GrowButton()
    {
        bool finishedScale = false;
        float timer = 0;
        float progress = 0;
        Vector3 startSize = rectToScale.localScale;

        while (!finishedScale)
        {
            timer += Time.deltaTime;
            progress = timer / selectGrowTime;
            rectToScale.localScale = Vector3.Lerp(startSize, bigSize, lerpingCurve.Evaluate(progress));

            if (progress >= 1) finishedScale = hasSelectScaled = true;
            yield return null;
        }
    }

    public void ScaleOnMouseExit()
    {
        StartCoroutine("ShrinkButton");
        StopCoroutine("GrowButton");
    }

    IEnumerator ShrinkButton()
    {
        bool finishedScale = false;
        float timer = 0;
        float progress = 0;
        Vector3 startSize = rectToScale.localScale;

        while (!finishedScale)
        {
            timer += Time.deltaTime;
            progress = timer / selectShrinkTime;
            rectToScale.localScale = Vector3.Lerp(startSize, smallSize, lerpingCurve.Evaluate(progress));

            if (progress >= 1)
            {
                finishedScale = true;
                hasSelectScaled = false;
            }
            yield return null;
        }
    }

    public void LevelSelected()
    {
        GameManager.gm.GoToScene(buildIndex);
    }
}
