using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class NoteCrasher : MonoBehaviour
{
    [Header("Components")]
    public Camera cam;
    public NoteSpawner spawner;
    [Header("Dimensions")]
    public Rect boundaries;
    [Header("Events")]
    public UnityEvent<float> onHorizontalCrash;
    public UnityEvent<float> onVerticalCrash;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundaries.center, boundaries.size);
    }

    private void Reset()
    {
        cam = Camera.main;
        spawner = FindObjectOfType<NoteSpawner>(true);
        SetBoundariesOnCamera();
    }

    private void OnEnable()
    {
        if (spawner != null) spawner.onMoveNotes.AddListener(OnNotesMove);
    }

    private void OnDisable()
    {
        if (spawner != null) spawner.onMoveNotes.RemoveListener(OnNotesMove);
    }

    private void Update()
    {
        if (cam != null) SetBoundariesOnCamera();
    }

    private void SetBoundariesOnCamera()
    {
        boundaries.size = 2f * cam.orthographicSize * new Vector2(cam.aspect, 1f);
        boundaries.center = cam.transform.position;
    }

    private void OnNotesMove(NoteSpawn[] instances, float fromTime, float toTime)
    {
        foreach (NoteSpawn note in instances)
        {
            if (IsNoteCrashing(note, out bool hCrash, out bool vCrash))
            {
                if (hCrash) onHorizontalCrash.Invoke(toTime - fromTime);
                if (vCrash) onVerticalCrash.Invoke(toTime - fromTime);
            }
        }
    }

    private bool IsNoteCrashing(NoteSpawn instance, out bool horizontalCrash, out bool verticalCrash)
    {
        horizontalCrash = false;
        verticalCrash = false;
        if (instance != null)
        {
            Vector2 noteStartPosition = (Vector2)instance.transform.position + instance.DisplayDistance * Vector2.one;
            Vector2 noteEndPosition = (Vector2)instance.transform.position + (instance.DisplayDistance + instance.DisplayLength) * Vector2.one;
            if (noteStartPosition.x <= boundaries.min.x && noteEndPosition.x >= boundaries.min.x)
            {
                float crashTime = instance.StartTime - (noteStartPosition.x - boundaries.min.x) / spawner.TimeScale;
                horizontalCrash = !instance.performance.IsPlayStateAt(crashTime, NotePerformance.PlayState.PLAYED_CORRECTLY);
            }
            if (noteStartPosition.y <= boundaries.min.y && noteEndPosition.y >= boundaries.min.y)
            {
                float crashTime = instance.StartTime - (noteStartPosition.y - boundaries.min.y) / spawner.TimeScale;
                verticalCrash = !instance.performance.IsPlayStateAt(crashTime, NotePerformance.PlayState.PLAYED_CORRECTLY);
            }
        }
        return horizontalCrash || verticalCrash;
    }
}
