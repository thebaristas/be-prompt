using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public GameObject creditsPanel;
    public GameObject levelSelectPanel;
    public Transform levelButtonContainer;
    public GameObject levelButtonPrefab;
    public ScrollRect scrollRect;

    private List<LevelParameters> levelParameters;

    private void Start()
    {
        // Load all LevelData scriptable objects in the Resources folder
        levelParameters = new List<LevelParameters>(Resources.LoadAll<LevelParameters>(""));
        levelParameters.Sort((LevelParameters lp1, LevelParameters lp2) => {
            return lp1.difficulty.CompareTo(lp2.difficulty);
        });

        Debug.Log(levelParameters.ToArray().Length);
        // Create a button for each LevelData object and add it to the level select panel
        foreach (LevelParameters levelParams in levelParameters)
        {
            Debug.Log(levelParams.name);
            GameObject levelButtonObject = Instantiate(levelButtonPrefab, levelButtonContainer);
            LevelButton levelButton = levelButtonObject.GetComponent<LevelButton>();
            levelButton.SetLevelParameters(levelParams);
        }
        scrollRect.content = levelButtonContainer.GetComponent<RectTransform>();
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
        Debug.LogWarning(AudioClipNames.M_click);
        AudioManager.Instance.PlayClip(AudioClipNames.M_click);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
        AudioManager.Instance.PlayClip(AudioClipNames.M_click);
    }

    public void ShowLevelSelect()
    {
        levelSelectPanel.SetActive(true);
        AudioManager.Instance.PlayClip(AudioClipNames.M_click);
        scrollRect.verticalNormalizedPosition = 1; // Scroll to top of level select panel
    }

    public void HideLevelSelect()
    {
        levelSelectPanel.SetActive(false);
        AudioManager.Instance.PlayClip(AudioClipNames.M_click);
    }
}
