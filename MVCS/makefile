migration-tenants:
	dotnet ef migrations add $(name) -s ./MVCS.Presentation.gRPC/ -p ./MVCS.Infrastructure/ -c ApplicationDbContext -o Persistence/Migrations

migration-shared:
	dotnet ef migrations add $(name) -s ./MVCS.Presentation.gRPC/ -p ./MVCS.Infrastructure/ -c IdentityDbContext -o Identity/Migrations

remove-tenants:
	dotnet ef migrations remove -s ./MVCS.Presentation.gRPC/ -p ./MVCS.Infrastructure/ -c ApplicationDbContext

remove-shared:
	dotnet ef migrations remove -s ./MVCS.Presentation.gRPC/ -p ./MVCS.Infrastructure/ -c IdentityDbContext

update-shared:
	dotnet ef database update -s ./MVCS.Presentation.gRPC/ -p ./MVCS.Infrastructure/ -c IdentityDbContext