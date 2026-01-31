using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    public class BossController : MonoBehaviour
    {
        // Showing up in Unity

        [SerializeField] private Transform player;
        [SerializeField] private GameObject redMask;
        [SerializeField] private GameObject blackMask;
        [SerializeField] private GameObject blueMask;
        [SerializeField] private float minimumCooldownAfterAction;
        [SerializeField] private PillarHealth redPillarHealth;
        [SerializeField] private PillarHealth blackPillarHealth;
        [SerializeField] private PillarHealth bluePillarHealth;
        [SerializeField] private BossProjectilePool redProjectilePool;
        [SerializeField] private BossProjectilePool blackProjectilePool;
        [SerializeField] private BossProjectilePool blueProjectilePool;

        [Header("Teleportation")]
        [SerializeField] private Vector2 teleportBounds;
        [SerializeField] private float teleportFrequency;
        [SerializeField] private float teleportDuration;
        [SerializeField] private float teleportMaxHeight;

        [Header("Attacks")]
        [SerializeField] private float attackFrequency;
        [SerializeField] private float rateOfFire;
        [SerializeField] private float attackDuration;
        [SerializeField] private float vulnerabilityDuration;

        [Header("Mask Switching")]
        [SerializeField] private float maskSwitchFrequency;
        [SerializeField] private float maskSwitchingSpeed;
        [SerializeField] private float rotationSpeed;

        // -------------------

        private float _teleportTimer;
        private float _attackTimer;
        private float _maskSwitchTimer;
        private MaskColor _activeMask;
        private readonly HashSet<MaskColor> _pillarDown = new();
        private int _masksLeft = Enum.GetValues(typeof(MaskColor)).Length;
        private Transform _activeMaskTransform;
        
        // Jump (teleport) interpolation variables
        private Vector3 _previousPosition;
        private Vector3 _nextPosition;
        private float _jumpProgressRemaining;
        
        
        public bool IsVulnerable { get; private set; }

        
        // public void TakeDamage(MaskColor mask)
        public void TakeDamage()
        {
            var mask = _activeMask;
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
            if (_jumpProgressRemaining > 0f)
            {
                var progress = 1f - _jumpProgressRemaining;
                var position = Vector3.Lerp(_previousPosition, _nextPosition, progress);

                // Progress between -1 and 1
                var shiftedProgress = 2f * (progress - 0.5f);
                var height = teleportMaxHeight * (1f - shiftedProgress * shiftedProgress);
                
                transform.position = new Vector3(position.x, height, position.z);
                _jumpProgressRemaining -= Time.deltaTime / teleportDuration;
            }
            
            var desiredRotation = GetDesiredRotation();
            
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        
        /// <summary>
        /// Teleports the boss to a random position within the teleportBounds.
        /// </summary>
        private void Teleport()
        {
            _previousPosition = transform.position;
            var randomX = Random.Range(-teleportBounds.x, teleportBounds.x);
            var randomZ = Random.Range(-teleportBounds.y, teleportBounds.y);
            _nextPosition = new Vector3(randomX, transform.position.y, randomZ);
            _jumpProgressRemaining = 1f;
        }

        /// <summary>
        /// Spawns a projectile at the boss's mask position.'
        /// </summary>
        private void Attack()
        {
            IsVulnerable = true;
            StartCoroutine(WaitAndBecomeInvulnerable());

            switch (_activeMask)
            {
                case MaskColor.Red:
                    StartCoroutine(RedMaskAttack());
                    EnsureCooldowns(attackDuration);
                    break;
                
                default:
                    var projectilePool = GetProjectilePool(_activeMask);
                    var projectile = projectilePool.GetProjectile();
                    var maskPosition = _activeMaskTransform.position;
                    projectile.GetComponent<BossProjectile>().Shoot(maskPosition, player, projectilePool);
                    break;
            }
        }

        private IEnumerator RedMaskAttack()
        {
            var duration = 0f;

            while (duration < attackDuration)
            {
                var projectile = redProjectilePool.GetProjectile();
                projectile.GetComponent<BossProjectile>().Shoot(_activeMaskTransform.position, player, redProjectilePool);
                yield return new WaitForSeconds(1f / rateOfFire);
                duration += Time.deltaTime;
            }
        }

        private IEnumerator WaitAndBecomeInvulnerable()
        {
            yield return new WaitForSeconds(vulnerabilityDuration);
            IsVulnerable = false;
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
            _activeMaskTransform = maskGameObject.transform;
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
        /// Returns the projectile pool corresponding to the given mask.
        /// </summary>
        /// <param name="mask">Mask</param>
        /// <returns>Projectile pool</returns>
        /// <exception cref="ArgumentOutOfRangeException">Mask is not in enum</exception>
        private BossProjectilePool GetProjectilePool(MaskColor mask) =>
            mask switch
            {
                MaskColor.Red => redProjectilePool,
                MaskColor.Black => blackProjectilePool,
                MaskColor.Blue => blueProjectilePool,
                _ => throw new ArgumentOutOfRangeException()
            };

        private Quaternion GetDesiredRotation()
        {
            // Rotate only around Y so the ACTIVE MASK direction matches the player direction.
            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            var toMask = _activeMaskTransform.position - transform.position;
            toMask.y = 0f;

            // Avoid invalid rotations when something is exactly on top/center.
            if (toPlayer.sqrMagnitude <= 0.0001f || toMask.sqrMagnitude <= 0.0001f)
                return Quaternion.identity;

            var playerDir = toPlayer.normalized;
            var maskDir = toMask.normalized;

            // Angle needed to rotate the boss around Y so maskDir aligns with playerDir.
            var yawDelta = Vector3.SignedAngle(maskDir, playerDir, Vector3.up);
            return Quaternion.AngleAxis(yawDelta, Vector3.up) * transform.rotation;
        }
        
        private void EnsureCooldowns(float cooldown = 0f)
        {
            if (cooldown == 0f)
            {
                cooldown = minimumCooldownAfterAction;
            }
            
            if (_teleportTimer < cooldown)
            {
                ResetTeleportTimer();
            }
            
            if (_attackTimer < cooldown)
            {
                ResetAttackTimer();
            }
            
            if (_maskSwitchTimer < cooldown)
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
