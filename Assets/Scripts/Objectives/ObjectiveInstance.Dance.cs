using System.Collections.Generic;

public abstract partial class ObjectiveInstance
{
    public class Dance : ObjectiveInstance
    {
        public float minDanceLevel;
        public int mindanceCount;

        private List<LevelEventInstance> completedEvents;

        public Dance() : base()
        { 
            completedEvents = new List<LevelEventInstance>();
        }

        public override ObjectiveInfo GetInfo()
        {
            ObjectiveInfo info = base.GetInfo();
            info.parameters[0] = minDanceLevel.ToString();
            info.parameters[1] = mindanceCount.ToString();
            return info;
        }

        public override void SetInfo(ObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out minDanceLevel);
            int.TryParse(value.parameters[1], out mindanceCount);
        }

        public override void OnLevelEventCompletion(LevelEventInstance eventInstance, float completion)
        {
            if (completedEvents == null) completedEvents = new List<LevelEventInstance>();
            if (eventInstance == null || eventInstance is LevelDanceEventInstance == false || completedEvents.Contains(eventInstance)) return;
            if (completion >= minDanceLevel)
            {
                completedEvents.Add(eventInstance);
                if (completedEvents.Count >= mindanceCount) Complete();
            }
        }

        public void ResetCounter() => completedEvents = new();
    }
}