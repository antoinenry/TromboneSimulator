using UnityEngine;

public class CheatUnlock : MonoBehaviour
{
    public KeyCode unlockKey = KeyCode.U;
    public int requiredPresses = 3;

    private int pressCount;

    private void OnEnable()
    {
        pressCount = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(unlockKey)) pressCount++;
        if (pressCount == requiredPresses) UnlockAll();
    }

    private void UnlockAll()
    {
        GameProgress progress = GameProgress.Current;
        if (progress != null) progress.UnlockAll();
    }
}
