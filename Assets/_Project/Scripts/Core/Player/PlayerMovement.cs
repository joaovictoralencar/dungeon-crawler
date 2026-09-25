using System;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonCrawler
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float InitialSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private CharacterController _characterController;

        [Header("Dodge")] [SerializeField] private float dodgeDistance = 3f;
        [SerializeField] private float dodgeDuration = 0.25f;
        [SerializeField] GameObject dodgeEffect;

        [Header("Input")] [SerializeField] private PlayerInputController playerInputController;

        /// <summary>Master lock: blocks walking AND dodging.</summary>
        public bool CanMove = true;

        /// <summary>Blocks walking/rotation only. Dodging is still allowed (used while attacking).</summary>
        public bool CanWalk = true;

        public UnityEvent Dodged { get; } = new();
        public UnityEvent DodgeEnded { get; } = new();
        public bool IsDodging => _isDodging;

        private Vector3 desiredMoveDirection;
        private bool _isDodging;
        private Vector3 _dodgeDirection;
        private float _dodgeTimer;

        private void OnEnable()
        {
            playerInputController.InputMoved.AddListener(OnInputMoved);
            playerInputController.InputDodged.AddListener(OnInputDodged);
        }

        private void OnDisable()
        {
            playerInputController.InputMoved.RemoveListener(OnInputMoved);
            playerInputController.InputDodged.RemoveListener(OnInputDodged);
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
            Dodged.Invoke();
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
                DodgeEnded.Invoke();
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