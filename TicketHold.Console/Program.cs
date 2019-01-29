using CommandLine;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketHold.Core;
using TicketHold.Core.Exceptions;

namespace TicketHold.Console
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var dbOptions = new DbContextOptionsBuilder<TicketContext>()
                    .UseSqlite(connection)
                    .Options;

            services.AddDbContext<TicketContext>(options =>
            {
                options.UseSqlite(connection);
            });

            services.AddScoped<ITicketService, TicketService>((sp) =>
            {
                var context = sp.GetRequiredService<TicketContext>();
                context.Database.EnsureCreated();
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger = sp.GetRequiredService<ILogger<TicketService>>();
                return new TicketService(cache, new TicketServiceSettings
                {
                    TicketHoldExpiration = TimeSpan.FromMinutes(2)
                }, context, logger, () =>
                {
                    return new TicketContext(dbOptions);
                });
            });

            var provider = services.BuildServiceProvider();

            var svc = provider.GetRequiredService<ITicketService>();

            //var available = await svc.NumSeatsAvailable();

            //var sh = await svc.FindAndHoldSeats(10, "test");
            //await svc.ReserveSeats(sh.SeatHoldId, sh.CustomerEmail);

            //available = await svc.NumSeatsAvailable();

            System.Console.WriteLine("Enter your input below, use --help to see options");
            while (true)
            {
                var input = System.Console.ReadLine().Split(' ');
                await CommandLine.Parser.Default.ParseArguments<QuerySeats, ReserveSeats, HoldSeats>(input)
                    .MapResult(async (QuerySeats q) => {
                        System.Console.WriteLine($"{await svc.NumSeatsAvailable()} seats remaining");
                        return 1; },
                    async (ReserveSeats r) => {
                        try
                        {
                            System.Console.WriteLine($"Success!  Your reservation Id is: " +
                                $"{(await svc.ReserveSeats(r.SeatHoldId, r.Email))}");
                        }
                        catch(NoReservationFoundException ex)
                        {
                            System.Console.WriteLine("No seat hold found for the provided id, please create a reservation first");
                        }
                        return 1; },
                    async (HoldSeats h) => {
                        try
                        {
                            System.Console.WriteLine($"Success!  Your reservation Id is: {(await svc.FindAndHoldSeats(h.Seats, h.Email)).SeatHoldId}");
                        }
                        catch(NoSeatsAvailableException ex)
                        {
                            System.Console.WriteLine("Not enough remaining seats");
                        }
                        return 1; },
                    async errs => 1);
            }
        }
    }
}
