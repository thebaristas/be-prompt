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

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        AudioSources = new Dictionary<string, AudioSource>();
        GetAllAudioClips(transform);
    }

    public void TransitionSnapshots(string name, float time) {
        AudioMixerSnapshot[] snaps = { mixer.FindSnapshot(name) };
        if (snaps[0] != null) {
            float[] weights = {1};
            mixer.TransitionToSnapshots(snaps, weights, time);
        }
    }

    public AudioSource PlayClip(string name) {
        Debug.LogWarning(name);
        AudioSource source = AudioSources[name];
        Debug.LogWarning(source);
        if (source) {
            // Play the sound effect clip
            source.PlayOneShot(source.clip);
        }
        return source;
    }

    public void PlayCrowdAndRestartAll(string nextSnapshotName)
    {
        AudioSource source = PlayClip(AudioClipNames.M_Crowd);
        // Wait for the sound effect to finish playing
        float soundEffectDuration = source.clip.length;

        TransitionSnapshots(AudioSnapshotsNames.Silence, soundEffectDuration / 2);
        StartCoroutine(WaitForSecondsAndRestartAll(soundEffectDuration, nextSnapshotName));
    }

    private IEnumerator WaitForSecondsAndRestartAll(float duration, string nextSnapshotName)
    {
        yield return new WaitForSeconds(duration / 2);
        // Increase the volume of the specified mixer group over fadeInDuration seconds
        TransitionSnapshots(nextSnapshotName, duration / 2);
        yield return new WaitForSeconds(duration / 2);
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
                    Debug.LogWarning(audioSource.clip.name);
                    AudioSources.Add(audioSource.clip.name, audioSource);
                }
            }
            GetAllAudioClips(childTransform);
        }
    }

}
