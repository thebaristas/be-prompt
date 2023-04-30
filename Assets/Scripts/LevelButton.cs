using UnityEngine;
using TMPro;

public class LevelButton : MonoBehaviour
{
    private LevelParameters levelParameters;

    public void SetLevelParameters(LevelParameters level)
    {
        levelParameters = level;
        // Update the button text to show the level name or number
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text) {
            text.text = levelParameters.name;
        }
    }

    public void OnClick()
    {
        // Start the selected level
        FindObjectOfType<LevelManager>().StartLevel(levelParameters);
    }
}
