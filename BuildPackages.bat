if not exist "./NuGet Packages" mkdir "./NuGet Packages"
call ".nuget/NuGet.exe" pack NHibernate.Spatial.MsSql\NHibernate.Spatial.MsSql.csproj -Build -IncludeReferencedProjects -Properties Configuration=Release -OutputDirectory "./NuGet Packages"
call ".nuget/NuGet.exe" pack NHibernate.Spatial.MySQL\NHibernate.Spatial.MySQL.csproj -Build -IncludeReferencedProjects -Properties Configuration=Release -OutputDirectory "./NuGet Packages"
call ".nuget/NuGet.exe" pack NHibernate.Spatial.PostGis\NHibernate.Spatial.PostGis.csproj -Build -IncludeReferencedProjects -Properties Configuration=Release -OutputDirectory "./NuGet Packages"
call ".nuget/NuGet.exe" pack NHibernate.Spatial.Oracle\NHibernate.Spatial.Oracle.csproj -Build -IncludeReferencedProjects -Properties Configuration=Release -OutputDirectory "./NuGet Packages"
