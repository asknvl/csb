using System;

namespace asknvl.leads
{
    public class LeadsGeneratorFactory
    {
        public LeadsGeneratorBase Get(LeadAlgorithmType type)
        {
            switch (type)
            {
                case LeadAlgorithmType.TrackerOnly:
                    return null;
                case LeadAlgorithmType.CAPIv1:
                    return new LeadsGenerator_CAPIv1();
                case LeadAlgorithmType.CAPIv2:
                    return new LeadsGenerator_CAPIv2();
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
