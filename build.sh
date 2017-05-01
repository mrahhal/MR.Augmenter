header "Adjusting file descriptors limit, if necessary"
FILE_DESCRIPTOR_LIMIT=$( ulimit -n )
if [ $FILE_DESCRIPTOR_LIMIT -lt 512 ]
then
	info "Increasing file description limit to 512"
	ulimit -n 512
fi

dotnet --info
dotnet restore
dotnet test test/MR.Augmenter.Tests/MR.Augmenter.Tests.csproj -f netcoreapp1.1
dotnet test test/MR.Augmenter.AspNetCore.Tests/MR.Augmenter.AspNetCore.Tests.csproj -f netcoreapp1.1
