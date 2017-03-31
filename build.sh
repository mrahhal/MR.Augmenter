dotnet --info
dotnet restore
dotnet test test/MR.Augmenter.Tests/MR.Augmenter.Tests.csproj -f netcoreapp1.1
dotnet test test/MR.Augmenter.AspNetCore.Tests/MR.Augmenter.AspNetCore.Tests.csproj -f netcoreapp1.1
