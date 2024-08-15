using UnityEngine;

public class AudienceSFXSource : MonoBehaviour
{
    public AudioClip idle;
    public AudioClip[] objectiveComplete;
    public AudioClip[] levelComplete;

    private AudioSource source;
    private LevelPlayer levelPlayer;
    private ObjectiveJudge objectiveJudge;


    private void Awake()
    {
        levelPlayer = FindObjectOfType<LevelPlayer>(true);
        source = GetComponent<AudioSource>();
        objectiveJudge = FindObjectOfType<ObjectiveJudge>(true);
    }

    private void OnEnable()
    {
        if (levelPlayer != null)
        {
            levelPlayer.onStartLoadLevel.AddListener(OnLoad);
            levelPlayer.onPlayLevel.AddListener(OnPlay);
            levelPlayer.onPauseLevel.AddListener(OnPause);
            levelPlayer.onEndLevel.AddListener(OnEnd);
            levelPlayer.onUnloadLevel.AddListener(OnUnload);
        }
        objectiveJudge?.onNewObjectiveComplete?.AddListener(OnObjectiveComplete);
    }

    private void OnDisable()
    {
        if (levelPlayer != null)
        {
            levelPlayer.onStartLoadLevel.RemoveListener(OnLoad);
            levelPlayer.onPlayLevel.RemoveListener(OnPlay);
            levelPlayer.onPauseLevel.RemoveListener(OnPause);
            levelPlayer.onEndLevel.RemoveListener(OnEnd);
            levelPlayer.onUnloadLevel.RemoveListener(OnUnload);
        }
        objectiveJudge?.onNewObjectiveComplete?.RemoveListener(OnObjectiveComplete);
    }

    private void OnLoad(Level l) => PlayLoop(idle);

    private void OnPlay() => StopLoop(idle);

    private void OnPause() => PlayLoop(idle);

    private void OnEnd()
    {
        PlayRandomOneShot(levelComplete);
        PlayLoop(idle);
    }

    private void OnUnload(Level l) => StopLoop(idle);

    private void OnObjectiveComplete(ObjectiveInfo o) => PlayRandomOneShot(objectiveComplete);

    private void PlayLoop(AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    private void StopLoop(AudioClip clip)
    {
        if (clip != null && source != null && source.isPlaying && source.clip == clip) source.Stop();
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    private void PlayRandomOneShot(AudioClip[] clips)
    {
        int clipCount = clips != null ? clips.Length : 0;
        if (clipCount > 0) PlayOneShot(clips[Random.Range(0, clipCount)]);
    }
}