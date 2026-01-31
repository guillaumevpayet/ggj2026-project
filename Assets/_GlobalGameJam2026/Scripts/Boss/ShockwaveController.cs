using System;
using UnityEngine;

namespace Boss
{
    public class ShockwaveController : BossProjectile
    {
        // Showing up in Unity
        
        [SerializeField] private float maxScale;
        [SerializeField] private float innerRadius;
        [SerializeField] private float outerRadius;
        [SerializeField] private float height;
        
        // -------------------
        
        public override void Shoot(Vector3 origin, Transform player, BossProjectilePool pool, Vector3? direction = null)
        {
            transform.localPosition = origin;
            Player = player;
            Pool = pool;
        }

        private void Update()
        {
            transform.localScale += new Vector3(1f, 0f, 1f) * (speed * Time.deltaTime);
            
            var playerPosition = Player.position;
            var distance = Vector3.Distance(playerPosition, transform.position);
            var scaledInnerRadius = innerRadius * transform.localScale.x;
            var scaledOuterRadius = outerRadius * transform.localScale.x;

            if (distance >= scaledInnerRadius && distance <= scaledOuterRadius && playerPosition.y <= height)
            {
                // TODO Deal damage to player
                Debug.Log("Hit player");
            }

            if (transform.localScale.x >= maxScale)
            {
                Pool.AddProjectileToPool(gameObject);
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            // Do nothing
        }
    }
}
