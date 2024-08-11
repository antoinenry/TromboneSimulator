using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class NoteCrasher : MonoBehaviour
{
    [Header("Components")]
    public Camera cam;
    public NoteSpawner spawner;
    public ParticleSystem horizontalCrashEffect;
    public ParticleSystem verticalCrashEffect;
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
        if (spawner != null) spawner.onMoveNotes.AddListener(OnNotesMove);
    }

    private void OnDisable()
    {
        if (spawner != null) spawner.onMoveNotes.RemoveListener(OnNotesMove);
        horizontalCrashEffect.Stop();
        verticalCrashEffect.Stop();
    }

    private void Update()
    {
        if (cam != null) SetBoundariesOnCamera();
        if (spawner?.playHead != null && spawner.playHead.PlayingSpeed == 0)
        {
            horizontalCrashEffect.Pause();
            verticalCrashEffect.Pause();
        }
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
                    if (horizontalCrashEffect && !horizontalCrashEffect.isPlaying)
                    {
                        horizontalCrashEffect.transform.position = new(boundaries.xMin, notePosition.y, 0f);
                        horizontalCrashEffect.Play();
                    }
                }
                else
                {
                    horizontalCrashEffect.Stop();
                }
                if (!float.IsNaN(vCrashTime))
                {
                    onVerticalCrash.Invoke(toTime - fromTime);
                    note.verticalCrashDelay = toTime - vCrashTime;
                    if (verticalCrashEffect && !verticalCrashEffect.isPlaying)
                    {
                        verticalCrashEffect.transform.position = new(notePosition.x, boundaries.yMin, 0f);
                        verticalCrashEffect.Play();
                    }
                }
                else
                {
                    verticalCrashEffect.Stop();
                }
            }
            else
            {
                horizontalCrashEffect.Stop();
                verticalCrashEffect.Stop();
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
}
