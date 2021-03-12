using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Bullet bullet;
    public int damage = 1;

    public Transform firePoint;

    public void Shot()
    {
        Bullet newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
    }
}
