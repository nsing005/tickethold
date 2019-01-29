using CommandLine;

namespace TicketHold.Console
{
    partial class Program
    {
        [Verb("query", HelpText = "Use this option to see how many seats remain")]
        public class QuerySeats
        {
        }
    }
}
