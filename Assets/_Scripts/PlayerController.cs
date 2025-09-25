using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float ascentSpeed = 10f;
    [SerializeField] private float moveSensitivity = 5f;
    [SerializeField] private float boostForce = 15f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float xMin = -5f;
    [SerializeField] private float xMax = 5f;
    
    [Header("Physics Settings")]
    [SerializeField] private float dragFactor = 2f;
    
    private Rigidbody rb;
    private bool canBoost = true;
    private float boostCooldown = 0.1f;
    private float lastBoostTime;
    
    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // Ensure we have a Rigidbody
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody component!");
        }
        
        // Set initial physics properties
        rb.linearDamping = 1f;
        rb.angularDamping = 5f;
        
        // Initialize last boost time
        lastBoostTime = -boostCooldown;
    }
    
    void Update()
    {
        // Handle input
        HandleTiltInput();
        HandleTapInput();
        
        // Update boost cooldown
        UpdateBoostCooldown();
    }
    
    void FixedUpdate()
    {
        // Apply constant ascent force
        ApplyAscentForce();
        
        // Clamp horizontal position within boundaries
        ClampHorizontalPosition();
    }
    
    /// <summary>
    /// Handles accelerometer input for horizontal movement
    /// </summary>
    private void HandleTiltInput()
    {
        // Get accelerometer input (tilt)
        float tiltInput = Input.acceleration.x;
        
        // Apply horizontal force based on tilt
        Vector3 horizontalForce = new Vector3(tiltInput * moveSensitivity, 0, 0);
        rb.AddForce(horizontalForce, ForceMode.Force);
    }
    
    /// <summary>
    /// Handles tap input for boost mechanic
    /// </summary>
    private void HandleTapInput()
    {
        // Check for tap input (mouse click for testing in editor, touch for mobile)
        bool tapDetected = false;
        
        #if UNITY_EDITOR
        // Mouse input for testing in Unity editor
        if (Input.GetMouseButtonDown(0))
            tapDetected = true;
        #elif UNITY_ANDROID || UNITY_IOS
        // Touch input for mobile devices
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            tapDetected = true;
        #endif
        
        // Apply boost if tap detected and cooldown is ready
        if (tapDetected && canBoost)
        {
            ApplyBoost();
        }
    }
    
    /// <summary>
    /// Applies constant upward force for ascent
    /// </summary>
    private void ApplyAscentForce()
    {
        Vector3 ascentForce = new Vector3(0, ascentSpeed, 0);
        rb.AddForce(ascentForce, ForceMode.Force);
    }
    
    /// <summary>
    /// Applies instantaneous upward boost force
    /// </summary>
    private void ApplyBoost()
    {
        Vector3 boost = new Vector3(0, boostForce, 0);
        rb.AddForce(boost, ForceMode.Impulse);
        
        // Set cooldown
        canBoost = false;
        lastBoostTime = Time.time;
        
        // Optional: Add visual/audio feedback here
        // Example: PlayBoostEffect();
    }
    
    /// <summary>
    /// Updates the boost cooldown timer
    /// </summary>
    private void UpdateBoostCooldown()
    {
        if (!canBoost && Time.time >= lastBoostTime + boostCooldown)
        {
            canBoost = true;
        }
    }
    
    /// <summary>
    /// Clamps the player's horizontal position within defined boundaries
    /// </summary>
    private void ClampHorizontalPosition()
    {
        Vector3 position = transform.position;
        
        // Clamp X position within boundaries
        if (position.x < xMin)
        {
            position.x = xMin;
            transform.position = position;
            
            // Stop horizontal velocity when hitting boundary
            Vector3 velocity = rb.linearVelocity;
            velocity.x = Mathf.Max(0, velocity.x); // Only allow positive velocity when at left boundary
            rb.linearVelocity = velocity;
        }
        else if (position.x > xMax)
        {
            position.x = xMax;
            transform.position = position;
            
            // Stop horizontal velocity when hitting boundary
            Vector3 velocity = rb.linearVelocity;
            velocity.x = Mathf.Min(0, velocity.x); // Only allow negative velocity when at right boundary
            rb.linearVelocity = velocity;
        }
    }
    
    /// <summary>
    /// Handles collision with other objects
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Handle hazard collisions
        if (collision.gameObject.CompareTag("Hazard"))
        {
            // TODO: Implement game over logic
            // Example: GameManager.Instance.GameOver();
            // Example: PlayCrashEffect();
            Debug.Log("Player hit a hazard! Game Over!");
        }
        
        // Handle platform or surface collisions
        if (collision.gameObject.CompareTag("Platform"))
        {
            // TODO: Implement platform interaction
            // Example: Apply bounce effect or stabilize player
            Debug.Log("Player landed on platform");
        }
    }
    
    /// <summary>
    /// Handles trigger interactions with collectibles and special zones
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Handle energy shard collection
        if (other.gameObject.CompareTag("EnergyShard"))
        {
            // TODO: Implement score increase and shard collection
            // Example: ScoreManager.Instance.AddScore(100);
            // Example: other.gameObject.SetActive(false);
            // Example: PlayCollectEffect();
            Debug.Log("Energy Shard collected! Score increased!");
        }
        
        // Handle power-up collection
        if (other.gameObject.CompareTag("PowerUp"))
        {
            // TODO: Implement power-up effects
            // Example: ApplyPowerUpEffect(other.GetComponent<PowerUp>().type);
            Debug.Log("Power-up collected!");
        }
        
        // Handle wind currents or speed zones
        if (other.gameObject.CompareTag("WindCurrent"))
        {
            // TODO: Apply temporary speed boost or directional force
            // Example: StartCoroutine(ApplyWindEffect(duration));
            Debug.Log("Entered wind current!");
        }
    }
    
    /// <summary>
    /// Public method to get current velocity (useful for UI or other systems)
    /// </summary>
    public Vector3 GetVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector3.zero;
    }
    
    /// <summary>
    /// Public method to check if player can boost (useful for UI indicators)
    /// </summary>
    public bool CanBoost()
    {
        return canBoost;
    }
    
    /// <summary>
    /// Public method to modify movement sensitivity at runtime
    /// </summary>
    public void SetMoveSensitivity(float newSensitivity)
    {
        moveSensitivity = newSensitivity;
    }
}
