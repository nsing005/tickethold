using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TicketHold.Core.Entities;

namespace TicketHold.Core
{
    public interface ITicketService
    {
        Task<int> NumSeatsAvailable();

        Task<SeatHold> FindAndHoldSeats(int numSeats, string customerEmail);

        Task<string> ReserveSeats(int seatHoldId, string customerEmail);
    }
}
