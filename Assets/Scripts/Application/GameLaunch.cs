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

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        yield return new WaitForEndOfFrame();
        // Load scene
        currentOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        while (!currentOperation.isDone)
        {
            onLoadProgress.Invoke(currentOperation.progress);
            yield return null;
        }
        // Unload other scene
        currentOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
    }
}
