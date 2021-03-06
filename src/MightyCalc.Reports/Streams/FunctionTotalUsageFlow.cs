using System;
using System.Linq;
using Akka;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using MightyCalc.Node;
using MightyCalc.Node.Akka;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionTotalUsageFlow
    {

        private static Flow<EventEnvelope, SequencedFunctionTotalUsage, NotUsed> CreateFlow()
        {
            return Flow.Create<EventEnvelope>()
                .SelectMany(e =>
                {
                    var calculationPerformed = (e.Event as CalculatorActor.CalculationPerformed);
                    //transform each element to pair with number of words in it
                    return calculationPerformed?.FunctionsUsed.GroupBy(f => f)
                        .Select(g => new SequencedFunctionTotalUsage
                        {
                            FunctionName = g.Key,
                            Sequence = (e.Offset as Sequence).Value,
                            InvocationsCount = g.Count()
                        });
                });
        }

        public static Flow<EventEnvelope, SequencedFunctionTotalUsage, NotUsed> Instance { get; } = CreateFlow();

    }
}