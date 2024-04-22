using System;

[Serializable]
public struct MusicPartIdentifier 
{ 
    public string partName; 
    public bool mainVoice;
    public int[] alternativeVoiceIndices;

    public MusicPartIdentifier(string partName)
    {
        this.partName = partName;
        this.mainVoice = true;
        this.alternativeVoiceIndices = new int[1] { 0 };
    }

    public MusicPartIdentifier(string partName, bool mainVoice, params int[] alternativeVoiceIndices)
    {
        this.partName = partName;
        this.mainVoice = mainVoice;
        this.alternativeVoiceIndices = alternativeVoiceIndices;
    }
}