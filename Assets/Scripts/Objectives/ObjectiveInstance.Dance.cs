public abstract partial class ObjectiveInstance
{
    public class Dance : LevelEventObjective<LevelDanceEventInstance>
    {
        public float minDanceLevel;
        public int minDanceCount;

        public Dance() : base() { }

        public override ObjectiveInfo GetInfo()
        {
            ObjectiveInfo info = base.GetInfo();
            info.parameters[0] = minDanceLevel.ToString();
            info.parameters[1] = minDanceCount.ToString();
            return info;
        }

        public override void SetInfo(ObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out minDanceLevel);
            int.TryParse(value.parameters[1], out minDanceCount);
        }

        protected override float RequiredEventCompletion => minDanceLevel;
        protected override int RequiredEventCount => minDanceCount;
    }
}