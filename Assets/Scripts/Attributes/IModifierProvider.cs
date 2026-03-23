using System.Collections.Generic;

namespace Liberator.Attributes
{
    public interface IModifierProvider
    {
        IEnumerable<float> GetAdditiveModifiers(Attribute attribute);
        IEnumerable<float> GetPercentageModifiers(Attribute attribute);
    }
}
