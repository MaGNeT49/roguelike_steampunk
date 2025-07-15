namespace RoguelikeSteampunk.StateMachine
{
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context,
            playerStateFactory)
        {
            IsRootState = true;
            InitializeSubState();
        }

        public override void EnterState()
        {
            Ctx.CurrentMovementY = Ctx.GroundedGravity;
            Ctx.AppliedMovementY = Ctx.GroundedGravity;
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void ExitState()
        {
        }

        public override void CheckSwitchStates()
        {
            if (Ctx.IsJumpPressed && !Ctx.RequireNewJumpPress)
            {
                SwitchState(Factory.Jump());
            }
        }

        public override void InitializeSubState()
        {
            if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SetSubState(Factory.Idle());
            }
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SetSubState(Factory.Walk());
            }
            else
            {
                SetSubState(Factory.Run());
            }
        }
    }
}