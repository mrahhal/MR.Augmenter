#addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json&version=9.0.1"

#load "./build/index.cake"

var target = Argument("target", "Default");

var build = BuildParameters.Create(Context);
var util = new Util(Context, build);

Task("Clean")
	.Does(() =>
{
	if (DirectoryExists("./artifacts"))
	{
		DeleteDirectory("./artifacts", true);
	}
});

Task("Restore")
	.IsDependentOn("Clean")
	.Does(() =>
{
	DotNetCoreRestore();
});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
{
	var settings = new DotNetCoreBuildSettings
	{
		Configuration = build.Configuration,
		VersionSuffix = build.Version.Suffix
	};
	foreach (var project in build.ProjectFiles)
	{
		DotNetCoreBuild(project.FullPath, settings);
	}
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	foreach (var project in build.TestProjectFiles)
	{
		if (IsRunningOnWindows())
		{
			DotNetCoreTest(project.FullPath);
		}
		else
		{
			var name = project.GetFilenameWithoutExtension();
			var dirPath = project.GetDirectory().FullPath;
			var config = build.Configuration;
			var xunit = GetFiles(dirPath + "/bin/" + config + "/net451/*/dotnet-test-xunit.exe").First().FullPath;
			var testfile = GetFiles(dirPath + "/bin/" + config + "/net451/*/" + name + ".dll").First().FullPath;

			using(var process = StartAndReturnProcess("mono", new ProcessSettings{ Arguments = xunit + " " + testfile }))
			{
				process.WaitForExit();
				if (process.GetExitCode() != 0)
				{
					throw new Exception("Mono tests failed!");
				}
			}
		}
	}
});

Task("Pack")
	.Does(() =>
{
	var settings = new DotNetCorePackSettings
	{
		Configuration = build.Configuration,
		VersionSuffix = build.Version.Suffix,
		OutputDirectory = "./artifacts/packages"
	};
	foreach (var project in build.ProjectFiles)
	{
		DotNetCorePack(project.FullPath, settings);
	}
});

Task("Default")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Pack")
	.Does(() =>
{
	util.PrintInfo();
});

Task("Version")
	.Does(() =>
{
	Information(build.FullVersion());
});

Task("Print")
	.Does(() =>
{
	util.PrintInfo();
});

Task("Patch")
	.Does(() =>
{
	util.PatchProjectFileVersions();
});

RunTarget(target);
