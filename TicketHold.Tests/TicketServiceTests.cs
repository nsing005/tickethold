using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using TicketHold.Core;
using TicketHold.Core.Exceptions;
using Xunit;

namespace TicketHold.Tests
{
    public class TicketServiceTests
    {
        [Theory, AutoMoqData]
        public void Sut_ShouldNotBeNull(TicketService sut)
        {
            sut.Should().NotBeNull();
        }

        [Theory, AutoMoqData]
        public async Task Sut_ShouldReturnAvailableSeats([Frozen] TicketContext context,
            TicketService sut)
        {
            context.SeatHolds.Add(new Core.Entities.SeatHold
            {
                HeldSeats = "1,2,3,4,5,6,7,8,9,10",
                CustomerEmail = "supertest@test.com"
            });

            context.SeatHolds.Add(new Core.Entities.SeatHold
            {
                HeldSeats = "11,12,13,14,15,16,17,18,19,20",
                CustomerEmail = "supertest2@test.com"
            });

            await context.SaveChangesAsync();

            (await sut.NumSeatsAvailable()).Should().Be(180);
        }


        [Theory, AutoMoqData]
        public async Task Sut_ShouldThrowIfLessSeatsThanAvailable([Frozen] TicketContext context,
            TicketService sut)
        {
            context.SeatHolds.Add(new Core.Entities.SeatHold
            {
                HeldSeats = "1,2,3,4,5,6,7,8,9,10",
                CustomerEmail = "supertest@test.com"
            });

            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<NoSeatsAvailableException>(async () => await sut.FindAndHoldSeats(200, "supertest@test.com"));
        }

        [Theory, AutoMoqData]
        public async Task Sut_ShouldThrowIfNoReservationFound([Frozen] TicketContext context,
            TicketService sut)
        {
            await Assert.ThrowsAsync<NoReservationFoundException>(async () => await sut.ReserveSeats(200, "supertest@test.com"));
        }

        [Theory, AutoMoqData]
        public async Task Sut_ShouldExpireReservation([Frozen] TicketContext context,
            [Frozen] IMemoryCache cache,
            [Frozen] TicketServiceSettings settings,
           TicketService sut)
        {
            settings.TicketHoldExpiration = TimeSpan.FromSeconds(1);

            var reservation = await sut.FindAndHoldSeats(10, "supertest@test.com");

            cache.TryGetValue(reservation.SeatHoldId.ToString(), out var seatHold).Should().BeTrue();

            await Task.Delay(2000);

            cache.TryGetValue(reservation.SeatHoldId.ToString(), out seatHold).Should().BeFalse();
        }


        [Theory, AutoMoqData]
        public async Task Sut_ShouldExpireReservationWhenConfirmed([Frozen] TicketContext context,
            [Frozen] IMemoryCache cache,
            [Frozen] TicketServiceSettings settings,
           TicketService sut)
        {
            settings.TicketHoldExpiration = TimeSpan.FromMinutes(2);
            var reservation = await sut.FindAndHoldSeats(10, "supertest@test.com");

            cache.TryGetValue(reservation.SeatHoldId.ToString(), out var seatHold).Should().BeTrue();

            await sut.ReserveSeats(reservation.SeatHoldId, "supertest@test.com");

            cache.TryGetValue(reservation.SeatHoldId.ToString(), out seatHold).Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public async Task Sut_ShouldUpdateAvailableSeatsOnExpiration([Frozen] TicketContext context,
            [Frozen] IMemoryCache cache,
            [Frozen] TicketServiceSettings settings,
           TicketService sut)
        {
            settings.TicketHoldExpiration = TimeSpan.FromSeconds(1);
            var reservation = await sut.FindAndHoldSeats(10, "supertest@test.com");

            (await sut.NumSeatsAvailable()).Should().Be(190);

            await Task.Delay(1100);

            cache.TryGetValue(reservation.SeatHoldId.ToString(), out var seatHold).Should().BeFalse();

            await Task.Delay(1100);

            (await sut.NumSeatsAvailable()).Should().Be(200);
        }
    }
}
