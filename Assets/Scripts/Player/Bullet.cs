using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed = 150f;

    Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
        StartCoroutine(DestroyBullet());
    }

    //Checking the distance from previous position in every frame with raycast
    void Update()
    {
        previousPosition = transform.position;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        RaycastHit bulletHit;
        if(Physics.Raycast(previousPosition, (transform.position - previousPosition).normalized, out bulletHit, (transform.position - previousPosition).magnitude))
        {
            Destroy(gameObject);

            string tag = bulletHit.collider.gameObject.tag;
            
            if(tag == "Ground")
            {
                Vector3Int voxelPosition = Vector3Int.FloorToInt(bulletHit.point + (bulletHit.normal * -0.5f));
                bulletHit.collider.gameObject.GetComponentInParent<Map>().GetChunk(voxelPosition).DestructVoxel(voxelPosition);
            }

            if (tag == "Player")
            {
                bulletHit.collider.gameObject.GetComponentInParent<Player>().DamagePlayer(1);
            }

            if (tag == "Enemy")
            {
                bulletHit.collider.gameObject.GetComponentInParent<Enemy>().DamageEnemy(1);
            }
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
