using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class NoteCrasher : MonoBehaviour
{
    [Header("Components")]
    public Camera cam;
    public NoteSpawner spawner;
    public GameObject hCrashEffect;
    [Header("Aspect")]
    public Rect boundaries;
    public float thickness = 0f;
    [Header("Events")]
    public UnityEvent<float> onHorizontalCrash;
    public UnityEvent<float> onVerticalCrash;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundaries.center, boundaries.size);
        Gizmos.DrawWireCube(boundaries.center, boundaries.size - 2f * thickness * Vector2.one);
    }

    public void Reset()
    {
        cam = Camera.main;
        spawner = FindObjectOfType<NoteSpawner>(true);
        SetBoundariesOnCamera();
    }

    private void OnEnable()
    {
        spawner?.onMoveNotes?.AddListener(OnNotesMove);
        hCrashEffect?.SetActive(false);
    }

    private void OnDisable()
    {
        spawner?.onMoveNotes?.RemoveListener(OnNotesMove);
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
            if (IsNoteCrashing(note, out float hCrashTime, out float vCrashTime))
            {
                Vector2 notePosition = note.transform.position;
                if (!float.IsNaN(hCrashTime))
                {
                    onHorizontalCrash.Invoke(toTime - fromTime);
                    note.horizontalCrashDelay = toTime - hCrashTime;
                    CrashEffect(notePosition, horizontal : true);
                }
                if (!float.IsNaN(vCrashTime))
                {
                    onVerticalCrash.Invoke(toTime - fromTime);
                    note.verticalCrashDelay = toTime - vCrashTime;
                }
            }
        }
    }

    private bool IsNoteCrashing(NoteSpawn instance, out float horizontalCrashTime, out float verticalCrashTime)
    {
        horizontalCrashTime = float.NaN;
        verticalCrashTime = float.NaN;
        if (instance != null)
        {
            Vector2 noteStartPosition = (Vector2)instance.transform.position + instance.DisplayStart * Vector2.one;
            Vector2 noteEndPosition = (Vector2)instance.transform.position + instance.DisplayEnd * Vector2.one;
            Vector2 crashCoordinates = boundaries.min + (thickness - 1f) * Vector2.one;
            if (noteStartPosition.x <= crashCoordinates.x && noteEndPosition.x >= crashCoordinates.x)
            {
                horizontalCrashTime = instance.StartTime - (noteStartPosition.x - crashCoordinates.x) / spawner.TimeScale;
                if (instance.performance.IsPlayStateAt(horizontalCrashTime, NotePerformance.PlayState.PLAYED_CORRECTLY)) horizontalCrashTime = float.NaN;
            }
            if (noteStartPosition.y <= crashCoordinates.y && noteEndPosition.y >= crashCoordinates.y)
            {
                verticalCrashTime = instance.StartTime - (noteStartPosition.y - crashCoordinates.y) / spawner.TimeScale;
                if (instance.performance.IsPlayStateAt(verticalCrashTime, NotePerformance.PlayState.PLAYED_CORRECTLY)) verticalCrashTime = float.NaN;
            }
        }
        return !float.IsNaN(horizontalCrashTime) || !float.IsNaN(verticalCrashTime);
    }

    private void CrashEffect(Vector2 notePosition, bool horizontal)
    {
        GameObject effect = horizontal ? hCrashEffect : null;
        if (effect == null) return;
        Vector2 effectPosition = boundaries.min + (thickness - 1f) * Vector2.one;
        if (horizontal) effectPosition.y = notePosition.y;
        else effectPosition.x = notePosition.x;
        effect.transform.position = effectPosition;
        effect.SetActive(true);
    }
}
