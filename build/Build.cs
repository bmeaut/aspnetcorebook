using Nuke.Common;
using static Nuke.Common.Tools.Docker.DockerTasks;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.NuGet;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Compile) })]
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

    Target Compile => _ => _
        .DependsOn(Restore)
        .Produces(RootDirectory / "manuscript/assets/*")
        .Executes(() =>
        {
            DockerRun(v => v
                .SetRm(true)
                .SetImage("asciidoctor/docker-asciidoctor")
                .SetName("asciidoc")
                .SetVolume($"\"{RootDirectory / "manuscript"}\":/documents")
                .SetCommand("sh").SetArgs("/documents/publish.sh"));
        });

}
