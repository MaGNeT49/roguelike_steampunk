using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoguelikeSteampunk
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Rotation Parameters")] [SerializeField]
        private float _rotationFactorPerFrame = 5f;

        [Header("Run Parameters")] [SerializeField]
        private float _increasedMovement = 3f;

        [Header("Jump Parameters")] [SerializeField]
        private float _maxJumpHeight = 2.0f;

        [SerializeField] private float _maxJumpTime = 0.75f;

        private Vector2 _currentMovementInput;
        private Vector3 _currentMovement;
        private Vector3 _currentRunMovement;
        private Vector3 _appliedMovement;

        private const float GroundedGravity = -0.05f;
        private float _gravityForJump = 9.8f;
        private float _initialJumpVelocity;
        private bool _isJumping;

        private PlayerInput _playerInput;
        private CharacterController _characterController;
        private int _jumpCount;
        private Coroutine _currentJumpResetRoutine;
        private Animator _animator;
        private int _isJumpingHash;
        private bool _isJumpAnimating;
        private int _jumpCountHash;
        private Dictionary<int, float> _initialJumpVelocities;

        public bool IsMovementPressed { get; private set; }

        public bool IsRunPressed { get; private set; }
        public bool IsJumpPressed { get; private set; }

        private void Awake()
        {
            _playerInput = new PlayerInput();

            _characterController = GetComponent<CharacterController>();

            SetupJumpVariables();
        }

        private void Update()
        {
            HandleRotation();

            Move();

            HandleGravity();
            HandleJump();
        }

        private void SetupJumpVariables()
        {
            var timeToApex = _maxJumpTime / 2;

            _gravityForJump = -2 * _maxJumpHeight / Mathf.Pow(timeToApex, 2);
            _initialJumpVelocity = 2 * _maxJumpHeight / timeToApex;
        }

        private void Move()
        {
            var currentMovement = _currentMovement;

            if (IsRunPressed)
            {
                currentMovement = _currentRunMovement;
            }

            _appliedMovement.x = currentMovement.x;
            _appliedMovement.z = currentMovement.z;

            _characterController.Move(_appliedMovement * Time.deltaTime);
        }

        private void HandleJump()
        {
            if (!_isJumping && _characterController.isGrounded && IsJumpPressed)
            {
                if (_jumpCount < 3 && _currentJumpResetRoutine != null)
                {
                    StopCoroutine(_currentJumpResetRoutine);
                }

                _animator.SetBool(_isJumpingHash, true);
                _isJumpAnimating = true;
                _isJumping = true;
                _jumpCount += 1;
                _animator.SetInteger(_jumpCountHash, _jumpCount);
                _currentMovement.y = _initialJumpVelocities[_jumpCount];
                _appliedMovement.y = _initialJumpVelocities[_jumpCount];
            }
            else if (!IsJumpPressed && _isJumping && _characterController.isGrounded)
            {
                _isJumping = false;
            }
        }

        private void HandleGravity()
        {
            if (_characterController.isGrounded)
            {
                if (_isJumpAnimating)
                {
                    _animator.SetBool(_isJumpingHash, false);
                    _isJumpAnimating = false;
                    _currentJumpResetRoutine = StartCoroutine(IJumpResetRoutine());
                    if (_jumpCount == 3)
                    {
                        _jumpCount = 0;
                        _animator.SetInteger(_jumpCountHash, _jumpCount);
                    }
                }
                _currentMovement.y = GroundedGravity;
                _appliedMovement.y = GroundedGravity;
            }
            else
            {
                var previousYVelocity = _currentMovement.y;
                _currentMovement.y = _currentMovement.y + _gravityForJump * Time.deltaTime;
                _appliedMovement.y = (previousYVelocity + _currentMovement.y) * 0.5f;
            }
        }

        private IEnumerator IJumpResetRoutine()
        {
            throw new System.NotImplementedException();
        }

        private void HandleRotation()
        {
            var positionToLookAt = new Vector3(_currentMovement.x, 0, _currentMovement.z);

            var currentRotation = transform.rotation;

            if (IsMovementPressed)
            {
                var targetRotation = Quaternion.LookRotation(positionToLookAt);
                transform.rotation = Quaternion.Slerp(currentRotation, targetRotation,
                    _rotationFactorPerFrame * Time.deltaTime);
            }
        }

        private void OnMovementInput(InputAction.CallbackContext context)
        {
            _currentMovementInput = context.ReadValue<Vector2>();

            _currentMovement =
                new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y);

            _currentRunMovement =
                new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y) *
                _increasedMovement;

            IsMovementPressed = _currentMovementInput != Vector2.zero;
        }


        private void OnJump(InputAction.CallbackContext context)
        {
            IsJumpPressed = context.ReadValueAsButton();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            IsRunPressed = context.ReadValueAsButton();
        }

        private void OnEnable()
        {
            _playerInput.Game.Enable();

            _playerInput.Game.Move.started += OnMovementInput;
            _playerInput.Game.Move.performed += OnMovementInput;
            _playerInput.Game.Move.canceled += OnMovementInput;

            _playerInput.Game.Jump.started += OnJump;
            _playerInput.Game.Jump.canceled += OnJump;

            _playerInput.Game.Run.started += OnRun;
            _playerInput.Game.Run.canceled += OnRun;
        }

        private void OnDisable()
        {
            _playerInput.Game.Disable();

            _playerInput.Game.Move.started -= OnMovementInput;
            _playerInput.Game.Move.performed -= OnMovementInput;
            _playerInput.Game.Move.canceled -= OnMovementInput;

            _playerInput.Game.Jump.started -= OnJump;
            _playerInput.Game.Jump.canceled -= OnJump;

            _playerInput.Game.Run.started -= OnRun;
            _playerInput.Game.Run.canceled -= OnRun;
        }
    }
}