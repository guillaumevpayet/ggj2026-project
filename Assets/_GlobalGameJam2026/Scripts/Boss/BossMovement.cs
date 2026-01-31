using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    public class BossMovement : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] private Vector2 teleportBounds;
        [SerializeField] private float teleportFrequency;
        [SerializeField] private float attackFrequency;
        [SerializeField] private float maskSwitchFrequency;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject redProjectilePrefab;
        [SerializeField] private GameObject greenProjectilePrefab;
        [SerializeField] private GameObject blueProjectilePrefab;
        [SerializeField] private GameObject redMask;
        [SerializeField] private GameObject greenMask;
        [SerializeField] private GameObject blueMask;
        [SerializeField] private Vector3 activeMaskScale;
        [SerializeField] private float maskSwitchingSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float minimumAttackDelayAfterAction;
        
        // -------------------

        private enum Mask
        {
            Red,
            Green,
            Blue
        };

        private float _teleportTimer;
        private float _attackTimer;
        private float _maskSwitchTimer;
        private Mask _activeMask;

        private void Awake()
        {
        }

        private void Start()
        {
            ResetTeleportTimer();
            ResetAttackTimer();
            ResetMaskSwitchTimer();
        }

        private void Update()
        {
            SetMaskActive(_activeMask);
            
            HandleTeleportTimer();
            HandleAttackTimer();
            HandleMaskSwitchingTimer();
        }

        /// <summary>
       /// Handles the countdown timer for the boss's teleportation mechanism.
       /// When the timer reaches zero, the boss is teleported to a new random position
       /// within the teleport bounds, and the timer is reset based on the teleport frequency.
       /// </summary>
        private void HandleTeleportTimer()
        {
            _teleportTimer -= Time.deltaTime;

            if (_teleportTimer > 0f)
            {
                return;
            }
            
            Teleport();
            ResetTeleportTimer();
            EnsureAttackDelay();
        }

        /// <summary>
       /// Handles the countdown timer for the boss's attack mechanism.
       /// When the timer reaches zero, an attack is triggered, and the timer is reset based on the attack frequency.
       /// </summary>
        private void HandleAttackTimer()
        {
            _attackTimer -= Time.deltaTime;

            if (_attackTimer > 0f)
            {
                return;
            }
            
            Attack();
            ResetAttackTimer();
        }

        /// <summary>
       /// Handles the countdown timer for the boss's mask switching mechanism.'
       /// </summary>
        private void HandleMaskSwitchingTimer()
        {
            _maskSwitchTimer -= Time.deltaTime;

            if (_maskSwitchTimer > 0f)
            {
                return;
            }
            
            SwitchMask();
            ResetMaskSwitchTimer();
            EnsureAttackDelay();
        }

        /// <summary>
       /// Teleports the boss to a random position within the teleportBounds.
       /// </summary>
        private void Teleport()
        {
            var randomX = Random.Range(-teleportBounds.x, teleportBounds.x);
            var randomZ = Random.Range(-teleportBounds.y, teleportBounds.y);
            transform.position = new Vector3(randomX, transform.position.y, randomZ);
            
            // Rotate only around Y (ignore vertical difference).
            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        }
        
        private void Attack()
        {
            var projectilePrefab = GetProjectilePrefab(_activeMask);
            var playerDirection = (player.position - transform.position).normalized;
            
            var projectile = Instantiate(
                projectilePrefab,
                transform.position + playerDirection * 2f,
                Quaternion.identity
            );
            
            projectile.GetComponent<BossProjectile>().Shoot(player);
        }

        /// <summary>
       /// Switches the active mask to a random one from the enum.
       /// </summary>
        private void SwitchMask()
        {
            var newMask = _activeMask;

            // Pick a mask that is not the current one.
            while (newMask == _activeMask)
            {
                newMask = (Mask) Random.Range(0, Enum.GetValues(typeof(Mask)).Length);
            }
            
            SetMaskInactive(_activeMask);
            _activeMask = newMask;
            SetMaskActive(_activeMask);
        }

        /// <summary>
       /// Sets the inactive mask to its original scale.
       /// </summary>
       /// <param name="mask">Inactive mask</param>
        private void SetMaskInactive(Mask mask)
        {
            var maskGameObject = GetMaskGameObject(mask);
            ScaleMask(maskGameObject.transform, Vector3.one);
        }

        /// <summary>
       /// Switches the active mask to the next one in the sequence.
       /// </summary>
       /// <param name="mask"></param>
        private void SetMaskActive(Mask mask)
        {
            var maskGameObject = GetMaskGameObject(mask);
            ScaleMask(maskGameObject.transform, activeMaskScale);
            
            // Rotate only around Y so the ACTIVE MASK direction matches the player direction.
            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            var toMask = maskGameObject.transform.position - transform.position;
            toMask.y = 0f;

            // Avoid invalid rotations when something is exactly on top/center.
            if (toPlayer.sqrMagnitude <= 0.0001f || toMask.sqrMagnitude <= 0.0001f)
                return;

            var playerDir = toPlayer.normalized;
            var maskDir = toMask.normalized;

            // Angle needed to rotate the boss around Y so maskDir aligns with playerDir.
            var yawDelta = Vector3.SignedAngle(maskDir, playerDir, Vector3.up);

            var targetRotation = Quaternion.AngleAxis(yawDelta, Vector3.up) * transform.rotation;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed
            );
        }

        /// <summary>
       /// Lerps the scale of the given mask transform to the desired scale.
       /// </summary>
       /// <param name="maskTransform"></param>
       /// <param name="desiredScale"></param>
        private void ScaleMask(Transform maskTransform, Vector3 desiredScale) =>
            maskTransform.localScale = Vector3.Lerp(
                maskTransform.localScale,
                desiredScale, maskSwitchingSpeed
            );

        /// <summary>
       /// Returns the GameObject corresponding to the given mask.
       /// </summary>
       /// <param name="mask">Mask</param>
       /// <returns>GameObject</returns>
       /// <exception cref="ArgumentOutOfRangeException">Mask is not in enum</exception>
        private GameObject GetMaskGameObject(Mask mask) =>
            mask switch
            {
                Mask.Red => redMask,
                Mask.Green => greenMask,
                Mask.Blue => blueMask,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
       /// Returns the projectile prefab corresponding to the given mask.
       /// </summary>
       /// <param name="mask">Mask</param>
       /// <returns>Projectile prefab</returns>
       /// <exception cref="ArgumentOutOfRangeException">Mask is not in enum</exception>
        private GameObject GetProjectilePrefab(Mask mask) =>
            mask switch
            {
                Mask.Red => redProjectilePrefab,
                Mask.Green => greenProjectilePrefab,
                Mask.Blue => blueProjectilePrefab,
                _ => throw new ArgumentOutOfRangeException()
            };

        private void EnsureAttackDelay()
        {
            if (_attackTimer < minimumAttackDelayAfterAction)
            {
                ResetAttackTimer();
            }
        }
        
        /// <summary>
       /// Resets the teleport timer.
       /// </summary>
        private void ResetTeleportTimer() => _teleportTimer = 10f / teleportFrequency;

        /// <summary>
       /// Resets the attack timer.
       /// </summary>
        private void ResetAttackTimer() => _attackTimer = 10f / attackFrequency;
        
        /// <summary>
       /// Resets the mask switching timer.
       /// </summary>
        private void ResetMaskSwitchTimer() => _maskSwitchTimer = 10f / maskSwitchFrequency;
    }
}
