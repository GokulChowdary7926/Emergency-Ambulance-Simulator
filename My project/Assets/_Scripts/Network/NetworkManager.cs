using UnityEngine;
using System.Collections.Generic;
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    [Header("Network Settings")]
    public int maxPlayers = 2;
    public int port = 7777;
    public string gameName = "GreenCorridor_Game";
    [Header("Player Prefabs")]
    public GameObject ambulancePlayerPrefab;
    public GameObject controlRoomPlayerPrefab;
    [Header("Spawn Points")]
    public Transform[] ambulanceSpawnPoints;
    public Transform controlRoomSpawn;
    private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();
    [System.Serializable]
    public class PlayerInfo
    {
        public int connectionId;
        public string playerName;
        public PlayerRole role;
        public GameObject playerObject;
        public enum PlayerRole { AmbulanceDriver, TrafficController }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartHost()
    {
        Debug.Log($"Host started on port {port}");
        SpawnPlayer(0, "Host", PlayerInfo.PlayerRole.AmbulanceDriver);
    }
    public void JoinGame(string ipAddress)
    {
        Debug.Log($"Connecting to {ipAddress}:{port}");
    }
    void SpawnPlayer(int connectionId, string playerName, PlayerInfo.PlayerRole role)
    {
        GameObject playerPrefab = role == PlayerInfo.PlayerRole.AmbulanceDriver ?
            ambulancePlayerPrefab : controlRoomPlayerPrefab;
        if (playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab not assigned for role: {role}");
            return;
        }
        Transform spawnPoint = role == PlayerInfo.PlayerRole.AmbulanceDriver ?
            GetAmbulanceSpawnPoint() : controlRoomSpawn;
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        AmbulanceController ambulance = player.GetComponent<AmbulanceController>();
        if (ambulance != null)
        {
        }
        ControlRoomController controlRoom = player.GetComponent<ControlRoomController>();
        if (controlRoom != null)
        {
            controlRoom.playerName = playerName;
        }
        PlayerInfo playerInfo = new PlayerInfo
        {
            connectionId = connectionId,
            playerName = playerName,
            role = role,
            playerObject = player
        };
        connectedPlayers.Add(playerInfo);
        Debug.Log($"Player spawned: {playerName} as {role}");
    }
    Transform GetAmbulanceSpawnPoint()
    {
        if (ambulanceSpawnPoints == null || ambulanceSpawnPoints.Length == 0)
        {
            return transform;
        }
        foreach (Transform spawn in ambulanceSpawnPoints)
        {
            if (spawn == null) continue;
            bool occupied = false;
            foreach (PlayerInfo player in connectedPlayers)
            {
                if (player.playerObject != null &&
                    Vector3.Distance(player.playerObject.transform.position, spawn.position) < 10f)
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied) return spawn;
        }
        return ambulanceSpawnPoints[0];
    }
    public void SyncSignalState(int signalId, string state)
    {
        Debug.Log($"Network: Signal {signalId} synced to {state}");
        TrafficSignal[] signals = FindObjectsOfType<TrafficSignal>();
        if (signalId >= 0 && signalId < signals.Length)
        {
            TrafficSignal signal = signals[signalId];
        }
    }
}
