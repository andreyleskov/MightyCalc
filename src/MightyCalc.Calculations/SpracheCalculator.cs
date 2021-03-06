using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sprache;
using Sprache.Calc;

namespace MightyCalc.Calculations
{
    public class SpracheCalculator : ICalculator
    {
        readonly XtensibleCalculator _calculator = new XtensibleCalculator();
        private readonly List<FunctionDefinition> _knownFunctions = new List<FunctionDefinition>();
        public SpracheCalculator()
        { 
            AddFunction("add","Addition","a+b", "a", "b");
            AddFunction("sub","Substraction","a-b", "a", "b");
            AddFunction("mul","Multiply","a*b", "a", "b");
            AddFunction("div","Divide","a/b", "a", "b");
            
            AddFunction("sqrt","Square Root",d => Math.Sqrt(d));
            AddFunction("cuberoot","Cube Root",d => Math.Pow(d, 1 / 3.0));
            AddFunction("fact","Factorial",d => Enumerable.Range(1, (int) d).Aggregate(1, (elem, fact) => fact * elem));

        }

        public CalculationResult Calculate(string expression, params Parameter[] parameters)
        {
            var dict = new Dictionary<string, double>();
            foreach (var parameter in parameters)
            {
                dict[parameter.Name] = parameter.Value;
            }

            var exp = _calculator.ParseExpression(expression, dict);
            var value = exp.Compile().Invoke();
            return new CalculationResult(value, ExtractCalledFunctions(exp).ToArray());
        }

        public class FunctionCallVisitor : ExpressionVisitor
        {
            public FunctionCallVisitor(string[] knownFunctions)
            {
                _knownFunctions = knownFunctions;
            }

            public List<string> FunctionNames { get; } = new List<string>();
            private readonly string[] _knownFunctions;

            public override Expression Visit(Expression expr)
            {
                switch (expr)
                {
                    case BinaryExpression e:
                    {
                        FunctionNames.Add(e.NodeType.ToString());
                        break;
                    }
                    case UnaryExpression e:
                    {
                        FunctionNames.Add(e.NodeType.ToString());
                        break;
                    }
                    case ConstantExpression e:
                        if(e.Value is string value)
                        {
                            var customFunction = value.Split(':')[0];
                            if(_knownFunctions.Contains(customFunction))
                                FunctionNames.Add(customFunction);
                        }
                        break;
                    case MethodCallExpression e:
                    {
                        if(e.Method.DeclaringType == typeof(System.Math))
                                FunctionNames.Add(e.Method.Name);
                        break;
                    }
                    default:
                    {
                        var a = expr;
                        break;
                    }
                }
                return base.Visit(expr);
            }
        }
        
        private IEnumerable<string> ExtractCalledFunctions(Expression expression)
        {
            var visitor = new FunctionCallVisitor(_knownFunctions.Select(f => f.Name).ToArray());

            visitor.Visit(expression);
            
            return visitor.FunctionNames;
        }

        public bool CanAddFunction(string expression, params string[] parameterNames)
        {
            _calculator.ParseExpression(expression, parameterNames.ToDictionary(p => p, p => 0d));
            return true;
        }
        
        public void AddFunction(string name, string description, string expression, params string[] parameterNames)
        {
            _calculator.RegisterFunction(name, expression, parameterNames.ToArray());
            
            try
            {
                Calculate(expression, parameterNames.Select(p => new Parameter(p, 0)).ToArray());

                var existingFunc =
                    _knownFunctions.FirstOrDefault(f => f.Name == name && f.Arity == parameterNames.Length);
                if (existingFunc != null)
                    _knownFunctions.Remove(existingFunc);
                        
                _knownFunctions.Add(new FunctionDefinition(name,parameterNames.Count(),description, expression));


            }
            catch (Exception ex)
            {
                throw new AddFunctionException(ex);
            }
        }

        
        public class AddFunctionException : Exception
        {
            public AddFunctionException(Exception exception):base("Error during add a new function",exception)
            {
                
            }
        }

        private void AddFunction(string name, string description, Func<double,double> expression)
        {
            _knownFunctions.Add(new FunctionDefinition(name,1,description,""));
            _calculator.RegisterFunction(name, expression);
        }
        private void AddFunction(string name, string description, Func<double,double,double> expression)
        {
            _knownFunctions.Add(new FunctionDefinition(name,2,description,""));
            _calculator.RegisterFunction(name, expression);
        }

        public IReadOnlyList<FunctionDefinition> GetKnownFunctions()
        {
            return _knownFunctions;
        }
    }
}