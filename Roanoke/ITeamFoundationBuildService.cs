using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Framework.Server;

namespace Roanoke
{
    public interface ITeamFoundationBuildService
    {
        TeamFoundationDataReader QueryBuilds(IList<BuildDetailSpec> buildDetailSpecs);
    }
}