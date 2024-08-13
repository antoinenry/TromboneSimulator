using UnityEngine;
using UnityEngine.Events;
using System;

public class CounterUtility : MonoBehaviour
{
    [Serializable]
    public class Counter
    {
        [SerializeField] private int counterValue;
        public int minValue = 0;
        public int maxValue = 100;
        [Header("Active movement")]
        public int incrementBufferSize = 1;
        public int decrementBufferSize = 1;
        [Header("Passive movement")]
        public float passiveMovementDelay = 1f;
        public float passiveMovementSpeed = -1f;

        public UnityEvent<int> onValueChange;

        private int incrementBuffer;
        private int decrementBuffer;
        private float passiveTimeBuffer;
        private float passiveMovement;

        public void OnEnable()
        {
            incrementBuffer = 0;
            decrementBuffer = 0;
            passiveTimeBuffer = 0f;
            passiveMovement = 0f;
            SetCounterValue(counterValue);
        }

        public void Update()
        {
            PassiveMovement();
        }

        public int Value
        {
            get => counterValue;
            set => SetCounterValue(value);
        }

        public void Increment()
        {
            passiveTimeBuffer = 0f;
            if (incrementBuffer < incrementBufferSize) incrementBuffer++;
            else SetCounterValue(counterValue + 1);
        }

        public void Decrement()
        {
            passiveTimeBuffer = 0f;
            if (decrementBuffer < decrementBufferSize) decrementBuffer++;
            else SetCounterValue(counterValue - 1);
        }

        private void SetCounterValue(int value)
        {
            value = Mathf.Clamp(value, minValue, maxValue);
            if (value == counterValue) return;
            if (value > counterValue) decrementBuffer = 0;
            else incrementBuffer = 0;
            counterValue = value;
            onValueChange.Invoke(value);
        }

        private void PassiveMovement()
        {
            if (passiveMovementSpeed == 0f) return;
            // Delay
            if (passiveTimeBuffer < passiveMovementDelay)
            {
                passiveMovement = 0f;
                passiveTimeBuffer += Time.deltaTime;
                return;
            }
            // Movement
            passiveMovement += Time.deltaTime * passiveMovementSpeed;
            if (passiveMovement > 1f)
            {
                SetCounterValue(counterValue + 1);
                passiveMovement -= 1f;
            }
            else if (passiveMovement < -1f)
            {
                SetCounterValue(counterValue - 1);
                passiveMovement += 1f;
            }
        }
    }

    public Counter counter;
    public ObjectMethodCaller caller = new("Increment", "Decrement");

    private void OnEnable() => counter?.OnEnable();
    private void Update() => counter?.Update();
    public void Increment() => counter?.Increment();
    public void Decrement() => counter?.Decrement();
}

