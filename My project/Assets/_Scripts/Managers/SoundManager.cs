using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class SoundCategory
{
    public string name;
    public AudioClip[] clips;
    public float volume = 1f;
    public float pitchRange = 0.1f;
    public float spatialBlend = 1f; // 3D effect
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("Ambient Sounds")]
    public AudioSource cityAmbience;
    public AudioSource trafficAmbience;
    public AudioSource rainAmbience;
    [Header("Vehicle Sounds")]
    public SoundCategory engineSounds;
    public SoundCategory tireScreech;
    public SoundCategory hornSounds;
    public SoundCategory crashSounds;
    [Header("Emergency Sounds")]
    public SoundCategory sirenVariants;
    public SoundCategory radioComms;
    public SoundCategory signalChange;
    [Header("Medical Sounds")]
    public SoundCategory heartbeat;
    public SoundCategory medicalEquipment;
    public SoundCategory vitalAlarms;
    [Header("UI Sounds")]
    public SoundCategory buttonClicks;
    public SoundCategory notifications;
    public SoundCategory countdownBeeps;
    [Header("Audio Mixing")]
    public AudioMixer gameMixer;
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float dialogueVolume = 1f;
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    private GameObject ambulance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        ambulance = GameObject.FindGameObjectWithTag("Ambulance");
    }
    void InitializeAudioSources()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject sourceObj = new GameObject($"AudioSource_{i}");
            sourceObj.transform.parent = transform;
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSources.Add(sourceObj.name, source);
        }
    }
    public void PlaySiren(bool start)
    {
        if (start)
        {
            if (ambulance != null)
            {
                Play3DSound(sirenVariants, ambulance.transform.position, 100f, true);
                StartCoroutine(SirenDopplerEffect());
            }
        }
        else
        {
            StopSound("Siren");
        }
    }
    IEnumerator SirenDopplerEffect()
    {
        AudioSource sirenSource = GetAvailableAudioSource();
        if (sirenSource == null || sirenVariants.clips.Length == 0) yield break;
        sirenSource.clip = sirenVariants.clips[0];
        sirenSource.loop = true;
        sirenSource.spatialBlend = 1f;
        sirenSource.rolloffMode = AudioRolloffMode.Logarithmic;
        sirenSource.minDistance = 10f;
        sirenSource.maxDistance = 500f;
        sirenSource.Play();
        while (sirenSource.isPlaying && ambulance != null)
        {
            Rigidbody rb = ambulance.GetComponent<Rigidbody>();
            if (rb != null && Camera.main != null)
            {
                Vector3 relativeVelocity = rb.velocity;
                Vector3 toCamera = (Camera.main.transform.position - ambulance.transform.position).normalized;
                float dopplerFactor = 1f + Vector3.Dot(relativeVelocity, toCamera) / 343f;
                sirenSource.pitch = Mathf.Clamp(dopplerFactor, 0.5f, 2f);
            }
            yield return null;
        }
    }
    public void PlayEngineSound(float rpm, float acceleration)
    {
        AudioSource engineSource = GetOrCreateAudioSource("Engine");
        if (engineSource != null && engineSounds.clips.Length > 0)
        {
            float targetPitch = 0.7f + (rpm / 6000f) * 1.3f;
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);
            float loadVolume = 0.3f + acceleration * 0.7f;
            engineSource.volume = Mathf.Lerp(engineSource.volume, loadVolume, Time.deltaTime * 3f);
            if (!engineSource.isPlaying)
            {
                engineSource.clip = engineSounds.clips[0];
                engineSource.loop = true;
                engineSource.Play();
            }
        }
    }
    public void PlayTireScreech(float intensity, Vector3 position)
    {
        if (intensity > 0.3f)
        {
            Play3DSound(tireScreech, position, 50f, false);
        }
    }
    public void PlayCrashSound(float impactForce, Vector3 position)
    {
        if (crashSounds.clips.Length > 0)
        {
            int clipIndex = Mathf.Clamp(Mathf.FloorToInt(impactForce / 500f), 0, crashSounds.clips.Length - 1);
            Play3DSound(crashSounds, position, 100f, false, clipIndex, impactForce / 2000f);
        }
    }
    public void PlayRadioMessage(string messageType)
    {
        Play2DSound(radioComms, 0, 1f);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSubtitle(GetRadioMessage(messageType), 3f);
        }
    }
    public void PlayHeartbeat(float heartRate)
    {
        AudioSource heartbeatSource = GetOrCreateAudioSource("Heartbeat");
        if (heartbeatSource != null && heartbeat.clips.Length > 0)
        {
            float interval = 60f / heartRate;
            if (!heartbeatSource.isPlaying)
            {
                heartbeatSource.clip = heartbeat.clips[0];
                heartbeatSource.loop = false;
                StartCoroutine(HeartbeatLoop(heartbeatSource, interval));
            }
        }
    }
    IEnumerator HeartbeatLoop(AudioSource source, float interval)
    {
        while (source != null)
        {
            source.Play();
            yield return new WaitForSeconds(interval);
            PatientSystem patient = FindObjectOfType<PatientSystem>();
            if (patient != null && patient.currentPatient != null)
            {
                interval = 60f / patient.currentPatient.heartRate;
            }
        }
    }
    public void UpdateWeatherAudio(LevelData.WeatherCondition weather)
    {
        switch (weather)
        {
            case LevelData.WeatherCondition.Rain:
                if (rainAmbience != null && !rainAmbience.isPlaying)
                {
                    rainAmbience.Play();
                }
                break;
            case LevelData.WeatherCondition.Storm:
                if (rainAmbience != null)
                {
                    rainAmbience.volume = 1f;
                    rainAmbience.Play();
                }
                break;
            default:
                if (rainAmbience != null)
                {
                    rainAmbience.Stop();
                }
                break;
        }
    }
    AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSources.Values)
        {
            if (source != null && !source.isPlaying) return source;
        }
        return null;
    }
    AudioSource GetOrCreateAudioSource(string name)
    {
        if (audioSources.ContainsKey(name)) return audioSources[name];
        GameObject newSource = new GameObject(name);
        newSource.transform.parent = transform;
        AudioSource source = newSource.AddComponent<AudioSource>();
        audioSources[name] = source;
        return source;
    }
    void StopSound(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].Stop();
        }
    }
    void Play3DSound(SoundCategory category, Vector3 position, float maxDistance, bool loop = false, int clipIndex = -1, float volumeMultiplier = 1f)
    {
        AudioSource source = GetAvailableAudioSource();
        if (source == null || category.clips.Length == 0) return;
        source.transform.position = position;
        source.clip = clipIndex >= 0 ? category.clips[clipIndex] :
                     category.clips[Random.Range(0, category.clips.Length)];
        source.volume = category.volume * volumeMultiplier * sfxVolume;
        source.pitch = 1f + Random.Range(-category.pitchRange, category.pitchRange);
        source.spatialBlend = category.spatialBlend;
        source.maxDistance = maxDistance;
        source.loop = loop;
        source.Play();
    }
    void Play2DSound(SoundCategory category, int clipIndex = -1, float volumeMultiplier = 1f)
    {
        AudioSource source = GetAvailableAudioSource();
        if (source == null || category.clips.Length == 0) return;
        source.spatialBlend = 0f; // 2D sound
        source.clip = clipIndex >= 0 ? category.clips[clipIndex] :
                     category.clips[Random.Range(0, category.clips.Length)];
        source.volume = category.volume * volumeMultiplier * sfxVolume;
        source.Play();
    }
    string GetRadioMessage(string type)
    {
        Dictionary<string, string[]> messages = new Dictionary<string, string[]>
        {
            { "dispatch", new[] {
                "Ambulance 7, respond to cardiac emergency.",
                "Priority 1 call, proceed with lights and sirens.",
                "Patient critical, time is essential."
            }},
            { "update", new[] {
                "ETA to hospital?",
                "Patient condition update?",
                "Traffic conditions?"
            }},
            { "arrival", new[] {
                "Hospital notified of your arrival.",
                "Medical team standing by.",
                "Proceed to emergency bay."
            }}
        };
        if (messages.ContainsKey(type))
        {
            string[] options = messages[type];
            return options[Random.Range(0, options.Length)];
        }
        return "Copy that.";
    }
}
