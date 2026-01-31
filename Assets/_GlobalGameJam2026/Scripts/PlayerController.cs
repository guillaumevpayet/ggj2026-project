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
    [SerializeField] private Animator animator;

    private void Start()
    {
        player = GetComponent<Rigidbody>();
        playerObject = GameObject.Find("PlayerContainer");
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
        if (playerObject.transform.position.y <= 4.1)
        {
            isJumping = false;
        }
        adjustRotation(temp);
        if (temp != new Vector3(0, 0, 0))
        {
            animator.SetFloat("Speed", 5);
        }else
        {
            animator.SetFloat("Speed", 0);
        }
        if (isJumping)
        {
            animator.SetBool("Jumping", true);
        } else
        {
            animator.SetBool("Jumping", false);
        }


    }
    private void adjustRotation(Vector3 velocity)
    {
        if (velocity.x < 0)
        {
            if (velocity.z < 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(-0.5f, 0, -0.5f));
            }
            if (velocity.z > 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(-0.5f, 0, 0.5f));
            }
            if (velocity.z == 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(-1f, 0, 0));
            }
        }
        if (velocity.x > 0)
        {
            if (velocity.z < 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.5f, 0, -0.5f));
            }
            if (velocity.z > 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.5f, 0, 0.5f));
            }
            if (velocity.z == 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(1f, 0, 0));
            }
        }
        if (velocity.x == 0)
        {
            if (velocity.z < 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1));
            }
            if (velocity.z > 0)
            {
                float curRotation = playerObject.transform.localRotation.z;
                playerObject.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1));
            }
        }
    }
}