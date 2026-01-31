using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    public class BossController : MonoBehaviour
    {
        // Showing up in Unity

        [SerializeField] private Transform player;
        [SerializeField] private GameObject redProjectilePrefab;
        [SerializeField] private GameObject blackProjectilePrefab;
        [SerializeField] private GameObject blueProjectilePrefab;
        [SerializeField] private GameObject redMask;
        [SerializeField] private GameObject blackMask;
        [SerializeField] private GameObject blueMask;
        [SerializeField] private float minimumCooldownAfterAction;

        [Header("Teleportation")]
        [SerializeField] private Vector2 teleportBounds;
        [SerializeField] private float teleportFrequency;

        [Header("Attacks")]
        [SerializeField] private float attackFrequency;
        [SerializeField] private PillarHealth redPillarHealth;
        [SerializeField] private PillarHealth blackPillarHealth;
        [SerializeField] private PillarHealth bluePillarHealth;

        [Header("Mask Switching")]
        [SerializeField] private float maskSwitchFrequency;
        [SerializeField] private Vector3 activeMaskScale;
        [SerializeField] private float maskSwitchingSpeed;
        [SerializeField] private float rotationSpeed;

        // -------------------

        private float _teleportTimer;
        private float _attackTimer;
        private float _maskSwitchTimer;
        private MaskColor _activeMask;
        private readonly HashSet<MaskColor> _pillarDown = new();
        private int _masksLeft = Enum.GetValues(typeof(MaskColor)).Length;
        private Quaternion _desiredRotation;

        public void TakeDamage(MaskColor mask)
        {
            switch (_masksLeft == 0)
            {
                case false when mask != _activeMask:
                    return;
                case true:
                    // TODO Player wins!
                    Debug.Log("Player wins!");
                    return;
            }

            var pillar = mask switch
            {
                MaskColor.Red => redPillarHealth,
                MaskColor.Black => blackPillarHealth,
                MaskColor.Blue => bluePillarHealth,
                _ => throw new ArgumentOutOfRangeException()
            };

            var isPillarDown = pillar.TakeDamage();
            Debug.Log("Bob: ouch!");

            if (!isPillarDown)
            {
                return;
            }
            
            _pillarDown.Add(mask);
            _masksLeft -= 1;
            Debug.Log($"Bob lost the {mask} mask.");

            if (_masksLeft > 1)
            {
                SwitchMask();
            }
        }


        private void Awake()
        {
        }

        private void Start()
        {
            SetMaskActive(_activeMask);
            
            ResetTeleportTimer();
            ResetAttackTimer();
            ResetMaskSwitchTimer();
        }

        private void Update()
        {
            HandleTeleportTimer();
            HandleAttackTimer();
            HandleMaskSwitchingTimer();
            HandleLerps();
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
            EnsureCooldowns();
        }

        /// <summary>
        /// Handles the countdown timer for the boss's attack mechanism.
        /// When the timer reaches zero, an attack is triggered, and the timer is reset based on the attack frequency.
        /// </summary>
        private void HandleAttackTimer()
        {
            if (_masksLeft < 1)
            {
                return;
            }
            
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
            if (_masksLeft < 2)
            {
                return;
            }
            
            _maskSwitchTimer -= Time.deltaTime;

            if (_maskSwitchTimer > 0f)
            {
                return;
            }
            
            SwitchMask();
            ResetMaskSwitchTimer();
            EnsureCooldowns();
        }

        /// <summary>
        /// Lerps the boss's rotation towards the desired rotation.'
        /// </summary>
        private void HandleLerps()
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                _desiredRotation,
                rotationSpeed * Time.deltaTime
            );
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
            var maskTransform = GetMaskGameObject(_activeMask).transform;

            // Rotate only around Y (ignore vertical difference).
            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            var toMask = maskTransform.position - transform.position;
            toMask.y = 0f;

            // Avoid invalid rotations when something is exactly on top/center.
            if (toPlayer.sqrMagnitude <= 0.0001f || toMask.sqrMagnitude <= 0.0001f)
                return;

            var playerDir = toPlayer.normalized;
            var maskDir = toMask.normalized;

            // Angle needed to rotate the boss around Y so maskDir aligns with playerDir.
            var yawDelta = Vector3.SignedAngle(maskDir, playerDir, Vector3.up);

            // Apply instantly (teleport is an instant action).
            transform.rotation = Quaternion.AngleAxis(yawDelta, Vector3.up) * transform.rotation;
            _desiredRotation = transform.rotation;
        }

        /// <summary>
        /// Spawns a projectile at the boss's mask position.'
        /// </summary>
        private void Attack()
        {
            var projectilePrefab = GetProjectilePrefab(_activeMask);
            var maskPosition = GetMaskGameObject(_activeMask).transform.position;
            
            var projectile = Instantiate(
                projectilePrefab,
                maskPosition,
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

            // Pick a mask that is not the current one and is not down.
            while (newMask == _activeMask || _pillarDown.Contains(newMask))
            {
                newMask = (MaskColor) Random.Range(0, Enum.GetValues(typeof(MaskColor)).Length);
            }
            
            _activeMask = newMask;
            SetMaskActive(_activeMask);
        }

        /// <summary>
        /// Switches the active mask to the next one in the sequence.
        /// </summary>
        /// <param name="mask"></param>
        private void SetMaskActive(MaskColor mask)
        {
            var maskGameObject = GetMaskGameObject(mask);
            
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
            _desiredRotation = Quaternion.AngleAxis(yawDelta, Vector3.up) * transform.rotation;
        }

        /// <summary>
        /// Returns the GameObject corresponding to the given mask.
        /// </summary>
        /// <param name="mask">Mask</param>
        /// <returns>GameObject</returns>
        /// <exception cref="ArgumentOutOfRangeException">Mask is not in enum</exception>
        private GameObject GetMaskGameObject(MaskColor mask) =>
            mask switch
            {
                MaskColor.Red => redMask,
                MaskColor.Black => blackMask,
                MaskColor.Blue => blueMask,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Returns the projectile prefab corresponding to the given mask.
        /// </summary>
        /// <param name="mask">Mask</param>
        /// <returns>Projectile prefab</returns>
        /// <exception cref="ArgumentOutOfRangeException">Mask is not in enum</exception>
        private GameObject GetProjectilePrefab(MaskColor mask) =>
            mask switch
            {
                MaskColor.Red => redProjectilePrefab,
                MaskColor.Black => blackProjectilePrefab,
                MaskColor.Blue => blueProjectilePrefab,
                _ => throw new ArgumentOutOfRangeException()
            };

        private void EnsureCooldowns()
        {
            if (_teleportTimer < minimumCooldownAfterAction)
            {
                ResetTeleportTimer();
            }
            
            if (_attackTimer < minimumCooldownAfterAction)
            {
                ResetAttackTimer();
            }
            
            if (_maskSwitchTimer < minimumCooldownAfterAction)
            {
                ResetMaskSwitchTimer();
            }
        }

        /// <summary>
        /// Resets the teleport timer.
        /// </summary>
        private void ResetTeleportTimer() => _teleportTimer = 10f / (4 - _masksLeft) / teleportFrequency;

        /// <summary>
        /// Resets the attack timer.
        /// </summary>
        private void ResetAttackTimer() => _attackTimer = 10f / (4 - _masksLeft) / attackFrequency;

        /// <summary>
        /// Resets the mask switching timer.
        /// </summary>
        private void ResetMaskSwitchTimer() => _maskSwitchTimer = 10f / (4 - _masksLeft) / maskSwitchFrequency;
    }
}
