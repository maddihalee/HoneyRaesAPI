using HoneyRaesAPI.Models;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

List<HoneyRaesAPI.Models.Customer> customers = new List<HoneyRaesAPI.Models.Customer> 
{
    new Customer()
    {
        Id = 1,
        Name = "Steve",
        Address = "123 Street Rd",
    },
    new Customer() 
    { 
        Id = 2,
        Name = "Bob",
        Address = "124 Street Rd",
    }, 
    new Customer()
    {
        Id = 3,
        Name = "Jon",
        Address = "2586 Windhaven Dr"
    }
};
List<HoneyRaesAPI.Models.Employee> employees = new List<HoneyRaesAPI.Models.Employee> 
{
    new Employee()
    {
        Id = 1,
        Name = "Michael",
        Specialty = "Debugging"
    },
    new Employee()
    {
        Id = 2,
        Name = "Sheryl",
        Specialty = "Livin'"
    }
};
List<HoneyRaesAPI.Models.ServiceTicket> serviceTickets = new List<HoneyRaesAPI.Models.ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 123,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "A problem",
        Emergency = true,
        DateCompleted = DateTime.Now,
    },
    new ServiceTicket()
    {
        Id = 1234,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "A serious problem",
        Emergency = false,
        DateCompleted = DateTime.Now,
    }
};

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/customers/{id}", (int id) => {
    Customer customer = customers.FirstOrDefault(e => e.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    serviceTickets.Remove(serviceTickets.FirstOrDefault(ticket => ticket.Id == id));
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/servicetickets/emergency", () =>
{
    List<ServiceTicket> emergencyTicket = serviceTickets.Where(st => st.Emergency == true).ToList();
    return Results.Ok(emergencyTicket);

});

app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    return Results.Ok(unassignedTickets);
});



app.Run();
