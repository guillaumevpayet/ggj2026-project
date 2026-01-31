
using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    private int3 health;
    private Rigidbody player;
    private float speed = 50;
    private float jumpHeight = 400;
    private bool isJumping = false;
    private float correctionBoost = 2f;

    private void Start()
    {
        player = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        bool up = Input.GetKey(KeyCode.D);
        bool down = Input.GetKey(KeyCode.A);
        bool left = Input.GetKey(KeyCode.W);
        bool right = Input.GetKey(KeyCode.S);
        Vector3 temp = new (0, 0, 0);
        Vector3 velocity = player.linearVelocity;

        if (up && !isJumping)
        {
            float tempSpeed = speed;
            if(!down && velocity.x < 0)
            {
                tempSpeed = tempSpeed * correctionBoost;
            }
            temp.x = temp.x + tempSpeed;
        }
        if (down && !isJumping)
        {
            float tempSpeed = speed;
            if (!up && velocity.x > 0)
            {
                tempSpeed = tempSpeed * correctionBoost;
            }
            temp.x = temp.x - tempSpeed;
        }
        if (left && !isJumping)
        {
            float tempSpeed = speed;
            if (!right && velocity.z < 0)
            {
                tempSpeed = tempSpeed * correctionBoost;
            }
            temp.z = temp.z + tempSpeed;
        }
        if (right && !isJumping)
        {
            float tempSpeed = speed;
            if (!left && velocity.z > 0)
            {
                tempSpeed = tempSpeed * correctionBoost;
            }
            temp.z = temp.z - tempSpeed;
        }
        if (!isJumping && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            temp.y = jumpHeight;
            Debug.Log(temp);
        }
        player.AddForce(temp);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isJumping = false;
    }


}