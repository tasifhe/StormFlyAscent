using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Forever;
using Dreamteck.Splines;

/// <summary>
/// Simple obstacle spawner that hooks into LevelGenerator events.
/// Add this to any GameObject in the scene (e.g., ObstacleManager or Level Generator).
/// Does NOT require being on a Level Segment prefab.
/// </summary>
public class ObstacleSpawnerSimple : MonoBehaviour
{
    [Header("Master Control")]
    [Tooltip("Enable/disable automatic obstacle spawning. Disable this if manually placing obstacles in level segments.")]
    public bool enableAutomaticSpawning = false;
    
    [System.Serializable]
    public class ObstacleConfig
    {
        [Tooltip("Obstacle prefab to spawn")]
        public GameObject prefab;
        
        [Tooltip("Spawn weight (higher = more common)")]
        [Range(0f, 100f)]
        public float spawnWeight = 50f;
        
        [Tooltip("Possible movement types for this obstacle")]
        public ObstacleMovement.MovementType[] possibleMovements = 
        {
            ObstacleMovement.MovementType.Static,
            ObstacleMovement.MovementType.VerticalOscillate,
            ObstacleMovement.MovementType.HorizontalOscillate
        };
        
        [Tooltip("Chance for obstacle to be static (0-1)")]
        [Range(0f, 1f)]
        public float staticChance = 0.4f;
    }
    
    [Header("Obstacle Configuration")]
    [Tooltip("List of obstacles that can be spawned")]
    public ObstacleConfig[] obstacleConfigs;
    
    [Header("Spawn Settings")]
    [Tooltip("How many obstacles to spawn per segment")]
    public int minObstaclesPerSegment = 3;
    public int maxObstaclesPerSegment = 8;
    
    [Tooltip("Offset obstacles from path center")]
    public Vector2 lateralOffsetRange = new Vector2(-3f, 3f);
    public Vector2 verticalOffsetRange = new Vector2(0f, 5f);
    
    [Header("Difficulty Scaling")]
    [Tooltip("Increase obstacle count over time")]
    public bool scaleDifficulty = true;
    public float difficultyScaleRate = 0.1f;
    
    [Header("Hierarchy")]
    [Tooltip("Optional parent transform for spawned obstacles")]
    public Transform obstaclesParent;
    
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private float totalWeight = 0f;
    private int segmentsGenerated = 0;
    
    private void OnEnable()
    {
        LevelGenerator.onSegmentCreated += OnSegmentCreated;
        Debug.Log("[ObstacleSpawnerSimple] Subscribed to onSegmentCreated event");
    }
    
    private void OnDisable()
    {
        LevelGenerator.onSegmentCreated -= OnSegmentCreated;
    }
    
    private void Start()
    {
        CalculateTotalWeight();
        
        // Create parent if needed
        if (obstaclesParent == null)
        {
            GameObject parent = GameObject.Find("Obstacles");
            if (parent == null)
            {
                parent = new GameObject("Obstacles");
            }
            obstaclesParent = parent.transform;
        }
        
        Debug.Log($"[ObstacleSpawnerSimple] Initialized. Total weight: {totalWeight}, Configs: {obstacleConfigs?.Length ?? 0}");
    }
    
    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        if (obstacleConfigs == null) return;
        
        foreach (var config in obstacleConfigs)
        {
            if (config != null)
                totalWeight += config.spawnWeight;
        }
    }
    
    /// <summary>
    /// Called by LevelGenerator when a new segment is created
    /// </summary>
    private void OnSegmentCreated(LevelSegment segment)
    {
        if (!enableAutomaticSpawning)
        {
            return;
        }
        
        Debug.Log($"[ObstacleSpawnerSimple] Segment created: {segment.name}");
        
        // Start coroutine to wait for segment to be ready
        StartCoroutine(SpawnObstaclesWhenReady(segment));
    }
    
    private System.Collections.IEnumerator SpawnObstaclesWhenReady(LevelSegment segment)
    {
        // Wait for the segment's path to be generated
        int maxFrames = 60;
        while (maxFrames-- > 0)
        {
            if (segment.path != null && 
                segment.path.spline != null && 
                segment.path.spline.points != null &&
                segment.path.spline.points.Length >= 2)
            {
                break;
            }
            yield return null;
        }
        
        // Validate
        if (segment.path == null || segment.path.spline == null)
        {
            Debug.LogWarning($"[ObstacleSpawnerSimple] Segment {segment.name} path not ready after waiting");
            yield break;
        }
        
        if (segment.path.spline.points == null || segment.path.spline.points.Length < 2)
        {
            Debug.LogWarning($"[ObstacleSpawnerSimple] Segment {segment.name} spline has no points");
            yield break;
        }
        
        if (obstacleConfigs == null || obstacleConfigs.Length == 0)
        {
            Debug.LogWarning("[ObstacleSpawnerSimple] No obstacle configs defined!");
            yield break;
        }
        
        if (totalWeight <= 0f)
        {
            CalculateTotalWeight();
            if (totalWeight <= 0f)
            {
                Debug.LogWarning("[ObstacleSpawnerSimple] Total weight is 0!");
                yield break;
            }
        }
        
        // Spawn obstacles
        SpawnObstaclesOnSegment(segment);
        segmentsGenerated++;
    }
    
    private void SpawnObstaclesOnSegment(LevelSegment segment)
    {
        int obstacleCount = Random.Range(minObstaclesPerSegment, maxObstaclesPerSegment + 1);
        
        if (scaleDifficulty)
        {
            obstacleCount += Mathf.FloorToInt(segmentsGenerated * difficultyScaleRate);
        }
        
        Debug.Log($"[ObstacleSpawnerSimple] Spawning {obstacleCount} obstacles on {segment.name}");
        
        Spline spline = segment.path.spline;
        
        for (int i = 0; i < obstacleCount; i++)
        {
            SpawnSingleObstacle(segment, spline, i, obstacleCount);
        }
    }
    
    private void SpawnSingleObstacle(LevelSegment segment, Spline spline, int index, int totalCount)
    {
        // Select random config
        ObstacleConfig config = SelectRandomConfig();
        if (config == null || config.prefab == null)
        {
            Debug.LogWarning("[ObstacleSpawnerSimple] Config or prefab is null");
            return;
        }
        
        // Calculate position along segment
        float percent = (index + 0.5f) / totalCount;
        percent += Random.Range(-0.1f, 0.1f);
        percent = Mathf.Clamp01(percent);
        
        // Evaluate spline
        SplineSample sample = new SplineSample();
        spline.Evaluate(percent, ref sample);
        
        // Add offsets
        float lateralOffset = Random.Range(lateralOffsetRange.x, lateralOffsetRange.y);
        float verticalOffset = Random.Range(verticalOffsetRange.x, verticalOffsetRange.y);
        
        Vector3 spawnPosition = sample.position;
        spawnPosition += sample.right * lateralOffset;
        spawnPosition += sample.up * verticalOffset;
        
        // Create obstacle
        Quaternion rotation = Quaternion.LookRotation(sample.forward, sample.up);
        GameObject obstacle = Instantiate(config.prefab, spawnPosition, rotation, obstaclesParent);
        spawnedObstacles.Add(obstacle);
        
        Debug.Log($"[ObstacleSpawnerSimple] Spawned {config.prefab.name} at {spawnPosition}");
        
        // Configure movement
        ConfigureMovement(obstacle, config);
        
        // Reset obstacle state
        var obstacleBase = obstacle.GetComponent<ObstacleBase>();
        if (obstacleBase != null)
        {
            obstacleBase.ResetObstacle();
        }
    }
    
    private ObstacleConfig SelectRandomConfig()
    {
        if (totalWeight <= 0f) return null;
        
        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        foreach (var config in obstacleConfigs)
        {
            if (config == null) continue;
            cumulative += config.spawnWeight;
            if (randomValue <= cumulative)
            {
                return config;
            }
        }
        
        return obstacleConfigs[0];
    }
    
    private void ConfigureMovement(GameObject obstacle, ObstacleConfig config)
    {
        var movement = obstacle.GetComponent<ObstacleMovement>();
        if (movement == null) return;
        
        bool isStatic = Random.value < config.staticChance;
        
        if (isStatic)
        {
            movement.movementType = ObstacleMovement.MovementType.Static;
        }
        else if (config.possibleMovements != null && config.possibleMovements.Length > 0)
        {
            movement.movementType = config.possibleMovements[Random.Range(0, config.possibleMovements.Length)];
        }
        
        movement.moveDistance = Random.Range(2f, 5f);
        movement.moveSpeed = Random.Range(1f, 3f);
    }
}
