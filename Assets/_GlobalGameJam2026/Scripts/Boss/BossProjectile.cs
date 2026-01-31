using UnityEngine;

namespace Boss
{
    public class BossProjectile : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] private float speed;
        
        // -------------------
        
        private Transform _player;
        private Rigidbody _rb;
        private BossProjectilePool _pool;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Shoot(Vector3 origin, Transform player, BossProjectilePool pool)
        {
            transform.position = origin;
            var playerDirection = (player.position - transform.position).normalized;
            _rb.linearVelocity = playerDirection * speed;
            _player = player;
            _pool = pool;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform == _player)
            {
                // TODO Deal damage to player
            }
            
            _pool.AddProjectileToPool(gameObject);
        }
    }
}
