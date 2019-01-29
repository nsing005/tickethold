using CommandLine;

namespace TicketHold.Console
{
    partial class Program
    {
        [Verb("hold", HelpText = "Use this option to hold seats")]
        public class HoldSeats
        {
            [Option('e', "email", Required = true, HelpText = "Provide your email address")]
            public string Email { get; set; }

            [Option('n', "numberOfSeats", Required =true, HelpText = "How many seats would you like?")]
            public int Seats { get; set; }

        }
    }
}
