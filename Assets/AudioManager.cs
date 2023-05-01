using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public struct AudioSnapshotsNames {
    public const string Menu = "Menu";
    public const string A1 = "A1";
    public const string A2 = "A2";
    public const string B1 = "B1";
    public const string B2 = "B2";
    public const string C1 = "C1";
    public const string Silence = "Silence";
}

public struct AudioClipNames {
    public const string M_A1 = "M_A1";
    public const string M_A2 = "M_A2";
    public const string M_B1 = "M_B1";
    public const string M_B2 = "M_B2";
    public const string M_C1 = "M_C1";
    public const string M_Crowd = "M_Crowd Chatting";
    public const string M_Aaah = "M_Aaah";
    public const string M_Applause = "M_Applause";
    public const string M_Shush1 = "M_Shush1";
    public const string M_Thump = "M_Thump";
    public const string M_boooh = "M_boooh";
    public const string M_click = "M_click mouse";
    public const string M_ohdisappointed = "M_oh disappointed";
    public const string M_swoosh = "M_swoosh";
}

public struct SnapshotDuration {
    public string name;
    public float duration;
    public float timeToReach;

    public SnapshotDuration(string name, float duration, float timeToReach)
    {
        this.name = name;
        this.duration = duration;
        this.timeToReach = timeToReach;
    }
}

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioMixer mixer;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;

    private static AudioManager instance;
    private Dictionary<string, AudioSource> AudioSources;

    public static AudioManager Instance
    {
        get
        {
            // If the instance hasn't been set yet, find it in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();

                // If there are no AudioManager instances in the scene, log an error message
                if (instance == null)
                {
                    Debug.LogError("No AudioManager instance found in the scene!");
                }
            }

            return instance;
        }
    }

    void Awake()
    {
        // If there is already an instance of the class, destroy the new one
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Otherwise, set the instance to this object
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioSources = new Dictionary<string, AudioSource>();
        GetAllAudioClips(transform);
    }

    public void TransitionSnapshots(string name, float timeToReach) {
        AudioMixerSnapshot[] snaps = { mixer.FindSnapshot(name) };
        if (snaps[0] != null) {
            float[] weights = {1};
            mixer.TransitionToSnapshots(snaps, weights, timeToReach);
        }
    }

    public AudioSource PlayClip(string name) {
        AudioSource source = AudioSources[name];
        if (source) {
            // Play the sound effect clip
            source.PlayOneShot(source.clip);
        }
        return source;
    }

    public void ProgramSnapshotDurations(SnapshotDuration[] snapshotDurations)
    {
        float duration = 0f;
        foreach (SnapshotDuration snapshotDuration in snapshotDurations) {
            if (duration >= float.Epsilon) {
                StartCoroutine(PlaySnapshotAfterDelay(snapshotDuration.name, duration, snapshotDuration.timeToReach));
            } else {
                TransitionSnapshots(snapshotDuration.name, snapshotDuration.timeToReach);
            }
            duration = snapshotDuration.duration;
        }
    }

    private IEnumerator PlaySnapshotAfterDelay(string nextSnapshotName, float delay, float timeToReach)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log(nextSnapshotName);
        TransitionSnapshots(nextSnapshotName, timeToReach);
    }

    public IEnumerator PlayClipAfterDelay(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayClip(name);
    }

    void GetAllAudioClips(Transform parentTransform)
    {
        foreach (Transform childTransform in parentTransform)
        {
            AudioSource audioSource = childTransform.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                if (!AudioSources.ContainsKey(audioSource.clip.name))
                {
                    AudioSources.Add(audioSource.clip.name, audioSource);
                }
            }
            GetAllAudioClips(childTransform);
        }
    }

}
