using System;

public abstract partial class ObjectiveInstance
{
    public class Dance : ObjectiveInstance
    {
        public float startTime;
        public float duration;
        public int danceLevel;

        public Dance() : base() { }

        public override ObjectiveInfo GetInfo()
        {
            ObjectiveInfo info = base.GetInfo();
            info.parameters[0] = startTime.ToString();
            info.parameters[1] = duration.ToString();
            info.parameters[2] = danceLevel.ToString();
            return info;
        }

        public override void SetInfo(ObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out startTime);
            float.TryParse(value.parameters[1], out duration);
            int.TryParse(value.parameters[2], out danceLevel);
        }
    }
}