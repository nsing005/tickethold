# TicketHold

How to build the solution: run dotnet build on the console application
A web app is present, but unused

Unit tests can be run through the visual studio test explorer, although dotnet test should work as well

Assumptions:
Seating capacity can be hardcoded to a specific number (200)
Seat arrangment is considered unimportant
Seats are stored as comma separated list of seat numbers, this isn't a great design but it works for the purpose of this test
Expiration is handled via expiration in a memory cache.  This could lead to "committed" tickets as no temporary or status column is present on the SeatHold
Emails are mostly irrelevant- required but serves no purpose

How to run the application:
The console application can be run using visual studio or with dotnet run
A command line parser library is used, enter --help to see available options

A sqlite in memory database is used for storage
It will refresh whenever the application is restarted