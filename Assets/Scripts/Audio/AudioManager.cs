using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager instance {get; private set;}
    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    #endregion
    [SerializeField] private Sound[] sounds;

    private void Play(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);

        if(s == null) return;

        s.source.Play();
    }

    private void Stop(string name)
    { 
        Sound s = System.Array.Find(sounds, sound => sound.name == name);

        if(s == null) return;

        s.source.Stop();
    }


    void Start() 
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.audioMixer;
        }

        Play("BGM");
    } 

    public void StopAllSFX()
    {
        foreach(Sound s in sounds) 
        {
            if(s.name != "BGM") s.source.Stop();
        }
    }

    public void PlaySFX(string name) => Play(name);
    public void PlayButtonHoverSFX() => Play("ButtonHover");
    public void PlayButtonClickSFX() => Play("ButtonClick");
    public void PlayWinSFX() => Play("Win");
    public void PlayBlockSnapSFX() => Play("BlockSnap");
} 

