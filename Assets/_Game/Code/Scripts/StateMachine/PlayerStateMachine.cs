using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoguelikeSteampunk.StateMachine
{
    public class PlayerStateMachine : MonoBehaviour
    {
        private CharacterController _characterController;
        private Animator _animator;
        private PlayerInput _playerInput;

        private int _isWalkingHash;
        private int _isRunningHash;

        private Vector2 _currentMovementInput;
        private Vector3 _currentMovement;
        private Vector3 _appliedMovement;
        private bool _isMovementPressed;
        private bool _isRunPressed;

        private float _rotationFactorPerFrame = 15.0f;
        private float _runMultiplier = 4.0f;

        private float _gravity = -9.8f;
        private float _groundedGravity = -0.05f;

        private bool _isJumpPressed;
        private float _initialJumpVelocity;
        private float _maxJumpHeight = 4.0f;
        private float _maxJumpTime = 0.75f;
        private bool _isJumping;
        private int _isJumpingHash;
        private int _jumpCountHash;
        private bool _requireNewJumpPress;
        private int _jumpCount;
        private Dictionary<int, float> _initialJumpVelocities = new();
        private Dictionary<int, float> _jumpGravities = new();
        private Coroutine _currentJumpResetRoutine;

        private PlayerBaseState _currentState;
        private PlayerStateFactory _states;

        public PlayerBaseState CurrentState
        {
            get => _currentState;
            set => _currentState = value;
        }

        public Animator Animator => _animator;

        public Coroutine CurrentJumpResetRoutine
        {
            get => _currentJumpResetRoutine;
            set => _currentJumpResetRoutine = value;
        }

        public Dictionary<int, float> InitialJumpVelocities => _initialJumpVelocities;

        public int JumpCount
        {
            get => _jumpCount;
            set => _jumpCount = value;
        }

        public int IsJumpingHash => _isJumpingHash;

        public int JumpCountHash => _jumpCountHash;

        public bool RequireNewJumpPress
        {
            get => _requireNewJumpPress;
            set => _requireNewJumpPress = value;
        }

        public bool IsJumping
        {
            set => _isJumping = value;
        }

        public bool IsJumpPressed => _isJumpPressed;

        public float CurrentMovementY
        {
            get => _currentMovement.y;
            set => _currentMovement.y = value;
        }

        public float AppliedMovementY
        {
            get => _appliedMovement.y;
            set => _appliedMovement.y = value;
        }

        public CharacterController CharacterController => _characterController;
        public float GroundedGravity => _groundedGravity;
        public Dictionary<int, float> JumpGravities => _jumpGravities;
        public bool IsMovementPressed => _isMovementPressed;
        public bool IsRunPressed => _isRunPressed;
        public int IsWalkingHash => _isWalkingHash;
        public int IsRunningHash => _isRunningHash;

        public float AppliedMovementX
        {
            get => _appliedMovement.x;
            set => _appliedMovement.x = value;
        }

        public float AppliedMovementZ
        {
            get => _appliedMovement.z;
            set => _appliedMovement.z = value;
        }

        public Vector2 CurrentMovementInput => _currentMovementInput;
        public float RunMultiplier => _runMultiplier;

        private void Awake()
        {
            _playerInput = new PlayerInput();
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            _states = new PlayerStateFactory(this);
            _currentState = _states.Grounded();
            _currentState.EnterState();

            _isWalkingHash = Animator.StringToHash("isWalking");
            _isRunningHash = Animator.StringToHash("isRunning");
            _isJumpingHash = Animator.StringToHash("isJumping");
            _jumpCountHash = Animator.StringToHash("jumpCount");

            SetupJumpVariables();
        }

        private void Update()
        {
            HandleRotation();
            _currentState.UpdateStates();
            _characterController.Move(_appliedMovement * Time.deltaTime);
        }

        private void SetupJumpVariables()
        {
            var timeToApex = _maxJumpTime / 2;
            _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
            _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
            var secondJumpGravity = (-2 * (_maxJumpHeight + 2)) / Mathf.Pow(timeToApex * 1.25f, 2);
            var secondJumpInitialVelocity = (2 * (_maxJumpHeight + 2)) / (timeToApex * 1.25f);
            var thirdJumpGravity = (-2 * (_maxJumpHeight + 4)) / Mathf.Pow(timeToApex * 1.5f, 2);
            var thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 4)) / (timeToApex * 1.5f);

            _initialJumpVelocities.Add(1, _initialJumpVelocity);
            _initialJumpVelocities.Add(2, secondJumpInitialVelocity);
            _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

            _jumpGravities.Add(0, _gravity);
            _jumpGravities.Add(1, _gravity);
            _jumpGravities.Add(2, secondJumpGravity);
            _jumpGravities.Add(3, thirdJumpGravity);
        }

        private void HandleRotation()
        {
            var positionToLookAt = new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y);

            var currentRotation = transform.rotation;

            if (_isMovementPressed)
            {
                var targetRotation = Quaternion.LookRotation(positionToLookAt);

                transform.rotation = Quaternion.Slerp(currentRotation, targetRotation,
                    _rotationFactorPerFrame * Time.deltaTime);
            }
        }

        private void OnMovementInput(InputAction.CallbackContext context)
        {
            _currentMovementInput = context.ReadValue<Vector2>();
            _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            _isJumpPressed = context.ReadValueAsButton();
            _requireNewJumpPress = false;
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            _isRunPressed = context.ReadValueAsButton();
        }

        private void OnEnable()
        {
            _playerInput.Game.Enable();

            _playerInput.Game.Move.started += OnMovementInput;
            _playerInput.Game.Move.canceled += OnMovementInput;
            _playerInput.Game.Move.performed += OnMovementInput;

            _playerInput.Game.Run.started += OnRun;
            _playerInput.Game.Run.canceled += OnRun;

            _playerInput.Game.Jump.started += OnJump;
            _playerInput.Game.Jump.canceled += OnJump;
        }

        private void OnDisable()
        {
            _playerInput.Game.Disable();

            _playerInput.Game.Move.started -= OnMovementInput;
            _playerInput.Game.Move.canceled -= OnMovementInput;
            _playerInput.Game.Move.performed -= OnMovementInput;

            _playerInput.Game.Run.started -= OnRun;
            _playerInput.Game.Run.canceled -= OnRun;

            _playerInput.Game.Jump.started -= OnJump;
            _playerInput.Game.Jump.canceled -= OnJump;
        }
    }
}