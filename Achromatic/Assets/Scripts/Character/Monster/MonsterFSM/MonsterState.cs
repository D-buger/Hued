public abstract class MonsterState
{
    protected Monster monster;
    public MonsterState(Monster monster)
    {
        this.monster = monster;
    }
    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}