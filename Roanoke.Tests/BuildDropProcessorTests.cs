using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Roanoke.Tests
{
    [TestClass]
    public class BuildDropProcessorTests
    {
        [TestMethod]
        public void BuildDropProcessor_should_specify_wildcard_BuildNumber_in_query()
        {
            var calculator = Substitute.For<IFolderSizeCalculator>();
            var interupt = Substitute.For<IWarehouseInterupt>();
            var context = Substitute.For<IWarehouseContext>();
            var buildServer = Substitute.For<ITeamFoundationBuildService>();

            var reader = new TeamFoundationDataReader(new object[] {new BuildQueryResult[0]});
            buildServer.QueryBuilds(null).ReturnsForAnyArgs(reader);

            var processor = new BuildDropProcessor(interupt, context, buildServer, calculator);
            var collectionId = Guid.NewGuid();

            processor.ProcessBuildDrops(collectionId);

            buildServer.Received().QueryBuilds(Arg.Is<IList<BuildDetailSpec>>(l => l[0].BuildNumber == "*"));
        }

        [TestMethod]
        public void BuildDropProcessor_should_write_a_row_to_SQL()
        {
            var calculator = Substitute.For<IFolderSizeCalculator>();
            calculator.Calculate(null).ReturnsForAnyArgs(new FolderSizeResult());

            var interupt = Substitute.For<IWarehouseInterupt>();
            var context = Substitute.For<IWarehouseContext>();
            var buildServer = Substitute.For<ITeamFoundationBuildService>();

            var dac = Substitute.For<IWarehouseDataAccessComponent>();
            context.CreateWarehouseDataAccessComponent().Returns(dac);
            
            var queryResult = new BuildQueryResult();
            var reader = new TeamFoundationDataReader(new object[] {new[] {queryResult}});
            var buildDetail = new BuildDetail {DropLocation = @"\\foo\bar", Uri = "vstfs:///Build/Build/1"};
            queryResult.Builds.Add(buildDetail);
            buildServer.QueryBuilds(null).ReturnsForAnyArgs(reader);

            var processor = new BuildDropProcessor(interupt, context, buildServer, calculator);
            var collectionId = Guid.NewGuid();

            processor.ProcessBuildDrops(collectionId);

            dac.ReceivedWithAnyArgs().ExecuteNonQuery(CommandType.Text, null, null);
        }
    
    }
}
