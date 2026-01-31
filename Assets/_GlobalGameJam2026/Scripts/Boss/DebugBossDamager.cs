using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    [RequireComponent(typeof(BossController))]
    public class DebugBossDamager : MonoBehaviour
    {
        private BossController _boss;
        private float _timer;

        private void Awake()
        {
            _boss = GetComponent<BossController>();
        }

        private void Start()
        {
            ResetTimer();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer > 0f)
            {
                return;
            }
            
            var damageType = (MaskColor) Random.Range(0, Enum.GetValues(typeof(MaskColor)).Length);
            Debug.Log($"Bob is taking {damageType} damage");
            _boss.TakeDamage();
            ResetTimer();
        }
        
        private void ResetTimer() => _timer = 0.5f;
    }
}