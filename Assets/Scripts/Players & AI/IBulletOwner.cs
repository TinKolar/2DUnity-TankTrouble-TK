using UnityEngine;
using System.Collections.Generic;
public interface IBulletOwner
{
    void ReturnBullet(Bullet bullet);
    Transform GetBulletSpawnPoint();
}