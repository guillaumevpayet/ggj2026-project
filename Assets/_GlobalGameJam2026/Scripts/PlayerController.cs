using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    private int3 health;
    private Rigidbody player;
    private GameObject playerObject;
    public float speed;
    public float jumpHeight;
    private bool isJumping = true;
    public float dropRate;
    private Camera mainCamera;

    private void Start()
    {
        player = GetComponent<Rigidbody>();
        playerObject = GameObject.Find("Player");
        mainCamera = Camera.main;
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
        if (playerObject.transform.position.y <= 3.6)
        {
            isJumping = false;
        }
        PlayerLookAtMouse();
    }

    void PlayerLookAtMouse()
    {

    }
}