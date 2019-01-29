using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using TicketHold.Core;

namespace TicketHold.Tests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture().Customize(new DefaultCustomization()))
        {
        }
    }

    public class DefaultCustomization : CompositeCustomization
    {
        public DefaultCustomization() : base(new AutoMoqCustomization(), new DbCustomization(), new MemCacheCustomization())
        {

        }
    }

    public class DbCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<TicketContext>()
                .UseSqlite(connection)
                .Options;
            fixture.Register<TicketContext>(() =>
            {
                var cnx = new TicketContext(options);
                cnx.Database.EnsureCreated();
                return cnx;
            });
            fixture.Register<Func<TicketContext>>(() =>
            {
                return () =>
                {
                    var cnx = new TicketContext(options);
                    cnx.Database.EnsureCreated();
                    return cnx;
                    };
            });
        }
    }
    public class MemCacheCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<IMemoryCache>(() =>
            {
                return new MemoryCache(new MemoryCacheOptions());
            });
        }
    }
}
