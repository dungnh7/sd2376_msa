using MotoFacts;
using Microsoft.Extensions.Configuration;
using MotoFacts.Context;
using MotoFacts.Repository;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTracing;
using OpenTracing.Util;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(
                    (hostingContext, services, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .WriteTo.OpenTracing()
                        );


// Add services to the container.
builder.Services.AddCors(o => o.AddPolicy("EddyCROS", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<MotoFactsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var serice = builder.Services;
serice.AddSingleton<ITracer>(sp =>
{
    string serviceName = Assembly.GetEntryAssembly().GetName().Name;

    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
    .RegisterSenderFactory<ThriftSenderFactory>();

    // This will log to a default localhost installation of Jaeger.
    var config = Jaeger.Configuration.FromEnv(loggerFactory);

    var tracer = config.GetTracer();
    if (!GlobalTracer.IsRegistered())
    {
        // Allows code that can't use DI to also access the tracer.
        GlobalTracer.Register(tracer);
    }

    return tracer;
});

serice.AddOpenTracing(); // Add OpenTracing services

serice.AddSingleton<IDapperContext, DapperContext > ();
serice.AddTransient<IRepository<products>, Repository<products>>();

var app = builder.Build();


using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<MotoFactsDbContext>();
    context.Database.Migrate();
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{ }
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("EddyCROS");
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
