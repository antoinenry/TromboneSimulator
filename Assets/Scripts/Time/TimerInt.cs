using UnityEngine;
using UnityEngine.Events;


public class TimerInt : MonoBehaviour
{
    public bool move = true;
    public float time = 0f;
    public float startTime = 0f;
    public float endTime = 10f;
    public float speed = 1f;

    public UnityEvent onStart;
    public UnityEvent onEnd;
    public UnityEvent<int> onIntValue;

    private void Update()
    {
        if (move) MoveTimer(speed * Time.deltaTime);
    }

    public bool TimesUp()
    {
        return ((speed > 0f && time >= endTime) || (speed < 0f && time <= endTime));
    }

    public void MoveTimer(float deltaTime)
    {
        int intTime = Mathf.CeilToInt(time);
        time += deltaTime;
        int newIntTime = Mathf.CeilToInt(time);
        if (newIntTime != intTime) onIntValue.Invoke(newIntTime);
        if (TimesUp())
        {
            time = endTime;
            onEnd.Invoke();
            enabled = false;
        }
    }
}
