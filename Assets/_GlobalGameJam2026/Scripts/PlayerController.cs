
using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    private int3 health;
    private Rigidbody player;
    public float speed;
    public float jumpHeight;
    private bool isJumping = true;
    public float dropRate;

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
            temp.x = temp.x + speed;
        }
        if (down && !isJumping)
        {
            temp.x = temp.x - speed;
        }
        if (left && !isJumping)
        {
            temp.z = temp.z + speed;
        }
        if (right && !isJumping)
        {
            temp.z = temp.z - speed;
        }
        if (!isJumping && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            temp.y = jumpHeight;
            Debug.Log(temp);
        }
        else if (isJumping)
        {
            temp.x = velocity.x;
            temp.y = velocity.y - dropRate;
            temp.z = velocity.z;
        }
        player.linearVelocity = (temp);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isJumping = false;
    }


}