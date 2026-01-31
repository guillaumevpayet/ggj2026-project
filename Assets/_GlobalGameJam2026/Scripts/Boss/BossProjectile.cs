using UnityEngine;

namespace Boss
{
    public class BossProjectile : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] protected float speed;
        
        // -------------------
        
        protected Transform Player;
        protected BossProjectilePool Pool;
        private Vector3 _direction;

        public virtual void Shoot(Vector3 origin, Transform player, BossProjectilePool pool, Vector3? direction = null)
        {
            transform.localPosition = origin;
            _direction = direction ?? (player.position - transform.position).normalized;
            Player = player;
            Pool = pool;
        }

        private void Update()
        {
            transform.localPosition += _direction * (Time.deltaTime * speed);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.transform == Player)
            {
                // TODO Deal damage to player
                Debug.Log("Hit player");
            }
            
            Pool.AddProjectileToPool(gameObject);
        }
    }
}
