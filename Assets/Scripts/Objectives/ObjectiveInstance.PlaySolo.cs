public abstract partial class ObjectiveInstance
{
    public class PlaySolo : LevelEventObjective<LevelSoloEventInstance>
    {
        public float minSoloLevel;
        public int minSoloCount;

        public PlaySolo() : base() { }

        public override ObjectiveInfo GetInfo()
        {
            ObjectiveInfo info = base.GetInfo();
            info.parameters[0] = minSoloLevel.ToString();
            info.parameters[1] = minSoloCount.ToString();
            return info;
        }

        public override void SetInfo(ObjectiveInfo value)
        {
            base.SetInfo(value);
            float.TryParse(value.parameters[0], out minSoloLevel);
            int.TryParse(value.parameters[1], out minSoloCount);
        }

        protected override float RequiredEventCompletion => minSoloLevel;
        protected override int RequiredEventCount => minSoloCount;

    }
}