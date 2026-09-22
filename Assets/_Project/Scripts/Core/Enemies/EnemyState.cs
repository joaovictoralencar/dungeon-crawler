namespace DungeonCrawler.Core.Enemies
{
    public interface IEnemyState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    public sealed class EnemyStateMachine
    {
        public IEnemyState CurrentState { get; private set; }

        public void ChangeState(IEnemyState nextState)
        {
            if (ReferenceEquals(CurrentState, nextState))
                return;

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState?.Enter();
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }

        public void Stop()
        {
            CurrentState?.Exit();
            CurrentState = null;
        }
    }
}
