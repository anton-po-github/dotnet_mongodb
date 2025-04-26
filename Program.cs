using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(MongoDB.Bson.BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(MongoDB.Bson.BsonType.String));

services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

using (var scopeRole = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scopeRole.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
