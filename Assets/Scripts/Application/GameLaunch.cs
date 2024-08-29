using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLaunch : MonoBehaviour
{
    public string sceneToUnload = "LaunchScene";
    public string sceneToLoad = "MainScene";
    public UnityEvent<float> onLoadProgress;

    private AsyncOperation currentOperation;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Load scene
        currentOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!currentOperation.isDone)
        {
            onLoadProgress.Invoke(currentOperation.progress);
            yield return null;
        }
        // Unload other scene
        currentOperation = SceneManager.LoadSceneAsync(sceneToUnload);
        while (!currentOperation.isDone)
        {
            yield return null;
        }
    }
}
