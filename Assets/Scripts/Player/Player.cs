using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    CharacterController controller;
    new Animation animation;
    Map map;

    float movementSpeed = 20f;
    float gravity = 9.81f;
    float jumpSpeed = 3.5f;
    float directionY;

    public int playerHeight = 2;
    float waterSpeed = 0.5f; 
    public bool inWater = false;

    public Weapon weapon;
    bool canShoot = true;

    new Camera camera;

    public float distance = 50f;
    float lerpSpeed = 20f;
    float xAngle = 60f;
    float yAngle = 45f;
    float velocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        map = GameObject.Find("Map").GetComponent<Map>();
        animation = GetComponent<Animation>();
        animation["Movement"].speed = 6f;
    }

    private void Update()
    {
        if(DataContainer.Instance.isPlaying)
        {
            CheckWater();
            RotatePlayer();
            MovePlayer();
            PlayerShoot();
        }
    }

    private void LateUpdate()
    {
        if (DataContainer.Instance.isPlaying)
        {
            RotateCamera();
        }
    }

    void CheckWater()
    {
        Vector3Int playerWaterPosition = new Vector3Int((int)transform.position.x, (int)transform.position.y + playerHeight, (int)transform.position.z);
        if(map.CheckGroundVoxel(playerWaterPosition))
        {
            inWater = true;
        }
        else
        {
            inWater = false;
        }
    }

    void RotatePlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
            weapon.firePoint.transform.LookAt(hit.point);
        }
    }

    void MovePlayer()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float actualSpeed = movementSpeed;

        if(horizontalInput != 0 || verticalInput != 0)
        {
            animation.Play("Movement");
        }

        if (controller.isGrounded && !inWater)
        {
            if (Input.GetButtonDown("Jump"))
            {
                directionY = jumpSpeed;
            }
        }

        if(inWater)
        {
            directionY = 0;
            actualSpeed *= waterSpeed;
        }
        else
        {
            directionY -= gravity * Time.deltaTime;
        }

        Vector3 xDirection = horizontalInput * camera.transform.right;
        Vector3 yDirection = new Vector3(0, directionY, 0);
        Vector3 zDirection = verticalInput * Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;

        Vector3 direction = xDirection + yDirection + zDirection;

        controller.Move(direction * actualSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        if (Input.GetMouseButton(1))
        {
            velocity += Input.GetAxis("Mouse X");
        }

        yAngle += velocity;

        Quaternion rotation = Quaternion.Euler(xAngle, yAngle, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + transform.position;
        camera.transform.rotation = rotation;
        camera.transform.position = position;

        velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * lerpSpeed);
    }

    void PlayerShoot()
    {
        if(!canShoot)
        {
            return;
        }

        if(Input.GetMouseButton(0) && !inWater)
        {
            weapon.Shot();
            StartCoroutine(CanShoot());
        }
    }

    public void DamagePlayer(int value)
    {
        DataContainer.Instance.playerHealth -= 1;
    }

    IEnumerator CanShoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(0.2f);
        canShoot = true;
    }
}

