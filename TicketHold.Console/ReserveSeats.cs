using CommandLine;

namespace TicketHold.Console
{
    partial class Program
    {
        [Verb("reserve", HelpText = "Use this option to reserve seats")]
        public class ReserveSeats
        {
            [Option('e', "email", Required = true, HelpText = "Provide your email address")]
            public string Email { get; set; }

            [Option('s', "seatHoldId", Required =true, HelpText = "Your seat hold Id")]
            public int SeatHoldId { get; set; }
        }
    }
}
