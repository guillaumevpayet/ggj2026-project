using UnityEngine;

namespace Boss
{
    public class BossProjectile : MonoBehaviour
    {
        private Transform _player;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Shoot(Transform player)
        {
            var playerDirection = (player.position - transform.position).normalized;
            _rb.linearVelocity = playerDirection * 50f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // TODO Deal damage to player
            }
            
            Destroy(gameObject);
        }
    }
}
