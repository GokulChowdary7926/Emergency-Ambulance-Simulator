using UnityEngine;
using System.Collections.Generic;
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;
    [Header("Vehicle Effects")]
    public ParticleSystem tireSmokePrefab;
    public ParticleSystem exhaustPrefab;
    public ParticleSystem brakeDustPrefab;
    public ParticleSystem roadSprayPrefab;
    [Header("Emergency Effects")]
    public ParticleSystem sirenLightPrefab;
    public ParticleSystem emergencySparkPrefab;
    public ParticleSystem radioWavePrefab;
    [Header("Environmental Effects")]
    public ParticleSystem rainPrefab;
    public ParticleSystem fogPrefab;
    public ParticleSystem dustStormPrefab;
    [Header("Impact Effects")]
    public ParticleSystem crashDebrisPrefab;
    public ParticleSystem glassShardPrefab;
    public ParticleSystem skidMarkPrefab;
    [Header("Medical Effects")]
    public ParticleSystem bloodDripPrefab;
    public ParticleSystem oxygenMaskPrefab;
    public ParticleSystem cprCompressionPrefab;
    private Dictionary<string, Queue<ParticleSystem>> particlePools = new Dictionary<string, Queue<ParticleSystem>>();
    private Dictionary<ParticleSystem, string> activeParticles = new Dictionary<ParticleSystem, string>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void InitializePools()
    {
        if (tireSmokePrefab != null)
            InitializePool("TireSmoke", tireSmokePrefab, 20);
        if (exhaustPrefab != null)
            InitializePool("Exhaust", exhaustPrefab, 10);
        if (brakeDustPrefab != null)
            InitializePool("BrakeDust", brakeDustPrefab, 15);
        if (sirenLightPrefab != null)
            InitializePool("SirenLight", sirenLightPrefab, 4);
        if (crashDebrisPrefab != null)
            InitializePool("CrashDebris", crashDebrisPrefab, 10);
    }
    void InitializePool(string poolName, ParticleSystem prefab, int count)
    {
        if (!particlePools.ContainsKey(poolName))
        {
            particlePools[poolName] = new Queue<ParticleSystem>();
            for (int i = 0; i < count; i++)
            {
                ParticleSystem ps = Instantiate(prefab, transform);
                ps.gameObject.SetActive(false);
                particlePools[poolName].Enqueue(ps);
            }
        }
    }
    public ParticleSystem SpawnParticle(string poolName, Vector3 position, Quaternion rotation, float duration = -1f)
    {
        if (!particlePools.ContainsKey(poolName) || particlePools[poolName].Count == 0)
        {
            Debug.LogWarning($"No particles available in pool: {poolName}");
            return null;
        }
        ParticleSystem ps = particlePools[poolName].Dequeue();
        ps.transform.position = position;
        ps.transform.rotation = rotation;
        ps.gameObject.SetActive(true);
        ps.Play();
        activeParticles[ps] = poolName;
        if (duration > 0)
        {
            StartCoroutine(ReturnToPoolAfterDelay(ps, duration));
        }
        return ps;
    }
    public void ReturnParticle(ParticleSystem ps)
    {
        if (activeParticles.ContainsKey(ps))
        {
            string poolName = activeParticles[ps];
            ps.Stop();
            ps.gameObject.SetActive(false);
            particlePools[poolName].Enqueue(ps);
            activeParticles.Remove(ps);
        }
    }
    System.Collections.IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnParticle(ps);
    }
    public void SpawnTireSmoke(WheelCollider wheel, float intensity)
    {
        if (intensity > 0.2f && wheel != null)
        {
            Vector3 pos;
            Quaternion rot;
            wheel.GetWorldPose(out pos, out rot);
            ParticleSystem smoke = SpawnParticle("TireSmoke", pos, rot, 2f);
            if (smoke != null)
            {
                var emission = smoke.emission;
                emission.rateOverTime = intensity * 50f;
                var main = smoke.main;
                main.startSpeed = intensity * 5f;
            }
        }
    }
    public void SpawnExhaust(Vector3 position, Vector3 direction, float engineLoad)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        ParticleSystem exhaust = SpawnParticle("Exhaust", position, rotation, 1f);
        if (exhaust != null)
        {
            var main = exhaust.main;
            main.startSize = 0.1f + engineLoad * 0.2f;
            main.startSpeed = 2f + engineLoad * 3f;
        }
    }
    public void SpawnCrashEffect(Vector3 position, Vector3 normal, float force)
    {
        ParticleSystem debris = SpawnParticle("CrashDebris", position, Quaternion.identity, 3f);
        if (debris != null)
        {
            var main = debris.main;
            main.startSpeed = force * 0.1f;
            main.maxParticles = Mathf.FloorToInt(force * 0.01f);
        }
        if (force > 500f && glassShardPrefab != null)
        {
            ParticleSystem glass = Instantiate(glassShardPrefab, position, Quaternion.identity);
            glass.Play();
            Destroy(glass.gameObject, 5f);
        }
        if (skidMarkPrefab != null)
        {
            ParticleSystem skid = Instantiate(skidMarkPrefab, position, Quaternion.FromToRotation(Vector3.up, normal));
            skid.transform.parent = null;
            var skidMain = skid.main;
            skidMain.startLifetime = force * 0.002f;
            skid.Play();
        }
    }
    public void SpawnRain(bool enable, float intensity)
    {
        if (rainPrefab == null) return;
        if (enable && !rainPrefab.isPlaying)
        {
            rainPrefab.gameObject.SetActive(true);
            var emission = rainPrefab.emission;
            emission.rateOverTime = intensity * 1000f;
            rainPrefab.Play();
        }
        else if (!enable && rainPrefab.isPlaying)
        {
            rainPrefab.Stop();
        }
    }
    public void SpawnSirenLights(Vector3 position, bool enable)
    {
        if (enable)
        {
            ParticleSystem siren = SpawnParticle("SirenLight", position, Quaternion.identity, -1f);
            if (siren != null)
            {
                var shape = siren.shape;
                shape.angle = 30f;
                var main = siren.main;
                main.startColor = new ParticleSystem.MinMaxGradient(
                    Color.red, Color.blue
                );
            }
        }
        else
        {
            List<ParticleSystem> toRemove = new List<ParticleSystem>();
            foreach (var kvp in activeParticles)
            {
                if (kvp.Value == "SirenLight")
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var ps in toRemove)
            {
                ReturnParticle(ps);
            }
        }
    }
    public void SpawnMedicalEffect(string effectType, Vector3 position)
    {
        switch (effectType)
        {
            case "blood_drip":
                if (bloodDripPrefab != null)
                {
                    ParticleSystem blood = Instantiate(bloodDripPrefab, position, Quaternion.identity);
                    blood.Play();
                    Destroy(blood.gameObject, 2f);
                }
                break;
            case "oxygen_mask":
                if (oxygenMaskPrefab != null)
                {
                    ParticleSystem oxygen = Instantiate(oxygenMaskPrefab, position, Quaternion.identity);
                    oxygen.Play();
                }
                break;
            case "cpr_compression":
                if (cprCompressionPrefab != null)
                {
                    ParticleSystem cpr = Instantiate(cprCompressionPrefab, position, Quaternion.identity);
                    cpr.Play();
                    Destroy(cpr.gameObject, 1f);
                }
                break;
        }
    }
    void Update()
    {
        List<ParticleSystem> toRemove = new List<ParticleSystem>();
        foreach (var kvp in activeParticles)
        {
            if (kvp.Key == null || !kvp.Key.IsAlive())
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var ps in toRemove)
        {
            ReturnParticle(ps);
        }
    }
}
