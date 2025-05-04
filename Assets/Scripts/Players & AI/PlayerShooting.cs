using UnityEngine;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour, IBulletOwner
{
    [Header("Settings")]
    public Bullet bulletPrefab;
    public Transform firePoint;
    public float fireDelay = 0.3f;
    public int maxBullets = 5; // Strict limit

    private Queue<Bullet> bulletPool = new Queue<Bullet>();
    private List<Bullet> activeBullets = new List<Bullet>(); // Tracks ACTIVE bullets
    private float lastFireTime;
    private PlayerMovement PlayerControls;


    void Start()
    {
        // Initialize pool with exactly maxBullets + 1 buffer
        for (int i = 0; i < maxBullets + 1; i++)
        {
            CreateNewBullet();
        }
        PlayerControls = GetComponent<PlayerMovement>();
        
    }

    void Update()
    {
        if (Input.GetKeyDown(PlayerControls.shootKey) && CanShoot())
        {
            Shoot();
        }
    }

    public Transform GetBulletSpawnPoint()
    {
        return firePoint; // Your existing firePoint transform
    }

    bool CanShoot()
    {
        return Time.time >= lastFireTime + fireDelay &&
               activeBullets.Count < maxBullets;
    }

    void Shoot()
    {
        if (bulletPool.Count == 0)
        {
            CreateNewBullet(); // Emergency fallback
        }

        Bullet bullet = bulletPool.Dequeue();
        PrepareBullet(bullet);
        activeBullets.Add(bullet); // Add to active list
        lastFireTime = Time.time;
    }

    void PrepareBullet(Bullet bullet)
    {
        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        bullet.Activate(firePoint.up);
    }

    void CreateNewBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.owner = this;
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        // Remove from active list first
        if (activeBullets.Contains(bullet))
        {
            activeBullets.Remove(bullet);
        }

        // Then return to pool
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}