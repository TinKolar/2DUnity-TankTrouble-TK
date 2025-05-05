using UnityEngine;
using System.Collections.Generic;

public class AITank : MonoBehaviour, IBulletOwner
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 120f;
    public float pathUpdateInterval = 0.5f;
    public float distanceFromPlayer = 5f;
    public float angleAdjustment = 5f;
    public float distanceDijkstraCheck = 0.5f;


    [Header("Combat Settings")]
    public Bullet bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 8f;
    public float shootCooldown = 1.5f;
    public LayerMask obstacleMask;
    public LayerMask ShootingMask;
    public float fireDelay = 0.3f;
    [Tooltip("How many times per second to check for shooting opportunities")]
    public float shootingChecksPerSecond = 3f;
    public int maxBullets = 5; // Added bullet pool limit
    public float rotationPauseAfterShot = 5f; // New field


    [Header("Bullet Avoidance")]
    public float bulletDetectionRadius = 4f;
    public float bulletDangerAngle = 45f;
    public float avoidanceRotationMultiplier = 1.5f;
    public float avoidanceCheckInterval = 0.2f;
    public LayerMask bulletLayer;

    [Header("Targeting")]
    public float targetUpdateInterval = 5f;
    private float _lastTargetUpdate;
    private List<GameObject> _allPlayers = new List<GameObject>();


    private Transform _player;
    private Rigidbody2D _rb;
    private float _lastShotTime;
    private float _lastPathUpdate;
    private float _lastShootingCheckTime;
    private List<Vector2> _currentPath = new List<Vector2>();
    private Queue<Bullet> _bulletPool = new Queue<Bullet>(); // Bullet object pool
    private List<Bullet> _activeBullets = new List<Bullet>(); // Track active bullets
    private bool canShootPlayer;
    private float _rotationLockTime = -1f;

    private float _lastAvoidanceCheck;
    private bool _isAvoiding;
    private Vector2 _avoidanceDirection;


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        FindAllPlayers();
        UpdateTarget();

        // Initialize bullet pool
        for (int i = 0; i < maxBullets + 1; i++)
        {
            CreateNewBullet();
        }

        SpawnManager.RegisterPlayer(gameObject);
        GameFinishManager.RegisterTank(gameObject);

    }

    void Update()
    {
        if (_player == null || Time.time - _lastTargetUpdate > targetUpdateInterval)
        {
            ForceTargetUpdate();
            if (_player == null) return;
        }

        HandlePathUpdates();

        // Check for bullets at intervals
        if (Time.time - _lastAvoidanceCheck > avoidanceCheckInterval)
        {
            _lastAvoidanceCheck = Time.time;
            _isAvoiding = EvaluateBulletThreat();
        }

        if (_isAvoiding)
        {
            PerformAvoidance();
        }
        else if (Time.time >= _rotationLockTime)
        {
            HandleMovement();
        }

        if (Time.time - _lastShootingCheckTime >= 1f / shootingChecksPerSecond)
        {
            _lastShootingCheckTime = Time.time;
            if (CanShoot())
            {
                HandleShooting();
            }
        }

        if (Time.time - _lastTargetUpdate > targetUpdateInterval)
        {
            UpdateTarget();
        }

    }

    void ForceTargetUpdate()
    {
        // Full refresh of player list
        FindAllPlayers();
        _allPlayers.RemoveAll(player => player == null || !player.activeInHierarchy);

        // Find closest
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var player in _allPlayers)
        {
            if (player == null) continue;
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = player.transform;
            }
        }

        _player = closest;
        _lastTargetUpdate = Time.time;

        if (_player != null)
        {
            UpdatePath();
        }
    }

    void FindAllPlayers()
    {
        _allPlayers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
    }


    void UpdateTarget()
    {
        _allPlayers.RemoveAll(player => player == null);

        // If no players left, return
        if (_allPlayers.Count == 0)
        {
            _player = null;
            return;
        }

        // Find closest living player
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in _allPlayers)
        {
            if (player == null) continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }

        _player = closest;
        _lastTargetUpdate = Time.time;

        // Immediately update path if we found a new target
        if (_player != null)
        {
            UpdatePath();
        }
    }

    bool EvaluateBulletThreat()
    {
        Collider2D[] bullets = Physics2D.OverlapCircleAll(transform.position, bulletDetectionRadius, bulletLayer);

        foreach (var col in bullets)
        {
            Bullet bullet = col.GetComponent<Bullet>();
            if (bullet != null && bullet.owner != (IBulletOwner)this)
            {
                Vector2 bulletToTank = (Vector2)transform.position - (Vector2)bullet.transform.position;
                float approachAngle = Vector2.Angle(bullet.transform.up, bulletToTank);

                if (approachAngle < bulletDangerAngle)
                {
                    // Calculate avoidance direction (perpendicular to bullet path)
                    Vector2 bulletDir = bullet.transform.up;
                    _avoidanceDirection = new Vector2(-bulletDir.y, bulletDir.x);

                    // Choose direction that requires less rotation
                    float angleToAvoidance = Vector2.SignedAngle(transform.up, _avoidanceDirection);
                    if (Mathf.Abs(angleToAvoidance) > 90f)
                    {
                        _avoidanceDirection = -_avoidanceDirection;
                    }

                    return true;
                }
            }
        }
        return false;
    }

    void PerformAvoidance()
    {
        // Rotate toward avoidance direction
        float angle = Vector2.SignedAngle(transform.up, _avoidanceDirection);
        float rotateAmount = Mathf.Clamp(angle,
            -rotationSpeed * avoidanceRotationMultiplier * Time.deltaTime,
            rotationSpeed * avoidanceRotationMultiplier * Time.deltaTime);

        transform.Rotate(0, 0, rotateAmount);

        // Move forward/backward based on alignment
        if (Mathf.Abs(angle) < 45f)
        {
            _rb.velocity = transform.up * moveSpeed;
        }
        else if (Mathf.Abs(angle) > 135f)
        {
            _rb.velocity = -transform.up * (moveSpeed * 0.5f);
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }

        // Clear path to prevent interference
        if (_currentPath.Count > 0)
        {
            _currentPath.Clear();
        }
    }




        // Movement functions remain unchanged

        void HandlePathUpdates()
    {
        if (Time.time - _lastPathUpdate > pathUpdateInterval)
        {
            UpdatePath();
            _lastPathUpdate = Time.time;
        }
    }

    void UpdatePath()
    {
        if (_player == null)
        {
            UpdateTarget(); // Try to find a new target immediately
            if (_player == null) return; // Still no target found
        }

        // Add simple distance check to prevent unnecessary updates
        if (_currentPath.Count > 0 &&
            Vector2.Distance(_player.position, _currentPath[_currentPath.Count - 1]) < 1f)
        {
            return;
        }

        _currentPath = Pathfinder.Dijkstra(transform.position, _player.position, obstacleMask);
    }


    void HandleMovement()
    {
        // Early exit if no path
        if (_currentPath == null || _currentPath.Count == 0)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Check if there's a wall immediately in front of us
        bool wallInFront = CheckWallInFront();

        // Only stop moving if we're close enough AND can shoot the player AND no wall immediately in front
        if (distanceToPlayer <= distanceFromPlayer && canShootPlayer && !wallInFront)
        {
            Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
            float playerAngle = Vector2.SignedAngle(transform.up, toPlayer);
            float playerRotateAmount = Mathf.Clamp(playerAngle, -rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime);
            transform.Rotate(0, 0, playerRotateAmount);
            _rb.velocity = Vector2.zero;
            return;
        }

        // Reset canShootPlayer only when we're moving toward waypoint
        canShootPlayer = false;

        // Safely get the current waypoint
        Vector2 currentWaypoint;
        try
        {
            currentWaypoint = _currentPath[0];
        }
        catch (System.ArgumentOutOfRangeException)
        {
            _currentPath.Clear();
            _rb.velocity = Vector2.zero;
            return;
        }

        Vector2 toWaypoint = currentWaypoint - (Vector2)transform.position;
        float distance = toWaypoint.magnitude;

        // Rotation toward waypoint
        float angle = Vector2.SignedAngle(transform.up, toWaypoint);
        float rotateAmount = Mathf.Clamp(angle, -rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime);
        transform.Rotate(0, 0, rotateAmount);

        // Movement logic
        if (Mathf.Abs(angle) < angleAdjustment)
        {
            _rb.velocity = transform.up * moveSpeed;
        }
        else if (Mathf.Abs(angle) > 175f)
        {
            _rb.velocity = -transform.up * (moveSpeed * 0.5f);
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }

        if (distance < distanceDijkstraCheck)
        {
            // Safely remove waypoint
            if (_currentPath.Count > 0)
            {
                _currentPath.RemoveAt(0);
            }
        }
    }


    bool CheckWallInFront()
    {
        // Check if there's a wall very close in front of us (within 1 unit)
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            transform.up,
            1f,
            obstacleMask
        );

        return hit.collider != null && hit.collider.CompareTag("Wall");
    }

    void HandleShooting()
    {
        Vector2 direction = transform.up;
        float maxDistance = 20f;

        RaycastHit2D hit = Physics2D.Raycast(
            bulletSpawnPoint.position,
            direction,
            maxDistance,
            ShootingMask
        );

        Debug.DrawRay(bulletSpawnPoint.position, direction * hit.distance,
                     hit.collider == null ? Color.green : Color.red, 0.06f);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                canShootPlayer = true;
                // Lock rotation when we have a direct shot at player
                _rotationLockTime = Time.time + rotationPauseAfterShot;
                Shoot();
            }
            else if (hit.collider.CompareTag("Wall"))
            {
                CheckBounceShots(hit, direction, maxDistance);
            }
        }
        else
        {
            canShootPlayer = false;
        }
    }

    void CheckBounceShots(RaycastHit2D hit, Vector2 direction, float distance, int bouncesRemaining = 3)
    {
        if (bouncesRemaining <= 0) return;

        Vector2 bounceDir = Vector2.Reflect(direction, hit.normal);
        Vector2 bounceOrigin = hit.point + bounceDir * 0.1f;
        distance -= hit.distance;

        if (distance <= 0) return;

        RaycastHit2D newHit = Physics2D.Raycast(
            bounceOrigin,
            bounceDir,
            distance,
            ShootingMask
        );

        Debug.DrawRay(bounceOrigin, bounceDir * newHit.distance,
             newHit.collider == null ? Color.green : Color.yellow, 0.04f);

        if (newHit.collider != null && newHit.collider.CompareTag("Player"))
        {
            canShootPlayer = true;
            _rotationLockTime = Time.time + rotationPauseAfterShot;
            Shoot();
        }
        else if (newHit.collider != null)
        {
            CheckBounceShots(newHit, bounceDir, distance, bouncesRemaining - 1);
        }
    }

    void Shoot()
    {
        if (_bulletPool.Count == 0)
        {
            CreateNewBullet(); // Emergency fallback
        }

        Bullet bullet = _bulletPool.Dequeue();
        PrepareBullet(bullet);
        _activeBullets.Add(bullet);
        _lastShotTime = Time.time;
    }

    void PrepareBullet(Bullet bullet)
    {
        bullet.transform.SetPositionAndRotation(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.Activate(bulletSpawnPoint.up * bulletSpeed);
        bullet.gameObject.SetActive(true);
    }

    void CreateNewBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.owner = this;
        bullet.gameObject.SetActive(false);
        _bulletPool.Enqueue(bullet);
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        if (_activeBullets.Contains(bullet))
        {
            _activeBullets.Remove(bullet);
        }

        bullet.gameObject.SetActive(false);
        _bulletPool.Enqueue(bullet);
    }

    public Transform GetBulletSpawnPoint()
    {
        return bulletSpawnPoint;
    }

    bool CanShoot()
    {
        return Time.time >= _lastShotTime + fireDelay &&
               _activeBullets.Count < maxBullets;
    }


    void OnDrawGizmos()
    {
        // Draw forward direction (matches shooting)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(bulletSpawnPoint.position, transform.up * 2f);

        // Draw current path
        if (_currentPath != null && _currentPath.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                // Draw main path line
                Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);

                // Draw node indicators
                Gizmos.color = i == 0 ? Color.green : Color.blue;
                Gizmos.DrawWireSphere(_currentPath[i], 0.1f);

                // Draw exploration grid (Dijkstra-specific)
                if (i < _currentPath.Count - 1)
                {
                    DrawDijkstraGrid(_currentPath[i], _currentPath[i + 1]);
                }
            }
            Gizmos.DrawWireSphere(_currentPath[_currentPath.Count - 1], 0.15f);
        }
        
    }

    void DrawDijkstraGrid(Vector2 from, Vector2 to)
    {
        // Show grid exploration between path nodes
        float gridSize = 0.4f;
        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);

        // Draw grid points along the path
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        for (float d = 0; d < distance; d += gridSize)
        {
            Vector2 point = from + direction * d;
            Gizmos.DrawWireCube(point, Vector3.one * gridSize * 0.8f);
        }

        // Draw orthogonal exploration
        Vector2[] orthoDirs = {
        new Vector2(-direction.y, direction.x),
        new Vector2(direction.y, -direction.x)
    };

        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        foreach (Vector2 orthoDir in orthoDirs)
        {
            for (float d = gridSize; d < gridSize * 3; d += gridSize)
            {
                Vector2 orthoPoint = from + orthoDir * d;
                Gizmos.DrawWireCube(orthoPoint, Vector3.one * gridSize * 0.5f);
            }
        }
    }
    private void OnDestroy()
    {
        GameFinishManager.UnregisterTank(gameObject);
    }


}