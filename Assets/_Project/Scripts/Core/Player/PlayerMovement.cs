using System;
using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float InitialSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        private CharacterController _characterController;

        [Header("Dodge")] [SerializeField] private float dodgeDistance = 3f;
        [SerializeField] private float dodgeDuration = 0.25f;
        [SerializeField] GameObject dodgeEffect;
        private PlayerInputController _playerInputController;

        /// <summary>Master lock: blocks walking AND dodging.</summary>
        public bool CanMove = true;

        /// <summary>Blocks walking/rotation only. Dodging is still allowed (used while attacking).</summary>
        public bool CanWalk = true;

        public bool IsDodging => _isDodging;
        public event Action DodgeStarted;
        public event Action DodgeFinished;
        public event Action<float> MovementSpeedChanged;

        private Vector3 desiredMoveDirection;
        private bool _isDodging;
        private Vector3 _dodgeDirection;
        private float _dodgeTimer;
        private float _lastMovementSpeed;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerInputController = GetComponent<PlayerInputController>();
        }

        private void OnEnable()
        {
            _playerInputController.MoveRequested += OnInputMoved;
            _playerInputController.DodgeRequested += OnInputDodged;
        }

        private void OnDisable()
        {
            _playerInputController.MoveRequested -= OnInputMoved;
            _playerInputController.DodgeRequested -= OnInputDodged;
        }

        private void OnInputMoved(Vector2 direction)
        {
            desiredMoveDirection = new Vector3(direction.x, 0, direction.y);
        }

        private void OnInputDodged()
        {
            if (desiredMoveDirection.sqrMagnitude <= 0.01f)
            {
                Dodge(transform.forward);
            }
            else
                Dodge(desiredMoveDirection);
        }

        private void Dodge(Vector3 direction)
        {
            if (!CanMove || _isDodging)
                return;

            _dodgeDirection = direction;

            if (_dodgeDirection.sqrMagnitude <= 0.01f)
                _dodgeDirection = transform.forward;

            _dodgeDirection.Normalize();

            _isDodging = true;
            _dodgeTimer = 0f;
            
            dodgeEffect.SetActive(true);
            // dodgeEffect.transform.parent = null;
            // dodgeEffect.transform.position = transform.position + new Vector3(0, 0.5f, 1);
            // dodgeEffect.transform.forward = transform.forward;
            DodgeStarted?.Invoke();
        }

        public void Move(Vector3 dir)
        {
            if (dir.sqrMagnitude <= 0.01f)
                return;

            dir.y = 0f;
            dir.Normalize();

            _characterController.Move(dir * InitialSpeed * Time.deltaTime);
        }

        private void Update()
        {
            UpdateMovementSpeed();

            if (!CanMove)
                return;

            if (_isDodging)
            {
                UpdateDodge();
                return;
            }

            if (!CanWalk)
                return;

            Move(desiredMoveDirection);
            RotateToSpeed(desiredMoveDirection);
        }

        public void LockWalking()
        {
            CanWalk = false;
        }

        public void UnlockWalking()
        {
            CanWalk = true;
        }

        private void UpdateMovementSpeed()
        {
            float speed = CanMove && CanWalk
                ? Mathf.Clamp01(desiredMoveDirection.magnitude)
                : 0f;

            if (Mathf.Approximately(speed, _lastMovementSpeed))
                return;

            _lastMovementSpeed = speed;
            MovementSpeedChanged?.Invoke(speed);
        }

        private void UpdateDodge()
        {
            _dodgeTimer += Time.deltaTime;

            float dodgeSpeed = dodgeDistance / dodgeDuration;
            _characterController.Move(_dodgeDirection * dodgeSpeed * Time.deltaTime);

            RotateToDirection(_dodgeDirection);

            if (_dodgeTimer >= dodgeDuration)
            {
                _isDodging = false;
                _dodgeTimer = 0f;
                DodgeFinished?.Invoke();
                dodgeEffect.SetActive(false);
            }
        }

        private void RotateToSpeed(Vector3 dir)
        {
            if (dir.sqrMagnitude <= 0.01f)
                return;

            Vector3 direction = new Vector3(dir.x, 0f, dir.z);
            RotateToDirection(direction);
        }

        private void RotateToDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.01f)
                return;

            direction.y = 0f;
            direction.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}