using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralMath.Expressions.HilbertSpace
{
    public interface IHSExplicitVectorVariable<TBasis> : IHSExplicitVector<TBasis>, IVariableExpression<IHSExplicitVector<TBasis>>
    {
    }

    public class HSExplicitVectorVariable<TBasis> : VariableExpression<IHSExplicitVector<TBasis>>, IHSExplicitVectorVariable<TBasis>
    {
        public HSExplicitVectorVariable(object identifier)
            : base(identifier)
        { }
    }
}
