using System;

public abstract partial class ObjectiveInstance
{
    public class Highscore : ObjectiveInstance
    {
        public float scoreGoal;

        public Highscore() : base() { }

        public override ObjectiveInfo GetInfo()
        {
            ObjectiveInfo info = base.GetInfo();
            info.parameters[0] = scoreGoal.ToString();
            return info;
        }

        public override void SetInfo(ObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out scoreGoal);
        }

        public override void OnPerformanceJudgeScore(float score)
        {
            if (score >= scoreGoal) Complete();
        }
    }
}