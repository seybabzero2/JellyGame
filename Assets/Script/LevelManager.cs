using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ObstacleVariant
{
    public GameObject prefab;
    public float baseWidth;
    public float baseHeight;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Direction Settings")]
public Vector3 currentDirection = Vector3.forward; 

    [Header("Global Speed Settings")]
    public float currentSpeed = 15f;
    public float speedIncreaseRate = 0.5f;
    public float speedIncreaseInterval = 5f;

    [Header("Spawner Settings")]
    public float spawnZPosition = 50f;
    public float timeBetweenSpawns = 2f;

    [Header("Obstacle Data")]
    public ObstacleVariant[] variants; 

    private Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();
    
    private float spawnTimer;
    private float speedTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < variants.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
        }
    }

    private void Update()
    {
        speedTimer += Time.deltaTime;
        if (speedTimer >= speedIncreaseInterval)
        {
            currentSpeed += speedIncreaseRate;
            speedTimer = 0f;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= timeBetweenSpawns)
        {
            SpawnObstacle();
            spawnTimer = 0f;
        }
    }

    private void SpawnObstacle()
    {
        int randomType = Random.Range(0, variants.Length);
        float randomScale = Random.Range(0.8f, 1.2f);
        GameObject obs;

        if (pools[randomType].Count > 0)
        {
            obs = pools[randomType].Dequeue();
        }
        else
        {
            obs = Instantiate(variants[randomType].prefab);
            obs.GetComponent<BaseObstacle>().poolIndex = randomType; 
        }

        obs.transform.position = currentDirection * spawnZPosition;
        obs.transform.rotation = Quaternion.LookRotation(currentDirection);

        ShapeObstacle shapeComp = obs.GetComponent<ShapeObstacle>();
    
        if (shapeComp != null)
        {
            shapeComp.holeWidth = variants[randomType].baseWidth * randomScale;
            shapeComp.holeHeight = variants[randomType].baseHeight * randomScale;
        }

        obs.SetActive(true);
    }

    public void ReturnToPool(GameObject obs, int typeIndex)
    {
        obs.SetActive(false);
        pools[typeIndex].Enqueue(obs);
    }

    public Transform playerRig; 

private void ClearOldObstacles(TurnObstacle exceptionTurn)
{
    BaseObstacle[] activeObstacles = FindObjectsByType<BaseObstacle>(FindObjectsSortMode.None);

    foreach (BaseObstacle obs in activeObstacles)
    {
        if (obs == exceptionTurn)
        {
            continue;
        }

        ReturnToPool(obs.gameObject, obs.poolIndex);
    }
}

   public void ExecuteWorldTurn(bool isLeftTurn, TurnObstacle currentTurn)
    {
        float angle = isLeftTurn ? -90f : 90f;
        currentDirection = Quaternion.Euler(0, angle, 0) * currentDirection;

        ClearOldObstacles(currentTurn);

        Camera.main.GetComponent<CameraFollow>()?.AddShakeImpulse(1.0f, 0.3f);
        
        StartCoroutine(RotateRig(angle));
    }

    private System.Collections.IEnumerator RotateRig(float angle)
    {
        Quaternion startRot = playerRig.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);
        float time = 0;
        float duration = 0.2f; 

        while (time < duration)
        {
            time += Time.deltaTime;
            playerRig.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
            yield return null;
        }
        playerRig.rotation = endRot;
    }
}