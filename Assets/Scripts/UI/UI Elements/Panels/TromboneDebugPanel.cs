using UnityEngine;
using TMPro;

public class TromboneDebugPanel : MonoBehaviour
{
    public TromboneCore trombone;
    public TromboneDisplay tromboneDisplay;
    public TextMeshProUGUI textRenderer;
    public KeyCode exitKey = KeyCode.Escape;

    private void Update()
    {
        if (Input.GetKey(exitKey)) Application.Quit();
        string infoText = "Plop \n";
        infoText += "Resolution: " + Camera.main.pixelWidth + "x" + Camera.main.pixelHeight + " (px)\n";
        infoText += "Zero position: " + Camera.main.WorldToScreenPoint(tromboneDisplay.GrabPositionOrigin) + " (px)\n";
        infoText += "Slide scale: " + tromboneDisplay.toneWidth * Camera.main.pixelWidth / (Camera.main.aspect * 2f * Camera.main.orthographicSize) +" (px/halfTone)\n";
        infoText += "Pressure scale: " + tromboneDisplay.stepHeight * Camera.main.pixelHeight / (2f * Camera.main.orthographicSize) + " (px/harmonic)\n";
        infoText += "Mouse position: " + Input.mousePosition + " (px)\n";
        infoText += "Slide: " + trombone.slideTone + " (half tones)\n";
        infoText += "Pressure: " + trombone.pressureLevel + "(harmonic) \n";
        infoText += "Note: " + ToneAttribute.GetNoteName(trombone.Tone) + "\n";
        infoText += "Quit: press " + exitKey;
        textRenderer.text = infoText;
    }
}
