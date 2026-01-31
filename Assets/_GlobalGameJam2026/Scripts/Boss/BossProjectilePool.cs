using System.Collections.Generic;
using UnityEngine;

namespace Boss
{
    public class BossProjectilePool : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int poolSize;
        
        // -------------------
        
        private readonly Stack<GameObject> _projectiles = new();
        
        /// <summary>
        /// Returns a projectile from the pool. If the pool is empty, a new one is instantiated.
        /// </summary>
        /// <returns>Projectile</returns>
        public GameObject GetProjectile()
        {
            var projectile = _projectiles.Count == 0 ? Instantiate(projectilePrefab) : _projectiles.Pop();
            projectile.transform.parent = null;
            projectile.SetActive(true);
            return projectile;
        }

        /// <summary>
        /// Adds a projectile to the pool.
        /// </summary>
        /// <param name="projectile">Projectile</param>
        public void AddProjectileToPool(GameObject projectile)
        {
            projectile.transform.parent = transform;
            projectile.SetActive(false);
            _projectiles.Push(projectile);
        }
        
        private void Awake()
        {
            for (var i = 0; i < poolSize; i++)
            {
                var projectile = Instantiate(projectilePrefab);
                AddProjectileToPool(projectile);
            }
        }
    }
}
