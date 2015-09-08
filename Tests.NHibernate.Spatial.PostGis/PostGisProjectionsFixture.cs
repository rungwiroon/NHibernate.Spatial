using NHibernate.Cfg;
using NHibernate.Spatial.Type;
using NUnit.Framework;
using System;

namespace Tests.NHibernate.Spatial
{
    [TestFixture]
    public class PostGisProjectionsFixture : ProjectionsFixture
    {
        protected override Type GeometryType
        {
            get
            {
                return typeof(PostGisGeometryType);
            }
        }

        protected override void Configure(Configuration configuration)
        {
            TestConfiguration.Configure(configuration);
        }
    }
}