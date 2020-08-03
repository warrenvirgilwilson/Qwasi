using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralMath.Expressions
{
    public delegate TExpression UnaryOpFunction<TExpression, TOperand>(TOperand operand)
        where TExpression : class, IMathExpression<TExpression>
        where TOperand : class, IMathExpression<TOperand>;

    public interface IUnaryOperation<TExpression, TOperand> : IOperationExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
        where TOperand : class, IMathExpression<TOperand>
    {
        TOperand Operand { get; }
    }

    public abstract class AbstractUnaryOperation<TExpression, TOperand> : IUnaryOperation<TExpression, TOperand>
        where TExpression : class, IMathExpression<TExpression>
        where TOperand : class, IMathExpression<TOperand>
    {
        public TOperand Operand { get; }

        public string OpName { get; }
        public string OpSymbol { get; }

        protected UnaryOpFunction<TExpression, TOperand> OpFunction { get; }

        public TExpression Evaluate(MathContext context)
        {
            TOperand eval = this.Operand.Evaluate(context);

            if (eval.Equals(this.Operand))
                return this.AsExpressionType();

            return this.OpFunction(eval);
        }

        public TExpression Substitute(MathContext context)
        {
            TOperand sub = this.Operand.Substitute(context);

            if (sub.Equals(this.Operand))
                return this.AsExpressionType();

            return this.OpFunction(sub);
        }

        public override string ToString()
        {
            if (this.OpSymbol == null || this.OpSymbol == "")
                return this.OpName + "[" + this.Operand.ToString() + "]";

            return this.OpSymbol + this.Operand.ToString();
        }

        protected AbstractUnaryOperation(
            TOperand operand,
            UnaryOpFunction<TExpression, TOperand> opFunction,
            string opName,
            string opSymbol)
        {
            this.Operand = operand;
            this.OpFunction = opFunction;
            this.OpName = opName;
            this.OpSymbol = opSymbol;
        }
    }
}
