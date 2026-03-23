using Liberator.TurnManager;

namespace Liberator.Combat.Controllers
{
    public enum EnemyActionType
    {
        Attack,
        Heal,
        Stunned
    }

    public interface IEnemyAction
    {
        void DoAction(object sender, EnemyTurnArgs e);
    }
}
