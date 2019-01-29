using System;
using System.Collections.Generic;
using System.Text;

namespace TicketHold.Core.Entities
{
    public class SeatHold
    {
        public string HeldSeats { get; set; }

        public int SeatHoldId { get; set; }
        public string CustomerEmail { get; set; }
    }
}
