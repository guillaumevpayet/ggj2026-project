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
        [SerializeField] private float delayAtStart;
        [SerializeField] private float minimumCooldownAfterAction;
        [SerializeField] private float maximumCooldownAfterAction;
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
        [SerializeField] private float shockwaveCount;
        [SerializeField] private float spiralRateOfFire;
        [SerializeField] private float spiralRotationSpeed;
        [SerializeField] private GameObject spiralContainerPrefab;

        [Header("Mask Switching")]
        [SerializeField] private float maskSwitchFrequency;
        [SerializeField] private float maskSwitchingSpeed;
        [SerializeField] private float rotationSpeed;

        // -------------------

        private MaskColor _activeMask;
        private readonly HashSet<MaskColor> _pillarDown = new();
        private int _masksLeft = Enum.GetValues(typeof(MaskColor)).Length;
        private Transform _activeMaskTransform;


        /// <summary>
        /// Whether the boss is currently vulnerable to damage.
        /// </summary>
        public bool IsVulnerable { get; private set; }


        /// <summary>
        /// Applies damage to the boss based on the specified mask color.
        /// Handles pillar damage, the player's progression in the fight,
        /// and executes any necessary state transitions when a pillar is destroyed.
        /// </summary>
        /// <param name="mask">The color of the mask corresponding to the pillar being damaged.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided mask color is invalid.</exception>
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
            StartCoroutine(WaitAndDoSomething(delayAtStart));
        }

        private void Update()
        {
            HandleLerps();
        }


        /// <summary>
        /// Waits for a random duration before executing one of three possible actions: teleport, attack, or switch the active mask.
        /// </summary>
        /// <returns>An enumerator used for coroutine control.</returns>
        private IEnumerator WaitAndDoSomething(float waitDuration = 0f)
        {
            if (waitDuration == 0f)
            {
                waitDuration = Random.Range(minimumCooldownAfterAction, maximumCooldownAfterAction);
            }

            Debug.Log($"Bob waits for {waitDuration} seconds.");
            yield return new WaitForSeconds(waitDuration);

            var range = teleportFrequency + attackFrequency + maskSwitchFrequency;
            var action = Random.Range(0f, range);

            if (action <= teleportFrequency)
            {
                Teleport();
            }
            else if (action <= teleportFrequency + attackFrequency)
            {
                Attack();
            }
            else
            {
                SwitchMask();
            }
        }

        /// <summary>
        /// Lerps the boss's rotation towards the desired rotation.'
        /// </summary>
        private void HandleLerps()
        {
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
            Debug.Log("Bob teleports!");
            StartCoroutine(WaitAndDoSomething());
            var randomX = Random.Range(-teleportBounds.x, teleportBounds.x);
            var randomZ = Random.Range(-teleportBounds.y, teleportBounds.y);
            var nextPosition = new Vector3(randomX, transform.position.y, randomZ);
            StartCoroutine(Jump(nextPosition));
        }

        private IEnumerator Jump(Vector3 nextPosition)
        {
            var progress = 0f;
            var originalPosition = transform.position;
            
            while (progress < 1f)
            {
                var position = Vector3.Lerp(originalPosition, nextPosition, progress);

                // Progress between -1 and 1
                var shiftedProgress = 2f * (progress - 0.5f);
                var height = teleportMaxHeight * (1f - shiftedProgress * shiftedProgress);

                transform.position = new Vector3(position.x, height, position.z);
                progress += Time.deltaTime / teleportDuration;
                yield return null;
            }
            
            transform.position = nextPosition;
        }

        /// <summary>
        /// Spawns a projectile at the boss's mask position.'
        /// </summary>
        private void Attack()
        {
            Debug.Log("Bob attacks!");
            IsVulnerable = true;
            StartCoroutine(WaitAndBecomeInvulnerable());

            switch (_activeMask)
            {
                case MaskColor.Red:
                    StartCoroutine(MachineGunAttack());
                    break;
                
                case MaskColor.Black:
                    StartCoroutine(ShockwaveAttack());
                    break;

                case MaskColor.Blue:
                    StartCoroutine(SpiralAttack());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator MachineGunAttack()
        {
            var timeLeft = attackDuration;

            while (timeLeft > 0f)
            {
                var projectile = redProjectilePool.GetProjectile();
                var maskPosition = _activeMaskTransform.position;
                projectile.GetComponent<BossProjectile>().Shoot(maskPosition, player, redProjectilePool);
                yield return new WaitForSeconds(1f / rateOfFire);
                timeLeft -= 1f / rateOfFire;
            }
        }

        private IEnumerator ShockwaveAttack()
        {
            var timeLeft = attackDuration;
            var timeDelta = attackDuration / shockwaveCount;

            while (timeLeft > 0f)
            {
                var projectile = blackProjectilePool.GetProjectile();
                projectile.GetComponent<BossProjectile>().Shoot(transform.position, player, blackProjectilePool);
                yield return new WaitForSeconds(timeDelta);
                timeLeft -= timeDelta;
            }
        }

        private IEnumerator SpiralAttack()
        {
            var timeLeft = attackDuration;
            var timeDelta = 1f / spiralRateOfFire;
            var maskPosition = _activeMaskTransform.position;
            var spiralContainer = Instantiate(spiralContainerPrefab, maskPosition, Quaternion.identity);
            spiralContainer.transform.position = maskPosition;

            while (timeLeft > 0f)
            {
                var referenceAngle = spiralRotationSpeed * timeLeft / attackDuration;
                var baseDirection = Quaternion.AngleAxis(referenceAngle, Vector3.up) * Vector3.forward;

                for (var i = 0; i < 4; i++)
                {
                    var direction = Quaternion.AngleAxis(i * 90, Vector3.up) * baseDirection;
                    var projectile = blueProjectilePool.GetProjectile();
                    projectile.transform.SetParent(spiralContainer.transform);
                    projectile.GetComponent<BossProjectile>().Shoot(Vector3.zero, player, blueProjectilePool, direction);
                }

                yield return new WaitForSeconds(timeDelta);
                timeLeft -= timeDelta;
            }
        }

        private IEnumerator WaitAndBecomeInvulnerable()
        {
            yield return new WaitForSeconds(vulnerabilityDuration);
            IsVulnerable = false;
            StartCoroutine(WaitAndDoSomething());
        }

        /// <summary>
        /// Switches the active mask to a random one from the enum.
        /// </summary>
        private void SwitchMask()
        {
            Debug.Log("Bob switches masks!");
            StartCoroutine(WaitAndDoSomething());
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
    }
}
