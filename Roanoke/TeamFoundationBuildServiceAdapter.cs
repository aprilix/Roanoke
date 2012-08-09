using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Framework.Server;

namespace Roanoke
{
    public class TeamFoundationBuildServiceAdapter : ITeamFoundationBuildService
    {
        private readonly TeamFoundationBuildService _buildService;
        private readonly TeamFoundationRequestContext _requestContext;

        public TeamFoundationBuildServiceAdapter(TeamFoundationRequestContext requestContext)
        {
            _requestContext = requestContext;
            _buildService = _requestContext.GetService<TeamFoundationBuildService>();
        }

        public TeamFoundationDataReader QueryBuilds(IList<BuildDetailSpec> specs)
        {
            return _buildService.QueryBuilds(_requestContext, specs);
        }
    }
}