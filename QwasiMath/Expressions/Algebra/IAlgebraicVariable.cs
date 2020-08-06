using System;
using System.Collections.Generic;
using System.Text;

namespace QwasiMath.Expressions
{
    public interface IAlgebraicVariable : IAlgebraicExpression, IVariableExpression<IAlgebraicExpression>
    {
    }

    public class AlgebraicVariable : VariableExpression<IAlgebraicExpression>, IAlgebraicVariable
    {
        public AlgebraicVariable(object identifier)
            : base(identifier)
        { }
    }
}
