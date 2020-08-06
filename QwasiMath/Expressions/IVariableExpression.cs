using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace QwasiMath.Expressions
{
    public interface IVariableExpression<TExpression> : IMathExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
    {
        object Identifier { get; }
    }

    public abstract class VariableExpression<TExpression> : IVariableExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
    {
        public object Identifier { get; private set; }

        public TExpression Evaluate(MathContext context)
            => (context[this.Identifier] is TExpression e) ? e.Evaluate(context) : this.AsExpressionType();

        public TExpression Substitute(MathContext context)
            => (context[this.Identifier] is TExpression e) ? e : this.AsExpressionType();

        public override string ToString()
        {
            if (this.Identifier is IEnumerable<object> idList)
                return "[" + idList.Aggregate("", (acc, obj) => acc + (acc == "" ? "" : ":") + obj.ToString()) + "]";

            return this.Identifier.ToString();
        }

        protected VariableExpression(object identifier) => this.Identifier = identifier;
    }
}
