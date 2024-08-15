using System.Collections.Generic;

public abstract partial class ObjectiveInstance
{
    public abstract class LevelEventObjective<T> : ObjectiveInstance where T : LevelEventInstance
    {
        protected List<T> completedEvents;
        protected int CompletedEventCount => completedEvents != null ? completedEvents.Count : 0;
        protected abstract float RequiredEventCompletion { get; }
        protected abstract int RequiredEventCount{ get; }

        public LevelEventObjective() : base()
        {
            completedEvents = new List<T>();
        }

        public override void OnLevelEventCompletion(LevelEventInstance eventInstance, float completion)
        {
            if (completedEvents == null) completedEvents = new List<T>();
            if (eventInstance == null || eventInstance is T == false) return;
            T e = (T)eventInstance;
            if (completedEvents.Contains(e)) return;
            if (completion >= RequiredEventCompletion)
            {
                completedEvents.Add(e);
                if (completedEvents.Count >= RequiredEventCount) Complete();
            }
        }

        public void ResetCounter() => completedEvents = new();
    }
}