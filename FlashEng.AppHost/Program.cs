var builder = DistributedApplication.CreateBuilder(args);

// Додаємо API проект
var api = builder.AddProject<Projects.FlashEng_Api>("flasheng-api");

builder.Build().Run();