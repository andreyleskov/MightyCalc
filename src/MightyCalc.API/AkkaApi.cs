using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MightyCalc.Calculations.Aggregate.Commands;
using MightyCalc.Node;
using MightyCalc.Node.Domain;
using MightyCalc.Reports;

namespace MightyCalc.API
{
    public class AkkaApi : IApiController
    {
        private readonly INamedCalculatorPool _pool;
        private readonly IFunctionsTotalUsageQuery _totalUsageQuery;
        private readonly IFunctionsUsageQuery _usageQuery;
        private readonly IKnownFunctionsQuery _knownFunctionsQuery;

        public AkkaApi(INamedCalculatorPool pool, 
                       IFunctionsTotalUsageQuery totalUsageQuery,
                       IFunctionsUsageQuery usageQuery,
                       IKnownFunctionsQuery knownFunctionsQuery)
        {
            _knownFunctionsQuery = knownFunctionsQuery;
            _usageQuery = usageQuery;
            _totalUsageQuery = totalUsageQuery;
            _pool = pool;
        }

        public Task<double> CalculateAsync(Expression body)
        {
            return _pool.For("anonymous").Calculate(body.Representation,
                body.Parameters.Select(p => new Calculations.Parameter(p.Name, p.Value)).ToArray());
        }

        public async Task<IReadOnlyCollection<NamedExpression>> FindFunctionsAsync(string name)
        {
            var definitions = await _knownFunctionsQuery.Execute("anonymous",name);
            return definitions.Select(d => new NamedExpression()
            {
                Description = d.Description,
                Expression = new Expression
                {
                    Parameters = d.Parameters.Select(p => new Parameter()
                    {
                        Name = p
                    }).ToList(),
                    Representation = d.Expression
                },
                Name = d.Name
            }).ToArray();
        }

        public async Task CreateFunctionAsync(NamedExpression body)
        {
            //API-specific restriction, not coming from business logic! 
            var functionDefinitions = await _knownFunctionsQuery.Execute("anonymous", body.Name);
            if (functionDefinitions.Any(f => f.Name == body.Name))
                throw new FunctionAlreadyExistsException();

            await _pool.For("anonymous").AddFunction(body.Name,
                body.Description,
                body.Expression.Representation,
                body.Expression.Parameters.Select(p => p.Name).ToArray());
        }

        internal class FunctionAlreadyExistsException : Exception
        {
        }

        public Task ReplaceFunctionAsync(NamedExpression body)
        {
            return _pool.For("anonymous").AddFunction(body.Name,
                body.Description,
                body.Expression.Representation,
                body.Expression.Parameters.Select(p => p.Name).ToArray());
        }

        public async Task<Report> UsageTotalStatsAsync()
        {
            var usage = await _totalUsageQuery.Execute();
            return new Report()
            {
                UsageStatistics = usage.Select(u => new FunctionUsage
                {
                    Name = u.FunctionName,
                    UsageCount = (int) u.InvocationsCount
                }).ToList()
            };
        }

        public async Task<PeriodReport> UserUsageStatsAsync(DateTimeOffset? @from, DateTimeOffset? to)
        {
            var usage = await _usageQuery.Execute("anonymous", @from, to);
            return new PeriodReport()
            {
                UsageStatistics = usage.Select(u => new FunctionPeriodUsage
                {
                    Name = u.FunctionName,
                    UsageCount = u.InvocationsCount,
                    PeriodStart = u.PeriodStart,
                    PeriodEnd = u.PeriodEnd,
                    Period = u.Period.ToString("g")
                }).ToList()
            };
        }
    }
}