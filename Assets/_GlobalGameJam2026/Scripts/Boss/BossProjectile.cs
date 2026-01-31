using System;
using UnityEngine;

namespace Boss
{
    public class BossProjectile : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] private float speed;
        
        // -------------------
        
        private Transform _player;
        private BossProjectilePool _pool;
        private Vector3 _direction;

        public void Shoot(Vector3 origin, Transform player, BossProjectilePool pool, Vector3? direction = null)
        {
            transform.localPosition = origin;
            _direction = direction ?? (player.position - transform.position).normalized;
            _player = player;
            _pool = pool;
        }

        private void Update()
        {
            transform.localPosition += _direction * (Time.deltaTime * speed);
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
