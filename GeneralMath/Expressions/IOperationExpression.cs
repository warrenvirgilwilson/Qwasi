using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralMath.Expressions
{
    public interface IOperationExpression<TExpression> : IMathExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
    {
        string OpName { get; }
        string OpSymbol { get; }
    }
}
