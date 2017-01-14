dotnet --info
dotnet restore
dotnet test test/MR.Augmenter.Tests -f netcoreapp1.0
dotnet test test/MR.Augmenter.AspNetCore.Tests -f netcoreapp1.0
