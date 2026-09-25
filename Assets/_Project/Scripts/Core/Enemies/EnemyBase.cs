using UnityEngine;
using UnityEngine.AI;
using DungeonCrawler.Core.Combat;
using System;

namespace DungeonCrawler.Core.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyDeathHandler))]
    public abstract class EnemyBase : MonoBehaviour, IKnockbackable
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private Transform attackOrigin;
        [Header("Knockback")]
        [SerializeField, Min(0f)] private float defaultKnockbackForce = 4f;
        [SerializeField, Min(0f)] private float defaultKnockbackDuration = 0.15f;
        [SerializeField, Min(0f)] private float knockbackRecoveryDuration = 0.1f;

        protected NavMeshAgent Agent { get; private set; }
        protected EnemyConfig Config => config;
        internal EnemyHealth Health { get; private set; }
        internal Transform Target => target;
        protected Transform AttackOrigin => attackOrigin != null ? attackOrigin : transform;
        internal bool IsAttacking { get; private set; }
        internal float PatrolRadius => config.PatrolRadius;
        internal float PatrolWait => config.PatrolWait;
        internal float AttackAnticipation => AttackAnticipationDuration;
        protected virtual float AttackAnticipationDuration => config.AttackAnticipation;
        internal bool CanAttack => Time.time >= _nextAttackTime && !IsAttacking;
        protected float DistanceToTarget => target == null ? float.PositiveInfinity :
            Vector3.Distance(transform.position, target.position);

        public event Action AttackStarted;
        public event Action AttackFinished;
        public event Action AttackCancelled;

        private EnemyStateMachine _stateMachine;
        private float _attackTimer;
        private float _nextAttackTime;
        private bool _attackHitExecuted;
        private Vector3 _knockbackVelocity;
        private float _knockbackTimer;
        private float _knockbackRecoveryTimer;
        private bool _agentWasStoppedBeforeKnockback;
        private bool _resumeNavigationNextFrame;

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Health = GetComponent<EnemyHealth>();
            _stateMachine = new EnemyStateMachine();

            if (config == null)
                config = ScriptableObject.CreateInstance<EnemyConfig>();

            Agent.speed = config.MovementSpeed;
            Agent.angularSpeed = config.AngularSpeed;
            Agent.acceleration = config.Acceleration;
            Agent.stoppingDistance = config.StoppingDistance;
        }

        protected virtual void Start()
        {
            if (target == null)
            {
                DungeonCrawler.Core.Player.Player player =
                    FindAnyObjectByType<DungeonCrawler.Core.Player.Player>();
                target = player != null ? player.transform : null;
            }

            _stateMachine.ChangeState(new EnemyIdleState(this));
        }

        protected virtual void Update()
        {
            if (_knockbackTimer > 0f || _knockbackRecoveryTimer > 0f)
            {
                if (_knockbackTimer > 0f)
                {
                    _knockbackTimer -= Time.deltaTime;
                    if (Agent.isOnNavMesh)
                    {
                        Vector3 displacement = _knockbackVelocity * Time.deltaTime;
                        Agent.Move(displacement);
                    }
                }

                _knockbackRecoveryTimer -= Time.deltaTime;
                if (_knockbackRecoveryTimer <= 0f && _knockbackTimer <= 0f)
                {
                    Agent.velocity = Vector3.zero;
                    Agent.isStopped = _agentWasStoppedBeforeKnockback;
                    _resumeNavigationNextFrame = true;
                }
                else
                {
                    return;
                }
            }

            if (_resumeNavigationNextFrame)
            {
                _resumeNavigationNextFrame = false;
                return;
            }

            _stateMachine.Tick();
        }

        protected virtual void OnDisable()
        {
            _stateMachine?.Stop();
            IsAttacking = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        internal void ChangeState(IEnemyState state)
        {
            _stateMachine.ChangeState(state);
        }

        public bool HasTarget()
        {
            if (target == null)
                return false;

            IDamageable targetDamageable = target.GetComponentInParent<IDamageable>();
            if (targetDamageable != null && targetDamageable.IsDead)
                return false;

            float range = _stateMachine.CurrentState is EnemyChaseState
                ? config.LoseTargetRange
                : config.DetectionRange;

            if (DistanceToTarget > range)
                return false;

            if (!config.RequireLineOfSight)
                return true;

            Vector3 origin = transform.position + Vector3.up;
            Vector3 destination = target.position + Vector3.up;
            return !Physics.Linecast(origin, destination, config.ObstacleMask);
        }

        public bool IsInAttackRange()
        {
            return target != null && DistanceToTarget <= config.AttackRange;
        }

        public bool ShouldPatrol => config.Patrol;

        public void MoveTo(Vector3 position)
        {
            if (Agent.enabled && Agent.isOnNavMesh)
            {
                Agent.isStopped = false;
                Agent.SetDestination(position);
            }
        }

        public void StopMoving()
        {
            if (Agent.enabled && Agent.isOnNavMesh)
            {
                Agent.ResetPath();
                Agent.isStopped = true;
            }
        }

        public bool HasReachedDestination()
        {
            return Agent.isOnNavMesh && !Agent.pathPending &&
                   Agent.remainingDistance <= Agent.stoppingDistance;
        }

        public void FaceTarget()
        {
            if (target == null)
                return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
                return;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction),
                config.AngularSpeed * Time.deltaTime);
        }

        public void BeginAttack()
        {
            if (!CanAttack)
                return;

            IsAttacking = true;
            _attackHitExecuted = false;
            _nextAttackTime = Time.time + config.AttackInterval;
            StopMoving();
            AttackStarted?.Invoke();
        }

        protected abstract void PerformAttack();

        public void ExecuteAttackAnimationEvent()
        {
            if (!IsAttacking || _attackHitExecuted)
                return;

            _attackHitExecuted = true;
            PerformAttack();
        }

        public void FinishAttackAnimation()
        {
            if (!IsAttacking)
                return;

            EndAttackHitbox();
            IsAttacking = false;
            _attackHitExecuted = false;
            AttackFinished?.Invoke();
        }

        public void CancelAttack()
        {
            if (!IsAttacking)
                return;

            EndAttackHitbox();
            IsAttacking = false;
            _attackHitExecuted = false;
            AttackCancelled?.Invoke();
        }

        protected virtual void EndAttackHitbox() { }

        internal float MovementSpeed01 => config.MovementSpeed <= 0f
            ? 0f
            : Mathf.Clamp01(Agent.velocity.magnitude / config.MovementSpeed);

        public void ApplyDamage(Transform victim, float damage)
        {
            IDamageable damageable = victim == null ? null :
                victim.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage);
        }

        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
                return;

            if (!Agent.enabled || !Agent.isOnNavMesh)
                return;

            _agentWasStoppedBeforeKnockback = Agent.isStopped;
            Agent.ResetPath();
            Agent.isStopped = true;
            _knockbackVelocity = direction.normalized *
                (force > 0f ? force : defaultKnockbackForce);
            _knockbackTimer = duration > 0f ? duration : defaultKnockbackDuration;
            _knockbackRecoveryTimer = knockbackRecoveryDuration;
        }
    }
}
