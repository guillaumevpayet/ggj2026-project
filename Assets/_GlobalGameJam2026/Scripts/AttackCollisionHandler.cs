using Boss;
using UnityEngine;

public class AttackCollisionHandler : MonoBehaviour
{
    [SerializeField] public BossController controller;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HIT SOMETHING");
        if (other.CompareTag("Boss"))
        {
            Debug.Log("BOSS HIT");
            controller.TakeDamage();
        }
    }
}