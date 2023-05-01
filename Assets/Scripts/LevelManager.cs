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
        if (levelParameters.difficulty < 1 ) {
            SnapshotDuration[] snapshotDurations = { new SnapshotDuration(AudioSnapshotsNames.Silence, 1f, 3f), new SnapshotDuration(AudioSnapshotsNames.A1, 60, 2f) };
            AudioManager.Instance.ProgramSnapshotDurations(snapshotDurations);
        } else if (levelParameters.difficulty < 3 ) {
            SnapshotDuration[] snapshotDurations = { new SnapshotDuration(AudioSnapshotsNames.Silence, 1f, 3f), new SnapshotDuration(AudioSnapshotsNames.B1, 60, 2f) };
            AudioManager.Instance.ProgramSnapshotDurations(snapshotDurations);
        } else {
            SnapshotDuration[] snapshotDurations = { new SnapshotDuration(AudioSnapshotsNames.Silence, 1f, 3f), new SnapshotDuration(AudioSnapshotsNames.C1, 60, 2f) };
            AudioManager.Instance.ProgramSnapshotDurations(snapshotDurations);
        }
        AudioManager.Instance.PlayClip(AudioClipNames.M_Thump);
        StartCoroutine(AudioManager.Instance.PlayClipAfterDelay(AudioClipNames.M_Shush1, 1.2f));
        SceneManager.LoadScene(scene);
    }
}
