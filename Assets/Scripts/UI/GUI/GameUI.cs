using UnityEngine;

[ExecuteAlways]
public abstract class GameUI : MonoBehaviour
{
    public abstract bool GUIActive { get; set; }
}