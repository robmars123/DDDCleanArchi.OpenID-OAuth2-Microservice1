var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.BTM_Account_Api>("btm-account-api");
builder.AddProject<Projects.BTM_Duende_ASPIdentity>("btm-identityserver");

builder.AddProject<Projects.BTM_Account_MVC_UI>("btm-account-mvc-client");

builder.Build().Run();
