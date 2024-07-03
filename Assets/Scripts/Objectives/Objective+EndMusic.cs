using System;

public abstract partial class Objective
{
    public class EndMusic : Objective
    {
        public override void OnMusicEnd()
        {
            onComplete.Invoke();
        }
    }
}