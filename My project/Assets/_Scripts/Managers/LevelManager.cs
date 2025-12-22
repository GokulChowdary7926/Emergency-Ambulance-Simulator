using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class LevelData
{
    public string levelName;
    public string cityName;
    public Vector2 mapCoordinates; // Lat/Lon for real city
    public float mapSize = 2000f; // Meters
    public TimeOfDay timeOfDay;
    public WeatherCondition weather;
    public Difficulty difficulty;
    public string[] emergencyTypes;
    public int trafficDensity;
    public float timeLimit;
    [TextArea]
    public string briefing;
    public enum TimeOfDay { Morning, Noon, Evening, Night }
    public enum WeatherCondition { Clear, Rain, Fog, Storm }
    public enum Difficulty { Easy, Medium, Hard, Expert }
}
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [Header("Level Database")]
    public List<LevelData> levels = new List<LevelData>();
    public int currentLevelIndex = 0;
    [Header("Environmental Systems")]
    [Header("Time Settings")]
    public float timeScale = 60f; // 1 real second = 1 game minute
    public float currentTime = 8f * 60f; // 8:00 AM in minutes
    public bool dynamicTime = true;
    [Header("References")]
    public Camera mainCamera;
    public Light sunLight;
    public Light moonLight;
    public ReflectionProbe reflectionProbe;
    private LevelData currentLevel;
    private float levelStartTime;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void InitializeDefaultLevels()
    {
        levels.Add(new LevelData
        {
            levelName = "Training Run",
            cityName = "Training Facility",
            timeOfDay = LevelData.TimeOfDay.Morning,
            weather = LevelData.WeatherCondition.Clear,
            difficulty = LevelData.Difficulty.Easy,
            emergencyTypes = new[] { "Cardiac Arrest" },
            trafficDensity = 20,
            timeLimit = 600f,
            briefing = "Complete your training run. Navigate through light traffic to reach the hospital."
        });
        levels.Add(new LevelData
        {
            levelName = "Rush Hour Emergency",
            cityName = "Downtown",
            timeOfDay = LevelData.TimeOfDay.Evening,
            weather = LevelData.WeatherCondition.Rain,
            difficulty = LevelData.Difficulty.Medium,
            emergencyTypes = new[] { "Traumatic Injury", "Severe Bleeding" },
            trafficDensity = 80,
            timeLimit = 480f,
            briefing = "Navigate through heavy rush hour traffic during a rainstorm. Patient has severe bleeding."
        });
        levels.Add(new LevelData
        {
            levelName = "Night Crisis",
            cityName = "Industrial District",
            timeOfDay = LevelData.TimeOfDay.Night,
            weather = LevelData.WeatherCondition.Fog,
            difficulty = LevelData.Difficulty.Hard,
            emergencyTypes = new[] { "Multi-Vehicle Accident" },
            trafficDensity = 40,
            timeLimit = 420f,
            briefing = "Navigate through foggy industrial streets at night. Multiple patients require transport."
        });
        levels.Add(new LevelData
        {
            levelName = "Monsoon Rescue",
            cityName = "Flood Zone",
            timeOfDay = LevelData.TimeOfDay.Night,
            weather = LevelData.WeatherCondition.Storm,
            difficulty = LevelData.Difficulty.Expert,
            emergencyTypes = new[] { "Flood Rescue", "Cardiac Arrest" },
            trafficDensity = 30,
            timeLimit = 360f,
            briefing = "Heavy monsoon conditions. Roads are flooding. Patient trapped in waterlogged area."
        });
    }
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        currentLevelIndex = levelIndex;
        currentLevel = levels[levelIndex];
        levelStartTime = Time.time;
        ApplyTimeOfDay(currentLevel.timeOfDay);
        ApplyWeather(currentLevel.weather);
        SetTrafficDensity(currentLevel.trafficDensity);
        LoadCityMap(currentLevel.cityName, currentLevel.mapCoordinates, currentLevel.mapSize);
        SetupEmergencyScenario(currentLevel.emergencyTypes);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.goldenHour = currentLevel.timeLimit;
            GameManager.Instance.timeRemaining = currentLevel.timeLimit;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBriefing(currentLevel.briefing);
        }
        Debug.Log($"Level loaded: {currentLevel.levelName}");
    }
    void ApplyTimeOfDay(LevelData.TimeOfDay time)
    {
        switch (time)
        {
            case LevelData.TimeOfDay.Morning:
                currentTime = 8f * 60f; // 8:00 AM
                SetSkybox("Morning");
                break;
            case LevelData.TimeOfDay.Noon:
                currentTime = 12f * 60f; // 12:00 PM
                SetSkybox("Noon");
                break;
            case LevelData.TimeOfDay.Evening:
                currentTime = 18f * 60f; // 6:00 PM
                SetSkybox("Evening");
                break;
            case LevelData.TimeOfDay.Night:
                currentTime = 22f * 60f; // 10:00 PM
                SetSkybox("Night");
                EnableStreetLights(true);
                break;
        }
        UpdateLighting();
    }
    void ApplyWeather(LevelData.WeatherCondition weather)
    {
        switch (weather)
        {
            case LevelData.WeatherCondition.Clear:
                SetFog(false, 0f);
                if (ParticleManager.Instance != null)
                    ParticleManager.Instance.SpawnRain(false, 0f);
                break;
            case LevelData.WeatherCondition.Rain:
                SetFog(false, 0f);
                if (ParticleManager.Instance != null)
                    ParticleManager.Instance.SpawnRain(true, 0.7f);
                AdjustRoadFriction(0.8f); // Slippery roads
                break;
            case LevelData.WeatherCondition.Fog:
                SetFog(true, 0.05f);
                if (ParticleManager.Instance != null)
                    ParticleManager.Instance.SpawnRain(false, 0f);
                break;
            case LevelData.WeatherCondition.Storm:
                SetFog(true, 0.1f);
                if (ParticleManager.Instance != null)
                    ParticleManager.Instance.SpawnRain(true, 1f);
                AdjustRoadFriction(0.6f); // Very slippery
                EnableLightning(true);
                break;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.UpdateWeatherAudio(weather);
        }
    }
    void SetSkybox(string timeOfDay)
    {
        Material skybox = Resources.Load<Material>($"Skyboxes/{timeOfDay}");
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            if (reflectionProbe != null)
            {
                reflectionProbe.RenderProbe();
            }
        }
    }
    void SetFog(bool enable, float density)
    {
        RenderSettings.fog = enable;
        RenderSettings.fogDensity = density;
        if (enable)
        {
            RenderSettings.fogColor = Color.gray;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
        }
    }
    void EnableStreetLights(bool enable)
    {
        Light[] streetLights = FindObjectsOfType<Light>();
        foreach (Light light in streetLights)
        {
            if (light.gameObject.CompareTag("StreetLight"))
            {
                light.enabled = enable;
            }
        }
    }
    void EnableLightning(bool enable)
    {
        if (enable)
        {
            StartCoroutine(LightningStrikes());
        }
    }
    System.Collections.IEnumerator LightningStrikes()
    {
        while (currentLevel != null && currentLevel.weather == LevelData.WeatherCondition.Storm)
        {
            yield return new WaitForSeconds(Random.Range(10f, 30f));
            if (sunLight != null)
            {
                float originalIntensity = sunLight.intensity;
                sunLight.intensity = 5f;
                if (SoundManager.Instance != null && SoundManager.Instance.crashSounds.clips.Length > 0)
                {
                    Vector3 stormPos = GetRandomStormPosition();
                    SoundManager.Instance.PlayCrashSound(1000f, stormPos);
                }
                yield return new WaitForSeconds(0.1f);
                sunLight.intensity = originalIntensity;
                yield return new WaitForSeconds(0.05f);
                sunLight.intensity = 3f;
                yield return new WaitForSeconds(0.05f);
                sunLight.intensity = originalIntensity;
            }
        }
    }
    void AdjustRoadFriction(float multiplier)
    {
        AmbulanceController[] vehicles = FindObjectsOfType<AmbulanceController>();
        foreach (var vehicle in vehicles)
        {
            WheelCollider[] wheels = vehicle.GetComponentsInChildren<WheelCollider>();
            foreach (var wheel in wheels)
            {
                if (wheel != null)
                {
                    WheelFrictionCurve friction = wheel.forwardFriction;
                    friction.stiffness *= multiplier;
                    wheel.forwardFriction = friction;
                    friction = wheel.sidewaysFriction;
                    friction.stiffness *= multiplier;
                    wheel.sidewaysFriction = friction;
                }
            }
        }
    }
    void SetTrafficDensity(int density)
    {
        if (TrafficManager.Instance != null)
        {
            TrafficManager.Instance.maxVehicles = density;
            TrafficManager.Instance.spawnRate = density / 50f;
        }
    }
    void LoadCityMap(string cityName, Vector2 coordinates, float size)
    {
        GenerateProceduralCity(cityName, size);
    }
    void GenerateProceduralCity(string cityName, float size)
    {
        GameObject oldCity = GameObject.Find("City");
        if (oldCity != null) Destroy(oldCity);
        GameObject city = new GameObject("City");
        int gridSize = Mathf.FloorToInt(size / 100f);
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int z = -gridSize; z <= gridSize; z++)
            {
                if (x % 2 == 0 && z % 2 == 0) // Intersections
                {
                    CreateIntersection(new Vector3(x * 50f, 0, z * 50f));
                }
                else if (x % 2 == 0) // North-South road
                {
                    CreateRoadSegment(new Vector3(x * 50f, 0, z * 50f), new Vector3(10f, 0.1f, 50f));
                }
                else if (z % 2 == 0) // East-West road
                {
                    CreateRoadSegment(new Vector3(x * 50f, 0, z * 50f), new Vector3(50f, 0.1f, 10f));
                }
                else // Buildings
                {
                    if (Random.value > 0.7f)
                    {
                        CreateBuilding(new Vector3(x * 50f, 0, z * 50f));
                    }
                }
            }
        }
        GameObject hospital = GameObject.FindWithTag("Hospital");
        if (hospital == null)
        {
            GameObject hospitalObj = Resources.Load<GameObject>("Prefabs/Infrastructure/Hospital");
            if (hospitalObj != null)
            {
                hospital = Instantiate(hospitalObj);
                hospital.transform.position = new Vector3((gridSize - 1) * 50f, 0, (gridSize - 1) * 50f);
            }
        }
        GameObject ambulance = GameObject.FindWithTag("Ambulance");
        if (ambulance != null)
        {
            ambulance.transform.position = new Vector3((-gridSize + 1) * 50f, 0.5f, (-gridSize + 1) * 50f);
        }
    }
    void CreateIntersection(Vector3 position)
    {
        GameObject intersection = new GameObject("Intersection");
        intersection.transform.position = position;
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.transform.parent = intersection.transform;
        road.transform.localScale = new Vector3(50f, 0.1f, 50f);
        Material roadMat = Resources.Load<Material>("Materials/Road");
        if (roadMat != null)
        {
            road.GetComponent<Renderer>().material = roadMat;
        }
        GameObject signalPrefab = Resources.Load<GameObject>("Prefabs/Infrastructure/TrafficSignal");
        if (signalPrefab != null)
        {
            GameObject signal = Instantiate(signalPrefab);
            signal.transform.parent = intersection.transform;
            signal.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    void CreateRoadSegment(Vector3 position, Vector3 size)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.transform.position = position;
        road.transform.localScale = size;
        Material roadMat = Resources.Load<Material>("Materials/Road");
        if (roadMat != null)
        {
            road.GetComponent<Renderer>().material = roadMat;
        }
        road.name = "RoadSegment";
    }
    void CreateBuilding(Vector3 position)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.transform.position = position;
        building.transform.localScale = new Vector3(
            Random.Range(20f, 40f),
            Random.Range(10f, 50f),
            Random.Range(20f, 40f)
        );
        Material buildingMat = Resources.Load<Material>("Materials/Building");
        if (buildingMat != null)
        {
            building.GetComponent<Renderer>().material = buildingMat;
        }
        building.name = "Building";
    }
    void SetupEmergencyScenario(string[] emergencyTypes)
    {
        if (emergencyTypes == null || emergencyTypes.Length == 0) return;
        string selectedEmergency = emergencyTypes[Random.Range(0, emergencyTypes.Length)];
        PatientSystem patientSystem = FindObjectOfType<PatientSystem>();
        if (patientSystem != null)
        {
            patientSystem.emergencyType = selectedEmergency;
            patientSystem.InitializeRandomCondition();
        }
        float baseTime = currentLevel.timeLimit;
        switch (selectedEmergency)
        {
            case "Cardiac Arrest":
                baseTime *= 0.8f; // 20% less time
                break;
            case "Severe Bleeding":
                baseTime *= 0.7f; // 30% less time
                break;
            case "Multi-Vehicle Accident":
                baseTime *= 1.2f; // More time for multiple patients
                break;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.goldenHour = baseTime;
            GameManager.Instance.timeRemaining = baseTime;
        }
    }
    void Update()
    {
        if (dynamicTime)
        {
            currentTime += Time.deltaTime * timeScale / 60f; // Convert to minutes
            if (currentTime >= 24f * 60f)
            {
                currentTime -= 24f * 60f;
            }
            UpdateLighting();
        }
    }
    void UpdateLighting()
    {
        if (sunLight == null) return;
        float timeNormalized = currentTime / (24f * 60f);
        float sunAngle = timeNormalized * 360f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 0, 0);
        float intensityMultiplier = 1f;
        Color lightColor = Color.white;
        if (timeNormalized < 0.25f || timeNormalized > 0.75f) // Night
        {
            intensityMultiplier = 0.1f;
            lightColor = Color.blue * 0.3f + Color.white * 0.1f;
            sunLight.enabled = false;
            if (moonLight != null) moonLight.enabled = true;
        }
        else if (timeNormalized < 0.3f || timeNormalized > 0.7f) // Dawn/Dusk
        {
            intensityMultiplier = 0.5f;
            lightColor = Color.red * 0.5f + Color.yellow * 0.3f + Color.white * 0.2f;
            sunLight.enabled = true;
            if (moonLight != null) moonLight.enabled = false;
        }
        else // Day
        {
            intensityMultiplier = 1f;
            lightColor = Color.white;
            sunLight.enabled = true;
            if (moonLight != null) moonLight.enabled = false;
        }
        sunLight.intensity = intensityMultiplier;
        sunLight.color = lightColor;
        RenderSettings.ambientLight = lightColor * 0.3f;
        RenderSettings.ambientIntensity = intensityMultiplier * 0.5f;
    }
    Vector3 GetRandomStormPosition()
    {
        return new Vector3(
            Random.Range(-500f, 500f),
            Random.Range(100f, 300f),
            Random.Range(-500f, 500f)
        );
    }
    public void CompleteLevel()
    {
        float completionTime = Time.time - levelStartTime;
        float timeBonus = Mathf.Max(0, currentLevel.timeLimit - completionTime) * 10f;
        int levelScore = Mathf.RoundToInt(timeBonus + 1000 * (int)(currentLevel.difficulty + 1));
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(levelScore);
        }
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel < levels.Count)
        {
            PlayerPrefs.SetInt($"Level_{nextLevel}_Unlocked", 1);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelComplete(
                currentLevel.levelName,
                levelScore,
                completionTime
            );
        }
        Debug.Log($"Level {currentLevel.levelName} completed in {completionTime:F1}s");
    }
}
