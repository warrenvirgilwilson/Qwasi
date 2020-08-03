using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneralMath.Expressions
{
    public delegate TExpression BinaryOpFunction<TExpression, TArgA, TArgB>(TArgA a, TArgB b)
        where TExpression : class, IMathExpression<TExpression>
        where TArgA : class, IMathExpression<TArgA>
        where TArgB : class, IMathExpression<TArgB>;

    public interface IBinaryOperation<TExpression, TArgA, TArgB> : IOperationExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
        where TArgA : class, IMathExpression<TArgA>
        where TArgB : class, IMathExpression<TArgB>
    {
        TArgA A { get; }
        TArgB B { get; }
    }
    
    public abstract class AbstractBinaryOperation<TExpression, TArgA, TArgB> : IBinaryOperation<TExpression, TArgA, TArgB>
        where TExpression : class, IMathExpression<TExpression>
        where TArgA : class, IMathExpression<TArgA>
        where TArgB : class, IMathExpression<TArgB>
    {
        public TArgA A { get; }
        public TArgB B { get; }

        public string OpName { get; }
        public string OpSymbol { get; }

        protected BinaryOpFunction<TExpression, TArgA, TArgB> OpFunction { get; }

        public TExpression Evaluate(MathContext context)
        {
            TArgA evalA = this.A.Evaluate(context);
            TArgB evalB = this.B.Evaluate(context);

            if (evalA.Equals(this.A) && evalB.Equals(this.B))
                return this.AsExpressionType();

            return this.OpFunction(evalA, evalB);
        }

        public TExpression Substitute(MathContext context)
        {
            TArgA subA = this.A.Substitute(context);
            TArgB subB = this.B.Substitute(context);

            if (subA.Equals(this.A) && subB.Equals(this.B))
                return this.AsExpressionType();

            return this.OpFunction(subA, subB);
        }

        public override string ToString()
        {
            if (this.OpSymbol == null || this.OpSymbol == "")
                return this.OpName + "[" + this.A.ToString() + ", " + this.B.ToString() + "]";

            return "(" + this.A.ToString() + " " + this.OpSymbol + " " + this.B.ToString() + ")";
        }

        protected AbstractBinaryOperation(
            TArgA a, TArgB b,
            BinaryOpFunction<TExpression, TArgA, TArgB> opFunction,
            string opName, string opSymbol)
        {
            this.A = a;
            this.B = b;
            this.OpFunction = opFunction;
            this.OpName = opName;
            this.OpSymbol = opSymbol;
        }
    }
}
