using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CivilianCarAI : MonoBehaviour
{
    public enum DriverBehavior
    {
        Normal,
        Alerted,
        PullingOver,
        Stopped,
        Panic
    }
    [Header("AI Settings")]
    public DriverBehavior currentBehavior = DriverBehavior.Normal;
    public float normalSpeed = 40f;
    public float currentSpeed = 0f;
    public float awareness = 0.7f; // 0-1, how quickly driver reacts
    [Header("Emergency Response")]
    public float hearingRange = 150f;
    public float sightRange = 100f;
    public float panicThreshold = 0.8f;
    public float pullOverDistance = 30f;
    [Header("Vehicle Physics")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheelMeshes;
    public float maxMotorTorque = 300f;
    public float maxSteeringAngle = 35f;
    [Header("Visual & Audio")]
    public Light[] brakeLights;
    public AudioSource hornAudio;
    public ParticleSystem brakeDust;
    [Header("Path Following")]
    public List<Transform> pathNodes = new List<Transform>();
    public int currentNode = 0;
    public float nodeReachDistance = 5f;
    private Rigidbody rb;
    private float panicLevel = 0f;
    private Vector3 targetPullOverPosition;
    private bool isPullingOver = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, -0.3f, 0);
        }
        if (pathNodes.Count == 0)
        {
            FindRandomPath();
        }
        currentSpeed = normalSpeed;
    }
    void Update()
    {
        CheckEmergencySituation();
        switch (currentBehavior)
        {
            case DriverBehavior.Normal:
                FollowPath();
                break;
            case DriverBehavior.Alerted:
                SlowDownAndCheck();
                break;
            case DriverBehavior.PullingOver:
                PullOverToSide();
                break;
            case DriverBehavior.Stopped:
                StayStopped();
                break;
            case DriverBehavior.Panic:
                PanicBehavior();
                break;
        }
        UpdateLights();
        UpdateWheelVisuals();
    }
    void CheckEmergencySituation()
    {
        if (AmbulanceGPS.Instance == null) return;
        Vector3 ambulancePos = AmbulanceGPS.Instance.GetPosition();
        float distance = Vector3.Distance(transform.position, ambulancePos);
        bool canHear = distance < hearingRange;
        bool canSee = false;
        if (distance < sightRange)
        {
            Vector3 direction = ambulancePos - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction.normalized, out hit, sightRange))
            {
                if (hit.collider.CompareTag("Ambulance"))
                {
                    canSee = true;
                }
            }
        }
        bool emergencyActive = GameManager.Instance != null && GameManager.Instance.isEmergencyActive;
        if ((canHear || canSee) && emergencyActive)
        {
            if (distance < pullOverDistance)
            {
                currentBehavior = DriverBehavior.PullingOver;
                panicLevel = Mathf.Clamp01(panicLevel + Time.deltaTime);
                if (panicLevel > panicThreshold)
                {
                    currentBehavior = DriverBehavior.Panic;
                }
            }
            else if (distance < hearingRange * 0.7f)
            {
                currentBehavior = DriverBehavior.Alerted;
                panicLevel = Mathf.Clamp01(panicLevel + Time.deltaTime * 0.5f);
            }
        }
        else
        {
            if (currentBehavior != DriverBehavior.Normal)
            {
                if (!isPullingOver)
                {
                    currentBehavior = DriverBehavior.Normal;
                    panicLevel = Mathf.Clamp01(panicLevel - Time.deltaTime);
                }
            }
        }
        awareness = Mathf.Clamp01(1.0f - panicLevel * 0.5f);
    }
    void FollowPath()
    {
        if (pathNodes.Count == 0 || currentNode >= pathNodes.Count) return;
        Transform targetNode = pathNodes[currentNode];
        if (targetNode == null) return;
        Vector3 targetDirection = targetNode.position - transform.position;
        targetDirection.y = 0;
        if (targetDirection.magnitude < nodeReachDistance)
        {
            currentNode = (currentNode + 1) % pathNodes.Count;
            targetNode = pathNodes[currentNode];
            if (targetNode != null)
            {
                targetDirection = targetNode.position - transform.position;
            }
        }
        float steering = Vector3.SignedAngle(transform.forward, targetDirection.normalized, Vector3.up);
        steering = Mathf.Clamp(steering, -maxSteeringAngle, maxSteeringAngle);
        if (wheelColliders != null)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                if (wheel != null && wheel.transform.localPosition.z > 0) // Front wheels
                {
                    wheel.steerAngle = steering;
                }
            }
        }
        float speedDifference = normalSpeed - currentSpeed;
        float acceleration = Mathf.Clamp(speedDifference * 0.1f, -10f, 10f);
        if (wheelColliders != null)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                if (wheel != null && wheel.transform.localPosition.z < 0) // Rear wheels
                {
                    wheel.motorTorque = acceleration * maxMotorTorque;
                }
            }
        }
        if (rb != null)
        {
            currentSpeed = rb.velocity.magnitude * 3.6f; // Convert to km/h
        }
    }
    void SlowDownAndCheck()
    {
        float targetSpeed = normalSpeed * 0.5f;
        float speedDifference = targetSpeed - currentSpeed;
        float deceleration = Mathf.Clamp(speedDifference * 0.2f, -20f, 5f);
        if (wheelColliders != null)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                if (wheel != null)
                {
                    wheel.brakeTorque = Mathf.Abs(deceleration) * 100f;
                }
            }
        }
        if (Time.frameCount % 30 == 0) // Every half second at 60fps
        {
            CheckSideMirrors();
        }
        if (Random.value < 0.01f && hornAudio != null && !hornAudio.isPlaying)
        {
            hornAudio.Play();
        }
    }
    void PullOverToSide()
    {
        if (!isPullingOver)
        {
            RaycastHit hit;
            Vector3 rightDirection = transform.right;
            if (Physics.Raycast(transform.position, rightDirection, out hit, 10f))
            {
                if (hit.collider.CompareTag("Road") || hit.collider.CompareTag("Sidewalk"))
                {
                    targetPullOverPosition = hit.point - rightDirection * 2f;
                    isPullingOver = true;
                }
            }
        }
        if (isPullingOver)
        {
            Vector3 direction = targetPullOverPosition - transform.position;
            direction.y = 0;
            if (direction.magnitude > 1f)
            {
                float steering = Vector3.SignedAngle(transform.forward, direction.normalized, Vector3.up);
                steering = Mathf.Clamp(steering, -maxSteeringAngle, maxSteeringAngle);
                if (wheelColliders != null)
                {
                    foreach (WheelCollider wheel in wheelColliders)
                    {
                        if (wheel != null)
                        {
                            if (wheel.transform.localPosition.z > 0)
                            {
                                wheel.steerAngle = steering;
                            }
                            wheel.motorTorque = 50f; // Slow movement
                        }
                    }
                }
            }
            else
            {
                currentBehavior = DriverBehavior.Stopped;
                if (wheelColliders != null)
                {
                    foreach (WheelCollider wheel in wheelColliders)
                    {
                        if (wheel != null)
                        {
                            wheel.brakeTorque = 1000f;
                            wheel.motorTorque = 0f;
                        }
                    }
                }
            }
        }
    }
    void StayStopped()
    {
        if (wheelColliders != null)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                if (wheel != null)
                {
                    wheel.brakeTorque = 1000f;
                    wheel.motorTorque = 0f;
                }
            }
        }
        currentSpeed = 0f;
        if (AmbulanceGPS.Instance != null)
        {
            Vector3 toAmbulance = AmbulanceGPS.Instance.GetPosition() - transform.position;
            float dot = Vector3.Dot(toAmbulance.normalized, transform.forward);
            if (dot < -0.5f) // Ambulance is behind us
            {
                isPullingOver = false;
                currentBehavior = DriverBehavior.Normal;
                StartCoroutine(ResumeDriving());
            }
        }
    }
    IEnumerator ResumeDriving()
    {
        yield return new WaitForSeconds(2f);
        currentBehavior = DriverBehavior.Normal;
    }
    void PanicBehavior()
    {
        float randomSteer = Random.Range(-maxSteeringAngle, maxSteeringAngle);
        if (wheelColliders != null)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                if (wheel != null)
                {
                    if (wheel.transform.localPosition.z > 0)
                    {
                        wheel.steerAngle = randomSteer;
                    }
                    if (Random.value < 0.3f)
                    {
                        wheel.brakeTorque = Random.Range(500f, 1000f);
                    }
                    else
                    {
                        wheel.motorTorque = Random.Range(-100f, 200f);
                    }
                }
            }
        }
        if (hornAudio != null && !hornAudio.isPlaying)
        {
            hornAudio.Play();
        }
        if (Random.value < 0.05f)
        {
            currentBehavior = DriverBehavior.PullingOver;
        }
    }
    void CheckSideMirrors()
    {
        float leftCheck = Random.Range(0f, 1f);
        float rightCheck = Random.Range(0f, 1f);
        if (leftCheck < awareness || rightCheck < awareness)
        {
        }
    }
    void UpdateLights()
    {
        bool brakesOn = currentBehavior == DriverBehavior.Alerted ||
                       currentBehavior == DriverBehavior.PullingOver ||
                       currentBehavior == DriverBehavior.Stopped;
        if (brakeLights != null)
        {
            foreach (Light brakeLight in brakeLights)
            {
                if (brakeLight != null)
                {
                    brakeLight.enabled = brakesOn;
                    brakeLight.intensity = brakesOn ? 3f : 0f;
                }
            }
        }
        if (brakeDust != null)
        {
            var emission = brakeDust.emission;
            emission.rateOverTime = brakesOn && currentSpeed > 5f ? 20f : 0f;
        }
    }
    void UpdateWheelVisuals()
    {
        if (wheelColliders == null || wheelMeshes == null) return;
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if (i < wheelMeshes.Length && wheelMeshes[i] != null && wheelColliders[i] != null)
            {
                Vector3 pos;
                Quaternion rot;
                wheelColliders[i].GetWorldPose(out pos, out rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }
    }
    void FindRandomPath()
    {
        GameObject[] allNodes = GameObject.FindGameObjectsWithTag("PathNode");
        if (allNodes.Length > 3)
        {
            int numNodes = Random.Range(3, Mathf.Min(6, allNodes.Length));
            for (int i = 0; i < numNodes; i++)
            {
                int randomIndex = Random.Range(0, allNodes.Length);
                pathNodes.Add(allNodes[randomIndex].transform);
            }
        }
    }
    public void OnEmergencyAlert(bool emergency)
    {
        if (emergency)
        {
            if (currentBehavior == DriverBehavior.Normal)
            {
                currentBehavior = DriverBehavior.Alerted;
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (ParticleManager.Instance != null && collision.contacts.Length > 0)
        {
            ParticleManager.Instance.SpawnCrashEffect(
                collision.contacts[0].point,
                collision.contacts[0].normal,
                collision.relativeVelocity.magnitude
            );
        }
        if (SoundManager.Instance != null)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            SoundManager.Instance.PlayCrashSound(impactForce, collision.contacts[0].point);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullOverDistance);
        if (pathNodes.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                if (pathNodes[i] != null)
                {
                    Gizmos.DrawSphere(pathNodes[i].position, 1f);
                    if (i < pathNodes.Count - 1 && pathNodes[i+1] != null)
                    {
                        Gizmos.DrawLine(pathNodes[i].position, pathNodes[i+1].position);
                    }
                }
            }
        }
    }
}
