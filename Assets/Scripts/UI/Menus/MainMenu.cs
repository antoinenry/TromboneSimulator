using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteAlways]
public class MainMenu : MenuUI
{
    protected override void Awake()
    {
        base.Awake();
        UIMainMenu = this;
    }
}
