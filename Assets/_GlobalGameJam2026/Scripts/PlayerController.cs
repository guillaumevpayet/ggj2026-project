using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    private int currentHearts;

    public event Action<int> OnHealthChanged;

    private Rigidbody player;
    private GameObject playerObject;
    public float speed;
    public float jumpHeight;
    private bool isJumping = true;
    public float dropRate;
    public float hitboxActiveTime;
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private Animator animator;
    bool readyToAttack = true;

    private void Start()
    {
        currentHearts = maxHearts;
        OnHealthChanged?.Invoke(currentHearts);

        player = GetComponent<Rigidbody>();
        playerObject = GameObject.Find("PlayerContainer");

    }

    public void TakeDamage(int damage)
    {
        currentHearts -= damage;
        Debug.Log($"Player took damage. Current hearts: {currentHearts}");
        OnHealthChanged?.Invoke(currentHearts);
        
        if (currentHearts <= 0)
        {
            Debug.Log("Player Died");
            SceneManager.LoadScene("DefeatScreen");
        }
    }

    public void FixedUpdate()
    {
        bool up = Input.GetKey(KeyCode.D);
        bool down = Input.GetKey(KeyCode.A);
        bool left = Input.GetKey(KeyCode.W);
        bool right = Input.GetKey(KeyCode.S);
        Vector3 temp = new (0, 0, 0);
        Vector3 velocity = player.linearVelocity;
        bool attack = Input.GetMouseButton(0);



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
        if (playerObject.transform.position.y <= 3.1)
        {
            isJumping = false;
        }
        if (playerObject.transform.position.y < 1.5)
        {
            isJumping = true;
            playerObject.transform.position = new Vector3(playerObject.transform.position.x, 10, playerObject.transform.position.z);
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

        if (attack)
        {
            playerAttack();
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

    private void playerAttack()
    {
        if (!isJumping && readyToAttack)
        {
            readyToAttack = false;
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        animator.SetTrigger("Attack");
        attackHitbox.enabled = true;

        yield return new WaitForSeconds(hitboxActiveTime);

        attackHitbox.enabled = false;
        yield return new WaitForSeconds(1f);
        readyToAttack = true;
    }
}