using System;

public abstract partial class Objective
{
    public class Highscore : Objective
    {
        public float scoreGoal;

        public override SerializableObjectiveInfo GetInfo()
        {
            SerializableObjectiveInfo info = base.GetInfo();
            info.parameters[0] = scoreGoal.ToString();
            return info;
        }

        public override void SetInfo(SerializableObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out scoreGoal);
        }

        public override void OnPerformanceJudgeScore(float score)
        {
            if (score >= scoreGoal) onComplete.Invoke();
        }
    }
}