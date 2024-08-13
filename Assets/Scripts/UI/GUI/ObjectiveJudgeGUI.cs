using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class ObjectiveJudgeGUI : GameUI
{
    [Header("UI Components")]
    public Transform objectiveContainer;
    [Header("Content")]
    public ObjectiveCheckPanel objectivePanelPrefab;
    public float displayObjectiveDuration = 2f;

    private ObjectiveJudge objectives;
    private Coroutine displayObjectiveCoroutine;

    public override Component[] UIComponents => new Component[]
        { objectiveContainer };


    public ObjectiveJudge Objectives
    {
        get => objectives;
        set
        {
            RemoveListeners(objectives);
            objectives = value;
            AddListeners(objectives);
            GUIActive = objectives != null;
        }
    }

    public override bool GUIActive
    {
        set
        {
            if (value != GUIActive)
            {
                StopAllCoroutines();
                displayObjectiveCoroutine = null;
                DestroyObjectivePanels();
            }
            base.GUIActive = value;
        }
    }

    private void AddListeners(ObjectiveJudge objJudge)
    {
        if (objJudge)
        {
            objJudge.onNewObjectiveComplete.AddListener(DisplayObjectivePanel);
        }
    }

    private void RemoveListeners(ObjectiveJudge objJudge)
    {
        if (objJudge)
        {
            objJudge.onNewObjectiveComplete.RemoveListener(DisplayObjectivePanel);
        }
    }
    public void DisplayObjectivePanel(ObjectiveInfo objectiveInfo)
    {
        if (objectivePanelPrefab == null || objectiveContainer == null) return;
        StartCoroutine(QueueDisplayObjectiveCoroutine(objectiveInfo));
    }

    public void DestroyObjectivePanels()
    {
        if (objectiveContainer == null) return;
        ObjectiveCheckPanel[] panels = objectiveContainer.GetComponentsInChildren<ObjectiveCheckPanel>(true);
        foreach (ObjectiveCheckPanel panel in panels) Destroy(panel.gameObject);
    }

    private IEnumerator QueueDisplayObjectiveCoroutine(ObjectiveInfo objectiveInfo)
    {
        while (displayObjectiveCoroutine != null) yield return null;
        displayObjectiveCoroutine = StartCoroutine(DisplayObjectiveCoroutine(objectiveInfo));
    }

    private IEnumerator DisplayObjectiveCoroutine(ObjectiveInfo objectiveInfo)
    {
        ObjectiveCheckPanel panel = Instantiate(objectivePanelPrefab, objectiveContainer);
        panel.SetText(objectiveInfo.name);
        panel.PlayNewlyCheckedAnimation();
        yield return new WaitForSeconds(displayObjectiveDuration);
        panel.PlayDisappearAnimation(destroyOnAnimationEnd: true);
        displayObjectiveCoroutine = null;
    }
}