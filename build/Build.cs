using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions("release", GitHubActionsImage.WindowsLatest, GitHubActionsImage.WindowsLatest,
    InvokedTargets = new[]
    {
        nameof(Publish)
    },
    OnPushBranches = new[]
    {
        "main"
    },
    ImportSecrets = new[]
    {
        nameof(NugetApiKey)
    }
)]
class Build : NukeBuild
{
    public static int Main()
    {
        return Execute<Build>(x => x.Compile);
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath PackagesDirectory => RootDirectory / "packages";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            if (!DirectoryExists(PackagesDirectory))
            {
                Directory.CreateDirectory(PackagesDirectory);
            }

            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            PackagesDirectory.GlobFiles("*").ForEach(DeleteFile);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });


    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution.GetProject("VkNet.Extensions.Polling"))
                .SetOutputDirectory(PackagesDirectory)
                .SetConfiguration(Configuration));
        });


    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json";
    [Parameter] string NugetSymbolsUrl = "https://nuget.smbsrc.net/";
    [Parameter] string NugetApiKey;

    Target Publish => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiUrl)
        .Requires(() => NugetSymbolsUrl)
        .Requires(() => NugetApiKey)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            GlobFiles(PackagesDirectory, "*")
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSymbolSource(NugetSymbolsUrl)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NugetApiKey)
                    );
                });
        });
}