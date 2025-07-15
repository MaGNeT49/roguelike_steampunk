using UnityEngine;

namespace RoguelikeSteampunk
{
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(Animator))]
    public class AnimationController : MonoBehaviour
    {
        private static readonly int IsWalkingHash = Animator.StringToHash(IsWalking);
        private static readonly int IsRunningHash = Animator.StringToHash(IsRunning);
        
        private const string IsWalking = "isWalking";
        private const string IsRunning = "isRunning";

        private MovementController _movementController;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _movementController = GetComponent<MovementController>();
        }

        private void Update()
        {
            HandleAnimationWalking();
            HandleAnimationRunning();
        }

        private void HandleAnimationWalking()
        {
            bool isWalking = _animator.GetBool(IsWalkingHash);

            if (_movementController.IsMovementPressed && !isWalking)
                _animator.SetBool(IsWalkingHash, true);
            else if (!_movementController.IsMovementPressed && isWalking)
                _animator.SetBool(IsWalkingHash, false);
        }
        
        private void HandleAnimationRunning()
        {
            bool isRunning = _animator.GetBool(IsRunningHash);

            if (_movementController.IsMovementPressed && _movementController.IsRunPressed && !isRunning)
                _animator.SetBool(IsRunningHash, true);
            
            else if ((!_movementController.IsMovementPressed || !_movementController.IsRunPressed) && isRunning)
                _animator.SetBool(IsRunningHash, false);
        }
    }
}