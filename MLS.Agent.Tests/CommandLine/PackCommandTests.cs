﻿using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.CommandLine;
using WorkspaceServer;
using WorkspaceServer.Tests;
using WorkspaceServer.WorkspaceFeatures;
using Xunit;

namespace MLS.Agent.Tests.CommandLine
{
    public class PackCommandTests
    {
        [Fact]
        public async Task Pack_project_creates_a_nupkg_with_passed_version()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, "3.4.5"), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("3.4.5.nupkg"));
        }
        
        [Fact]
        public async Task Pack_project_works_with_blazor()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, enableBlazor: true), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("nupkg"));
        }

        [Fact]
        public async Task Pack_project_blazor_contents()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var packageName = Path.GetFileNameWithoutExtension(asset.Directory.GetFiles("*.csproj").First().Name);

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, enableBlazor: true), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("nupkg"));

            var dotnet = new Dotnet(asset.Directory);

            var result = await dotnet.ToolInstall(packageName, asset.Directory, asset.Directory);

            var exe = Path.Combine(asset.Directory.FullName, packageName);

            var tool = new WorkspaceServer.WorkspaceFeatures.PackageTool(packageName, asset.Directory);

            await tool.Prepare();

            var projectDirectory = await tool.LocateBuildAsset();
            var subDirectories = projectDirectory.GetDirectories();
            subDirectories.Should().Contain(d => d.Name == "packTarget");
            subDirectories.Should().Contain(d => d.Name == $"runner-{packageName}");

        }

    }
}
