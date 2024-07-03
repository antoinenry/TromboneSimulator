public abstract partial class ObjectiveInstance
{
    public class EndMusic : ObjectiveInstance
    {
        public EndMusic() : base() { }
        public override void OnMusicEnd() => Complete();
    }
}