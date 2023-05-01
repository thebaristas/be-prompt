using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LevelParameters levelParams {get; private set;}

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartLevel(LevelParameters levelParameters) {
        string scene = "SampleScene";
        Debug.Log($"loading scene {scene}");
        levelParams = levelParameters;
        AudioManager.Instance.PlayClip(AudioClipNames.M_Shush1);
        SceneManager.LoadScene(scene);
    }
}
