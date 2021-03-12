using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    CharacterController controller;
    new Animation animation;
    Map map;

    float movementSpeed = 10f;
    float gravity = 9.81f;
    float shootDistance = 30f;
    float currentDistance = 0f;
    float directionY;
    int enemyHeight = 2;
    bool inWater;
    int health = 2;

    Transform player;
    public Weapon weapon;
    bool canShoot = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        map = GameObject.Find("Map").GetComponent<Map>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        animation = GetComponent<Animation>();
        animation["Movement"].speed = 6f;
    }

    void Update()
    {
        if(DataContainer.Instance.isPlaying)
        {
            CalculateDistance();
            RotateEnemy();
            CheckWater();
            MoveEnemy();
            EnemyShoot();

            if (health == 0 || currentDistance > 200)
            {
                GameObject.FindGameObjectWithTag("EnemySpawner").GetComponent<EnemySpawner>().CountDownEnemy();
                Destroy(gameObject);
            }
        }
    }

    void CalculateDistance()
    {
        currentDistance = Vector3.Distance(player.position, transform.position);
    }

    void RotateEnemy()
    {
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        weapon.firePoint.transform.LookAt(player.transform.position + new Vector3(0, 2, 0));
    }

    void CheckWater()
    {
        Vector3Int playerWaterPosition = new Vector3Int((int)transform.position.x, (int)transform.position.y + enemyHeight, (int)transform.position.z);
        if (map.CheckGroundVoxel(playerWaterPosition))
        {
            inWater = true;
        }
        else
        {
            inWater = false;
        }
    }

    void MoveEnemy()
    {
        if(currentDistance > 25)
        {
            directionY -= gravity * Time.deltaTime;
            Vector3 direction = transform.forward + new Vector3(0, directionY, 0);

            animation.Play("Movement");

            controller.Move(direction * movementSpeed * Time.deltaTime);
        }
    }

    void EnemyShoot()
    {
        if (!canShoot)
        {
            return;
        }

        if (currentDistance <= shootDistance && !inWater)
        {
            weapon.Shot();
            StartCoroutine(CanShoot());
        }
    }

    public void DamageEnemy(int value)
    {
        health -= value;
    }

    IEnumerator CanShoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(1f);
        canShoot = true;
    }
}
