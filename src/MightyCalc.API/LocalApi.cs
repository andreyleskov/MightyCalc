using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MightyCalc.Calculations;
using MightyCalc.Reports;

namespace MightyCalc.API
{
    class LocalApi : IApiController
    {
        private readonly ICalculator _calculator;

        public LocalApi(ICalculator calculator)
        {
            _calculator = calculator;
        }
        public Task<double> CalculateAsync(Expression body)
        {
            var parameters = body.Parameters.Select(p => new Calculations.Parameter(p.Name, p.Value)).ToArray();
            var result = _calculator.Calculate(body.Representation, parameters).Value;
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<NamedExpression>> FindFunctionsAsync(string name)
        {
            IReadOnlyCollection<NamedExpression> expressions = 
                _calculator.GetKnownFunctions().Select(f => new NamedExpression()
            {
                Description = f.Description,
                Name = f.Name,
                Expression = new Expression()
                {
                    Representation = f.Expression,
                    Parameters = f.Parameters.Select(p => new Parameter() {Name = p})
                        .ToList()
                }
            }).ToArray();
            
            return Task.FromResult(expressions);
        }

        public Task CreateFunctionAsync(NamedExpression body)
        {
            //API-specific restriction, not coming from business logic! 
            if(_calculator.GetKnownFunctions().Any(f => f.Name == body.Name))
                     throw new FunctionAlreadyExistsException();
                
            _calculator.AddFunction(body.Name, body.Description,body.Expression.Representation,body.Expression.Parameters.Select(p => p.Name).ToArray());
            return Task.CompletedTask;
        }

   
        public Task ReplaceFunctionAsync(NamedExpression body)
        {
            _calculator.AddFunction(body.Name, body.Description,body.Expression.Representation,body.Expression.Parameters.Select(p => p.Name).ToArray());
            return Task.CompletedTask;
        }

        public Task<Report> UsageTotalStatsAsync()
        {
            return Task.FromResult(new Report()
                {UsageStatistics = new List<FunctionUsage>() {new FunctionUsage {Name = "Test", UsageCount = 1}}});
        }

        public Task<PeriodReport> UserUsageStatsAsync(DateTimeOffset? @from, DateTimeOffset? to)
        {
            return Task.FromResult(new PeriodReport()
                {UsageStatistics = new List<FunctionPeriodUsage>() {new FunctionPeriodUsage {Name = "Test", UsageCount = 1, 
                    PeriodStart = DateTimeOffset.Now.ToMinutePeriodBegin(),
                    PeriodEnd = DateTimeOffset.Now.ToMinutePeriodEnd(),
                    Period= TimeSpan.FromMinutes(1).ToString("g")
                }}});
        }
       
    }
    internal class FunctionAlreadyExistsException : Exception
    {
    }

}