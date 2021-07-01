using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.Docker.DockerTasks;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.Docker;
using System.IO;
using Octokit;
using MimeTypes;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Compile) }, 
    ImportGitHubTokenAs = nameof(GitHubToken))]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter]
    readonly string GitHubToken = "secret token";

    [Parameter]
    readonly string ReleaseTag;

    [Parameter]
    readonly string ReleaseName;

    [Parameter]
    readonly string ReleaseBody;

    [Parameter]
    readonly string GITHUB_ACTOR;

    [Parameter]
    readonly string GITHUB_REPOSITORY;

    [Parameter]
    readonly string AssetsDir = "assets";

    [Parameter]
    readonly bool IsDraft = true;

    (string Owner, string Repo) GitHubRepoIdentity
    {
        get
        {
            string[] splitted= GITHUB_REPOSITORY.Split('/');
            return (splitted[0], splitted[1]);
        }
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DockerImagePull(v => v.SetName("asciidoctor/docker-asciidoctor"));
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue(GitHubRepoIdentity.Repo))
                { Credentials = new Credentials(GitHubToken) };
            var newRelease = new NewRelease(ReleaseTag)
            {
                Name = ReleaseName,
                Body = ReleaseBody,
                Draft = IsDraft
            };
            
            var result = await client.Repository.Release.Create(GitHubRepoIdentity.Owner, GitHubRepoIdentity.Repo, newRelease);

            foreach (var fp in Directory.EnumerateFiles(AssetsDir))
            {
                using Stream file = File.OpenRead(fp);
                var assetUpload = new ReleaseAssetUpload()
                {
                    FileName = Path.GetFileName(fp),
                    ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fp)),
                    RawData = file
                };
                var asset = await client.Repository.Release.UploadAsset(result, assetUpload);
            }            
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DockerRun(v => v
                .SetRm(true)
                .SetImage("asciidoctor/docker-asciidoctor")
                .SetName("asciidoc")
                .SetVolume($"\"{Path.Combine(RootDirectory,"manuscript")}\":/documents")
                .SetCommand("sh").SetArgs("/documents/publish.sh"));
        });

}
