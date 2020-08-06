using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace QwasiMath.Expressions
{
    public delegate TExpression MultitermOpFunction<TExpression, TTerms>(IEnumerable<TTerms> terms)
        where TExpression : class, IMathExpression<TExpression>
        where TTerms : class, IMathExpression<TTerms>;

    public interface IMultitermOperation<TExpression, TTerms> : IOperationExpression<TExpression>, IEnumerable<TTerms>
        where TExpression : class, IMathExpression<TExpression>
        where TTerms : class, IMathExpression<TTerms>
    {
        IEnumerable<TTerms> Terms { get; }

        IEnumerator<TTerms> IEnumerable<TTerms>.GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class AbstractMultitermOperation<TExpression, TTerms> : IMultitermOperation<TExpression, TTerms>
        where TExpression : class, IMathExpression<TExpression>
        where TTerms : class, IMathExpression<TTerms>
    {
        public IEnumerable<TTerms> Terms { get; }

        public string OpName { get; }
        public string OpSymbol { get; }

        protected MultitermOpFunction<TExpression, TTerms> OpFunction { get; }

        public TExpression Evaluate(MathContext context)
        {
            TTerms[] evalTerms = this.Terms.Select(t => t.Evaluate(context)).ToArray();

            /*if (evalTerms.SequenceEqual(this.Terms))
                return this.AsExpressionType();*/

            return this.OpFunction(evalTerms);
        }

        public TExpression Substitute(MathContext context)
        {
            TTerms[] evalTerms = this.Terms.Select(t => t.Substitute(context)).ToArray();

            if (evalTerms.SequenceEqual(this.Terms))
                return this.AsExpressionType();

            return this.OpFunction(evalTerms);
        }

        public override string ToString()
        {
            string terms;
            if (this.OpSymbol == null || this.OpSymbol == "")
            {
                terms = this.Terms.Aggregate("", (acc, t) => acc + (acc == "" ? "" : ", ") + t.ToString());
                return this.OpName + "[" + terms + "]";
            }

            terms = this.Terms.Aggregate("", (acc, t) => acc + (acc == "" ? "" : this.OpSymbol + " ") + t.ToString());
            return "(" + terms + ")";
        }

        protected AbstractMultitermOperation(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<TExpression, TTerms> opFunction,
            string opName,
            string opSymbol)
        {
            this.Terms = terms.ToArray();
            this.OpFunction = opFunction;
            this.OpName = opName;
            this.OpSymbol = opSymbol;
        }
    }
}
