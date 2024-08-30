using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Comments", menuName = "Trombone Hero/Level Events/Comments")]
public class CommentSheet : ScriptableObject
{
    public enum CommentType
    {
        CorrectNote, Health, MissNote, Combo, Score
    }

    [Serializable]
    public struct ValueComment
    {
        public string name;
        public CommentType commentType;
        public float value;
        public float delta;
        public string[] comments;

        public string GetText()
        {
            int textCount = comments != null ? comments.Length : 0;
            if (textCount == 0) return string.Empty;
            return comments[UnityEngine.Random.Range(0, textCount - 1)];
        }
    }

    public ValueComment[] comments;

    public string GetComment(CommentType comment, float value, float delta)
    {
        if (comments == null) return null;
        ValueComment[] sameTypeComments = Array.FindAll(comments, c => c.commentType == comment);
        if (sameTypeComments == null || sameTypeComments.Length == 0) return null;
        if (delta > 0)
        {
            return Array.Find(sameTypeComments, c => delta >= c.delta && value >= c.value && value - delta < c.value).GetText();
        }
        else
        {
            return Array.Find(sameTypeComments, c => delta <= c.delta && value <= c.value && value - delta > c.value).GetText();
        }
    }
}