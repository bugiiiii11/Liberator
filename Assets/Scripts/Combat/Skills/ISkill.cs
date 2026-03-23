using Liberator.Combat.Controllers;

namespace Liberator.Combat.Skills
{
    public interface ISkill
    {
        void DoSpecialThing(EntityController target, EntityController myself);
    }
}
