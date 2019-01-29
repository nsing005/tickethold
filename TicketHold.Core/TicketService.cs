using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketHold.Core.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketHold.Core.Exceptions;

namespace TicketHold.Core
{
    public class TicketService : ITicketService
    {
        private readonly IMemoryCache _cache;
        private readonly TicketServiceSettings _settings;
        private readonly TicketContext _context;
        private readonly ILogger<TicketService> _logger;
        private readonly Func<TicketContext> factory;

        public TicketService(IMemoryCache cache, TicketServiceSettings settings, TicketContext context, ILogger<TicketService> logger, Func<TicketContext> factory)
        {
            _cache = cache;
            _settings = settings;
            _context = context;
            _logger = logger;
            this.factory = factory;
        }

        public async Task<SeatHold> FindAndHoldSeats(int numSeats, string customerEmail)
        {

            var unreservedSeats = (await FindUnreservedSeats()).Take(numSeats).ToList();

            if(unreservedSeats.Count() < numSeats)
            {
                throw new NoSeatsAvailableException();
            }

            var seatHold = new SeatHold()
            {
                HeldSeats = string.Join(',', unreservedSeats),
                CustomerEmail = customerEmail
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _settings.TicketHoldExpiration
            };

            options.RegisterPostEvictionCallback((o, v, r, s) =>
            {
                if (r == EvictionReason.Removed)
                {
                    return;
                }
                var cnx = factory();
                cnx.SeatHolds.Remove(v as SeatHold);
                cnx.SaveChanges();
            });

            await _context.SeatHolds.AddAsync(seatHold);
            await _context.SaveChangesAsync();
            _cache.Set<SeatHold>(seatHold.SeatHoldId.ToString(), seatHold, options);



            return seatHold;
        }

        public async Task<int> NumSeatsAvailable()
        {
            // Assuming there are 200 available seats for this "theater"
            // Configuration on that could be supplied on a per room basis

            var reservedSeats = (await _context.SeatHolds.Select(t => t).ToListAsync()).Sum(x => x.HeldSeats.Split(',').Count());
            //var pending = _cache.Get<List<string>>("pendingReservations");
            //var pendingObjects = pending.Select(x => _cache.Get<SeatHold>(x)).ToList();

            //var pendingReservationCount = pendingObjects.Sum(t => t.HeldSeat.Count());

            return 200 - reservedSeats;//(reservedSeats + pendingReservationCount);
        }

        public async Task<string> ReserveSeats(int seatHoldId, string customerEmail)
        {
            if(!_cache.TryGetValue<SeatHold>(seatHoldId.ToString(), out var seatHold))
            {
                throw new NoReservationFoundException();
            }
            _cache.Remove(seatHoldId.ToString());

            return seatHoldId.ToString();
        }

        protected async Task<List<int>> FindUnreservedSeats()
        {
            var heldSeats = new List<int>();
            // get all the confirmed reservations and add their seats
            heldSeats.AddRange((await _context.SeatHolds.Select(t => t).ToListAsync()).SelectMany(t => t.HeldSeats.Split(',').Select(x => Convert.ToInt32(x))));
            //var pending = _cache.Get<List<string>>("pendingReservations");
            //var pendingObjects = pending.Select(x => _cache.Get<SeatHold>(x)).ToList();

            //// do the same for the ones in the cache (unconfirmed)
            //heldSeats.AddRange(pendingObjects.SelectMany(t => t.HeldSeat));
            return (await AllSeats()).Where(t => !heldSeats.Contains(t)).ToList();
        }

        /// <summary>
        /// Because we need to know what all the seat numbers are
        /// Which is basically a number of 1-200
        /// Not starting at 0 because users don't understand that indicies start at 0
        /// </summary>
        /// <returns></returns>
        protected async Task<List<int>> AllSeats()
        {
            var seats = new List<int>();
            for (int i = 1; i <= 200; i++)
            {
                seats.Add(i);
            }
            return seats;
        }
    }
}
