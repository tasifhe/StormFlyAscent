using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Forever;
using System.Collections;

/// <summary>
/// Spawns obstacles along the level path using Dreamteck Forever's Builder system
/// Can be attached to LevelSegments as a Builder component
/// </summary>
public class ObstacleSpawner : Builder
{
    [System.Serializable]
    public class ObstacleConfig
    {
        [Tooltip("Obstacle prefab to spawn")]
        public GameObject prefab;
        
        [Tooltip("Spawn weight (higher = more common)")]
        [Range(0f, 100f)]
        public float spawnWeight = 50f;
        
        [Tooltip("Minimum distance along path between spawns")]
        public float minSpacing = 20f;
        
        [Tooltip("Maximum distance along path between spawns")]
        public float maxSpacing = 50f;
        
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
    [Tooltip("Optional parent transform to keep spawned obstacles organized. If null, will search/create by name.")]
    [SerializeField] private Transform obstaclesParent;

    [Tooltip("If Obstacles Parent is null, the spawner will find/create a GameObject with this name at scene root.")]
    [SerializeField] private string obstaclesParentName = "Obstacles";

    [Tooltip("Create the parent object automatically if it doesn't exist.")]
    [SerializeField] private bool createParentIfMissing = true;
    
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private float totalWeight = 0f;
    private int segmentsGenerated = 0;
    
    protected override void Awake()
    {
        base.Awake();
        CalculateTotalWeight();
    }

    private Transform GetObstaclesParent()
    {
        if (obstaclesParent != null) return obstaclesParent;

        if (!string.IsNullOrWhiteSpace(obstaclesParentName))
        {
            GameObject existing = GameObject.Find(obstaclesParentName);
            if (existing != null)
            {
                obstaclesParent = existing.transform;
                return obstaclesParent;
            }

            if (createParentIfMissing)
            {
                GameObject created = new GameObject(obstaclesParentName);
                obstaclesParent = created.transform;
                return obstaclesParent;
            }
        }

        // Fallback: keep under the segment/spawner
        return transform;
    }
    
    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (var config in obstacleConfigs)
        {
            totalWeight += config.spawnWeight;
        }
    }
    
    protected override void Build()
    {
        base.Build();
    }

    protected override IEnumerator BuildAsync()
    {
        // Wait until the segment's spline is actually generated/populated.
        // Build() can be called while the segment is still being extruded.
        const int maxWaitFrames = 120;
        int framesLeft = maxWaitFrames;

        while (framesLeft-- > 0)
        {
            if (levelSegment != null &&
                levelSegment.path != null &&
                levelSegment.path.spline != null &&
                levelSegment.path.spline.points != null &&
                levelSegment.path.spline.points.Length >= 2)
            {
                break;
            }

            yield return null;
        }

        Debug.Log($"[ObstacleSpawner] BuildAsync() segment={gameObject.name} splineReady={levelSegment?.path?.spline != null}");

        if (levelSegment == null || levelSegment.path == null || levelSegment.path.spline == null)
        {
            Debug.LogWarning("ObstacleSpawner: levelSegment/path/spline still null. Skipping obstacles for this segment.");
            yield break;
        }

        if (levelSegment.path.spline.points == null || levelSegment.path.spline.points.Length < 2)
        {
            Debug.LogWarning("ObstacleSpawner: Spline has no points yet. Skipping obstacles for this segment.");
            yield break;
        }

        if (obstacleConfigs == null || obstacleConfigs.Length == 0)
        {
            Debug.LogWarning("ObstacleSpawner: No obstacle configs defined! Add obstacle prefabs in inspector.");
            yield break;
        }

        CalculateTotalWeight();
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("ObstacleSpawner: Total spawn weight is 0. Make sure at least one config has spawnWeight > 0.");
            yield break;
        }

        ClearExistingObstacles();
        SpawnObstacles();
        segmentsGenerated++;

        Debug.Log($"[ObstacleSpawner] Spawned {spawnedObstacles.Count} obstacles on segmentIndex={segmentsGenerated}");
        yield return null;
    }
    
    private void SpawnObstacles()
    {
        // Calculate how many obstacles to spawn
        int obstacleCount = Random(minObstaclesPerSegment, maxObstaclesPerSegment + 1);

        // Apply difficulty scaling
        if (scaleDifficulty)
        {
            obstacleCount += Mathf.FloorToInt(segmentsGenerated * difficultyScaleRate);
        }

        Debug.Log($"[ObstacleSpawner] Attempting to spawn {obstacleCount} obstacles");
        
        // Spawn obstacles along the path
        for (int i = 0; i < obstacleCount; i++)
        {
            SpawnRandomObstacle(i, obstacleCount);
        }
    }
    
    private void SpawnRandomObstacle(int index, int totalCount)
    {
        // Select random obstacle based on weights
        ObstacleConfig config = SelectRandomConfig();
        if (config == null)
        {
            Debug.LogWarning($"[ObstacleSpawner] SelectRandomConfig returned null");
            return;
        }
        
        if (config.prefab == null)
        {
            Debug.LogWarning($"[ObstacleSpawner] Config has null prefab!");
            return;
        }
        
        // Double-check path and spline are still valid
        if (levelSegment == null || levelSegment.path == null || levelSegment.path.spline == null)
        {
            Debug.LogError($"[ObstacleSpawner] Path/spline became null during spawning!");
            return;
        }
        
        // Calculate position along segment (evenly distributed with some randomness)
        float percent = (index + 0.5f) / totalCount;
        percent += Random(-0.1f, 0.1f);
        percent = Mathf.Clamp01(percent);
        
        // Get position on path
        Dreamteck.Splines.SplineSample result;
        try
        {
            result = levelSegment.path.spline.Evaluate(percent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ObstacleSpawner] Failed to evaluate spline at {percent}: {e.Message}");
            return;
        }
        
        // Add random lateral and vertical offset
        float lateralOffset = Random(lateralOffsetRange.x, lateralOffsetRange.y);
        float verticalOffset = Random(verticalOffsetRange.x, verticalOffsetRange.y);
        
        Vector3 spawnPosition = result.position;
        spawnPosition += result.right * lateralOffset;
        spawnPosition += result.up * verticalOffset;
        
        // Spawn obstacle
        GameObject obstacle = Instantiate(config.prefab, spawnPosition, Quaternion.LookRotation(result.forward, result.up));
        obstacle.transform.SetParent(GetObstaclesParent(), true);
        spawnedObstacles.Add(obstacle);
        
        Debug.Log($"[ObstacleSpawner] Spawned obstacle #{index + 1} at {spawnPosition}");
        
        // Configure movement
        ConfigureObstacleMovement(obstacle, config);
        
        // Initialize obstacle
        var obstacleBase = obstacle.GetComponent<ObstacleBase>();
        if (obstacleBase != null)
        {
            obstacleBase.ResetObstacle();
        }
    }
    
    private ObstacleConfig SelectRandomConfig()
    {
        if (totalWeight <= 0f) return null;
        
        float randomValue = Random(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (var config in obstacleConfigs)
        {
            cumulativeWeight += config.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                return config;
            }
        }
        
        return obstacleConfigs[0]; // Fallback
    }
    
    private void ConfigureObstacleMovement(GameObject obstacle, ObstacleConfig config)
    {
        var movement = obstacle.GetComponent<ObstacleMovement>();
        if (movement == null) return;
        
        // Determine if static or moving
        bool isStatic = Random(0f, 1f) < config.staticChance;
        
        if (isStatic)
        {
            movement.movementType = ObstacleMovement.MovementType.Static;
        }
        else
        {
            // Select random movement from possible types
            if (config.possibleMovements.Length > 0)
            {
                movement.movementType = config.possibleMovements[Random(0, config.possibleMovements.Length)];
            }
        }
        
        // Randomize movement parameters
        movement.moveDistance = Random(2f, 5f);
        movement.moveSpeed = Random(1f, 3f);
    }
    
    private void ClearExistingObstacles()
    {
        foreach (var obstacle in spawnedObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        spawnedObstacles.Clear();
    }
    
    private void OnDestroy()
    {
        ClearExistingObstacles();
    }
}
