using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Boss
{
    public class BossController : MonoBehaviour
    {
        private static readonly int AttackingMaterialId = Shader.PropertyToID("_Attacking");

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
        [SerializeField] private Renderer bossRenderer;
        [SerializeField] private Material white;

        [Header("Teleportation")]
        [SerializeField] private Vector2 teleportBounds;
        [SerializeField] private float teleportFrequency;
        [SerializeField] private float teleportDuration;
        [SerializeField] private float teleportMaxHeight;
        [SerializeField] private float groundHeight;

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
        private bool _isVulnerable;
        private Material _bossMaterial;


        /// <summary>
        /// Applies damage to the boss based on the active mask color.
        /// Handles pillar damage, the player's progression in the fight,
        /// and executes any necessary state transitions when a pillar is destroyed.
        /// </summary>
        public void TakeDamage()
        {
            if (!_isVulnerable)
            {
                return;
            }

            _isVulnerable = false;
            _bossMaterial.SetFloat(AttackingMaterialId, 0f);
            StopAllCoroutines();
            StartCoroutine(Blink());

            if (_masksLeft == 0)
            {
                // TODO Player wins!
                // Debug.Log("Player wins!");
                return;
            }

            var pillar = _activeMask switch
            {
                MaskColor.Red => redPillarHealth,
                MaskColor.Black => blackPillarHealth,
                MaskColor.Blue => bluePillarHealth,
                _ => throw new ArgumentOutOfRangeException()
            };

            var isPillarDown = pillar.TakeDamage();
            // Debug.Log("Bob: ouch!");

            if (!isPillarDown)
            {
                StartCoroutine(WaitAndDoSomething());
                return;
            }

            _pillarDown.Add(_activeMask);
            _masksLeft -= 1;
            // Debug.Log($"Bob lost the {_activeMask} mask.");

            if (_masksLeft > 0)
            {
                SwitchMask();
            }
            else
            {
                SceneManager.LoadScene("VictoryScreen");
            }
        }


        private void Awake()
        {
            _bossMaterial = bossRenderer.material;
        }

        private void Start()
        {
            _bossMaterial.SetFloat(AttackingMaterialId, 0f);
            SetMaskActive(_activeMask);
            StartCoroutine(WaitAndDoSomething(delayAtStart));
        }

        private void Update()
        {
            HandleLerps();
        }

        private void OnDisable()
        {
            _bossMaterial.SetFloat(AttackingMaterialId, 0f);
        }


        private IEnumerator Blink()
        {
            for (var i = 0; i < 4; i++)
            {
                bossRenderer.material = white;
                yield return new WaitForSeconds(0.1f);
                bossRenderer.material = _bossMaterial;
                yield return new WaitForSeconds(0.1f);
            }
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

            yield return new WaitForSeconds(waitDuration);

            var includeSwitchingMasks = _masksLeft > 1;
            var includeAttacking = _masksLeft > 0;

            var range = teleportFrequency
                        + (includeAttacking ? attackFrequency : 0f)
                        + (includeSwitchingMasks ? maskSwitchFrequency : 0f);

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
            // Debug.Log("Bob teleports!");
            StartCoroutine(WaitAndDoSomething());
            var randomX = Random.Range(-teleportBounds.x, teleportBounds.x);
            var randomZ = Random.Range(-teleportBounds.y, teleportBounds.y);
            var nextPosition = new Vector3(randomX, groundHeight, randomZ);
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
            StartCoroutine(WaitAndDoSomething());
        }

        /// <summary>
        /// Spawns a projectile at the boss's mask position.'
        /// </summary>
        private void Attack()
        {
            _isVulnerable = true;
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
            const float duration = 0.2f;
            var timer = duration;

            while (timer > 0f)
            {
                var attacking = 1f - timer / duration;
                _bossMaterial.SetFloat(AttackingMaterialId, attacking);
                timer -= Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(vulnerabilityDuration);
            _isVulnerable = false;
            timer = duration;

            while (timer > 0f)
            {
                var attacking = timer / duration;
                _bossMaterial.SetFloat(AttackingMaterialId, attacking);
                timer -= Time.deltaTime;
                yield return null;
            }

            _bossMaterial.SetFloat(AttackingMaterialId, 0f);
            StartCoroutine(WaitAndDoSomething());
        }

        /// <summary>
        /// Switches the active mask to a random one from the enum.
        /// </summary>
        private void SwitchMask()
        {
            // Debug.Log("Bob switches masks!");
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
