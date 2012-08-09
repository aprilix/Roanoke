using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.Common;
using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Warehouse;

namespace Roanoke
{
    public class BuildDropProcessor
    {
        private readonly IWarehouseInterupt _warehouseInterupt;
        private readonly IWarehouseContext _warehouseContext;
        private readonly ITeamFoundationBuildService _buildServer;
        private readonly IFolderSizeCalculator _calculator;
        private const string LastBuildTrackingKey = "/Adapter/Watermark/Roanoke/LastBuildFinishTime";

        public BuildDropProcessor(IWarehouseInterupt warehouseInterupt, IWarehouseContext warehouseContext, ITeamFoundationBuildService buildServer, IFolderSizeCalculator calculator)
        {
            _warehouseInterupt = warehouseInterupt;
            _warehouseContext = warehouseContext;
            _buildServer = buildServer;
            _calculator = calculator;
        }

        public DataChangesResult ProcessBuildDrops(Guid collectionId)
        {
            var trackingTimestamp = GetTrackingTimestamp(collectionId);

            var buildDetailSpec = new BuildDetailSpec
                                      {
                                          MinFinishTime = trackingTimestamp,
                                          DefinitionFilter = new BuildDefinitionSpec { FullPath = string.Format(@"\{0}\{0}", BuildConstants.Star) }, // all build definitions in all team projects
                                          BuildNumber = BuildConstants.Star,
                                          QueryOrder = BuildQueryOrder.FinishTimeAscending,
                                          InformationTypes = { InformationTypes.BuildProject }
                                      };

            using (var buildReader = _buildServer.QueryBuilds(new[] {buildDetailSpec}))
            {

                foreach (var result in buildReader.CurrentEnumerable<BuildQueryResult>())
                {

                    while (result.Builds.MoveNext())
                    {

                        if (_warehouseInterupt.IsWarehouseHostCancelled || _warehouseInterupt.IsWarehouseSchemaLockRequested)
                        {
                            return DataChangesResult.DataChangesPending;
                        }

                        var buildDetail = result.Builds.Current;
                        if (!string.IsNullOrEmpty(buildDetail.DropLocation))
                        {
                            var folderSize = _calculator.Calculate(buildDetail.DropLocation);

                            var buildFinish = buildDetail.FinishTime.ToUniversalTime();
                            trackingTimestamp = trackingTimestamp < buildFinish ? buildFinish : trackingTimestamp;

                            SaveBuildDropSize(collectionId, buildDetail.Uri, folderSize, buildDetail.DropLocation);
                        }

                        SaveTrackingValue(collectionId, trackingTimestamp);
                    }    
                }
                
            }

            return DataChangesResult.NoChangesPending;
        }

        private void SaveBuildDropSize(Guid collectionId, string buildUri, FolderSizeResult folderSize, string dropLocation)
        {
            var buildBk = LinkingUtilities.DecodeUri(buildUri).ToolSpecificId + "|" + collectionId;
            using (var dac = _warehouseContext.CreateWarehouseDataAccessComponent())
            {
                dac.ExecuteNonQuery(
                    CommandType.Text,
                    @"
insert into [dbo].[FactBuildDropSize] ([BuildDropSizeBK], [FileCount], [FileSize], [LastUpdatedDateTime], [BuildBK])
values (@BuildDropSizeBK, @FileCount, @FileSize, @LastUpdatedDateTime, @BuildBK)",
                    new[]
                        {
                            new SqlParameter("@BuildDropSizeBK", dropLocation),
                            new SqlParameter("@FileCount", folderSize.FileCount),
                            new SqlParameter("@FileSize", folderSize.FileSize),
                            new SqlParameter("@LastUpdatedDateTime", DateTime.UtcNow),
                            new SqlParameter("@BuildBK", buildBk)
                        }
                    );
            }
        }

        private void SaveTrackingValue(Guid collectionId, DateTime trackingTimestamp)
        {
            var trackingString = trackingTimestamp.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture);
            using (var dac = _warehouseContext.CreateWarehouseDataAccessComponent())
            {
                dac.SetProperty(collectionId.ToString(), LastBuildTrackingKey, trackingString);
            }
        }

        private DateTime GetTrackingTimestamp(Guid collectionId)
        {
            using (var dac = _warehouseContext.CreateWarehouseDataAccessComponent())
            {
                var trackingString = dac.GetProperty(collectionId.ToString(), LastBuildTrackingKey);
                if (string.IsNullOrEmpty(trackingString))
                {
                    return DateTime.MinValue;
                }
                var ticks = Convert.ToInt64(trackingString, CultureInfo.InvariantCulture);
                return new DateTime(ticks + 1, DateTimeKind.Utc);
            }
        }
    }
}