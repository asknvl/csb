using csb.server;
using System;

namespace asknvl.leads
{
    public class LeadsGeneratorFactory
    {
        public static ILeadsGenerator Create(string geotag, LeadAlgorithmType? type, ITGFollowerTrackApi trackApi)
        {
            switch (type)
            {
                case null:
                case LeadAlgorithmType.TrackerOnly:
                    return new LeadsGenerator_TrackerOnly(geotag, trackApi);
                case LeadAlgorithmType.CAPIv1:
                    return new LeadsGenerator_CAPIv1(geotag, trackApi);
                case LeadAlgorithmType.CAPIv2:
                    return new LeadsGenerator_CAPIv2(geotag, trackApi);                  
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
