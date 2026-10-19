using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RA_MuniAnnouncements.Clients;
using RA_MuniAnnouncements.Services;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var authority = jwt["Authority"];
var useLocalJwt = string.IsNullOrWhiteSpace(authority);
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
byte[]? signingKey = null;

if (useLocalJwt)
{
	issuer ??= builder.Environment.IsDevelopment()
		? "muni-local"
		: throw new InvalidOperationException("Jwt:Issuer must be configured outside Development.");
	audience ??= builder.Environment.IsDevelopment()
		? "muni-client"
		: throw new InvalidOperationException("Jwt:Audience must be configured outside Development.");
	var signingKeyValue = jwt["SigningKey"];
	if (string.IsNullOrWhiteSpace(signingKeyValue) && builder.Environment.IsDevelopment())
	{
		signingKey = RandomNumberGenerator.GetBytes(32);
	}
	else if (!string.IsNullOrWhiteSpace(signingKeyValue))
	{
		signingKey = Convert.FromBase64String(signingKeyValue);
	}
	else
	{
		throw new InvalidOperationException("Jwt:SigningKey must be configured outside Development.");
	}
}

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		if (!string.IsNullOrWhiteSpace(authority))
		{
			options.Authority = authority;
			options.Audience = jwt["Audience"];
			options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
		}
		else
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = issuer,
				ValidateAudience = true,
				ValidAudience = audience,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(signingKey!),
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromMinutes(1)
			};
		}
	});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IFormsClient, MockFormsClient>();
builder.Services.AddSingleton<IServiceCatalogRepository, MockServiceCatalogRepository>();
builder.Services.AddScoped<MyRequestsService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment() && useLocalJwt)
{
	app.MapGet("/api/dev-auth/token", () =>
	{
		var now = DateTime.UtcNow;
		var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
		{
			Issuer = issuer,
			Audience = audience,
			IssuedAt = now,
			NotBefore = now.AddSeconds(-5),
			Expires = now.AddMinutes(15),
			Subject = new ClaimsIdentity([new Claim("idNumber", "012345678")]),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(signingKey!),
				SecurityAlgorithms.HmacSha256)
		});

		return Results.Ok(new { accessToken = token });
	}).AllowAnonymous();
}

app.Run();

public partial class Program;
