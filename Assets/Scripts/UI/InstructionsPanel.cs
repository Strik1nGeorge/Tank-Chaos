using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Panel to display game instructions to new players.
/// </summary>
public class InstructionsPanel : MonoBehaviour
{
    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Text   _instructionsText;

    public void Init(System.Action onClose)
    {
        if (_closeButton == null)
        {
            Transform t = transform.Find("CloseButton");
            if (t != null) _closeButton = t.GetComponent<Button>();
        }

        if (_instructionsText == null)
        {
            Transform t = transform.Find("InstructionsText");
            if (t != null) _instructionsText = t.GetComponent<Text>();
        }

        if (_instructionsText != null)
        {
            _instructionsText.text = 
                "<b>HOW TO PLAY</b>\n\n" +
                "<b>W/A/S/D:</b> Move Tank\n" +
                "<b>Left-Click:</b> Shoot\n" +
                "<b>Right-Click:</b> Switch Weapon (Cannon / Shotgun)\n" +
                "<b>Left-Shift:</b> Sprint / Dash (1.5s Cooldown)\n" +
                "<b>Spacebar:</b> Pause Game\n\n" +
                "<b>OBJECTIVE</b>\n" +
                "Clear all enemies in the sector to advance to the next level.\n\n" +
                "<b>ITEMS (Drops from dead enemies!)</b>\n" +
                "<b>Green:</b> Restore Health\n" +
                "<b>Blue:</b> Double Damage\n" +
                "<b>Cyan:</b> Pierce-Wall Lasers";
        }

        _closeButton?.onClick.RemoveAllListeners();
        _closeButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Closing Instructions Panel");
            onClose?.Invoke();
        });
    }
}
