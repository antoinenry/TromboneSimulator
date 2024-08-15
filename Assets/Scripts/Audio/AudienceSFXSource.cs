using UnityEngine;

public class AudienceSFXSource : SFXSource
{
    public AudioClip idle;
    public AudioClip[] objectiveComplete;
    public AudioClip[] levelComplete;

    private LevelPlayer levelPlayer;
    private ObjectiveJudge objectiveJudge;


    protected override void Awake()
    {
        base.Awake();
        levelPlayer = FindObjectOfType<LevelPlayer>(true);
        objectiveJudge = FindObjectOfType<ObjectiveJudge>(true);
    }

    private void OnEnable()
    {
        if (levelPlayer != null)
        {
            levelPlayer.onStartLoadLevel.AddListener(OnLoadLevel);
            levelPlayer.onPlayLevel.AddListener(OnPlayLevel);
            levelPlayer.onPauseLevel.AddListener(OnPauseLevel);
            levelPlayer.onEndLevel.AddListener(OnEndLevel);
            levelPlayer.onUnloadLevel.AddListener(OnUnloadLevel);
        }
        objectiveJudge?.onNewObjectiveComplete?.AddListener(OnObjectiveComplete);
    }

    private void OnDisable()
    {
        if (levelPlayer != null)
        {
            levelPlayer.onStartLoadLevel.RemoveListener(OnLoadLevel);
            levelPlayer.onPlayLevel.RemoveListener(OnPlayLevel);
            levelPlayer.onPauseLevel.RemoveListener(OnPauseLevel);
            levelPlayer.onEndLevel.RemoveListener(OnEndLevel);
            levelPlayer.onUnloadLevel.RemoveListener(OnUnloadLevel);
        }
        objectiveJudge?.onNewObjectiveComplete?.RemoveListener(OnObjectiveComplete);
    }

    private void OnLoadLevel(Level l) => PlayLoop(idle);

    private void OnPlayLevel() => StopLoop(idle);

    private void OnPauseLevel() => PlayLoop(idle);

    private void OnEndLevel()
    {
        PlayRandomOneShot(levelComplete);
        PlayLoop(idle);
    }

    private void OnUnloadLevel(Level l) => StopLoop(idle);

    private void OnObjectiveComplete(ObjectiveInfo o) => PlayRandomOneShot(objectiveComplete);
}