using System.Collections;
using UnityEngine;

namespace RoguelikeSteampunk.StateMachine
{
    public class PlayerJumpState : PlayerBaseState, IRootState
    {
        public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context,
            playerStateFactory)
        {
            IsRootState = true;
        }

        private IEnumerator IJumpResetRoutine()
        {
            yield return new WaitForSeconds(0.5f);

            Ctx.JumpCount = 0;
        }

        public override void EnterState()
        {
            InitializeSubState();
            HandleJump();
        }

        public override void UpdateState()
        {
            HandleGravity();
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
            
            if (Ctx.IsJumpPressed)
                Ctx.RequireNewJumpPress = true;
            
            Ctx.CurrentJumpResetRoutine = Ctx.StartCoroutine(IJumpResetRoutine());
            
            if (Ctx.JumpCount == 3)
            {
                Ctx.JumpCount = 0;
                Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
            }
        }

        public override void CheckSwitchStates()
        {
            if (Ctx.CharacterController.isGrounded)
                SwitchState(Factory.Grounded());
        }

        public override void InitializeSubState()
        {
            if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed)
                SetSubState(Factory.Idle());
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
                SetSubState(Factory.Walk());
            else
                SetSubState(Factory.Run());
        }

        private void HandleJump()
        {
            if (Ctx.JumpCount < 3 && Ctx.CurrentJumpResetRoutine != null)
                Ctx.StopCoroutine(Ctx.CurrentJumpResetRoutine);

            Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
            Ctx.IsJumping = true;
            Ctx.JumpCount += 1;
            Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
            Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
            Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
        }

        public void HandleGravity()
        {
            bool isFalling = Ctx.CurrentMovementY <= 0.0f || !Ctx.IsJumpPressed;
            float fallMultiplier = 2.0f;
            float previousYVelocity = Ctx.CurrentMovementY;

            if (isFalling)
            {
                Ctx.CurrentMovementY = Ctx.CurrentMovementY +
                                        (Ctx.JumpGravities[Ctx.JumpCount] * fallMultiplier * Time.deltaTime);
                Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * 0.5f, -20.0f);
            }
            else
            {
                Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * Time.deltaTime);
                Ctx.AppliedMovementY = (previousYVelocity + Ctx.CurrentMovementY) * 0.5f;
            }
        }
    }
}