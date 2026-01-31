using Boss;
using UnityEngine;

public class AttackCollisionHandler : MonoBehaviour
{
    public BossController controller;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
        {
            controller.TakeDamage();
        }
    }
}