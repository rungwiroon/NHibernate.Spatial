using log4net.Config;
using NetTopologySuite.Geometries;
using NHibernate.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNHibernateWithNpgsql
{
    [SetUpFixture]
    public class UnitTestSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            XmlConfigurator.Configure();
            NHibernateHelper.GenerateSchema();
        }
    }

    [TestFixture]
    public class UnitTest
    {
        [Test]
        public void InsertGoemetry()
        {
            var point = new Point(1, 1);
            var model = new Model()
            {
                Id = 1,
                PointForWrite = point,
                GpsData = new GpsData()
                {
                    Point = point
                }
            };

            using (var session = NHibernateHelper.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                session.Save(model);
                session.Flush();

                tx.Commit();

                session.Clear();

                var sqlQuery = session.CreateSQLQuery("select {t.*} from test_npgsql t where id = :id")
                    .AddEntity("t", typeof(Model))
                    .SetInt64("id", 1L);

                var dbModel1 = sqlQuery.List<Model>().First();

                Assert.IsNotNull(dbModel1);
                //Assert.IsNotNull(dbModel1.Point);
                //Assert.AreEqual(point, dbModel1.Point);
                //Assert.IsNotNull(dbModel1.GpsData);
                //Assert.IsNotNull(dbModel1.GpsData.Point);
                //Assert.AreEqual(point, dbModel1.GpsData.Point);

                //session.Clear();

                var dbModel2 = session.Get<Model>(1L);

                Assert.IsNotNull(dbModel2);
                //Assert.IsNotNull(dbModel2.Point);
                //Assert.AreEqual(point, dbModel2.Point);
                //Assert.IsNotNull(dbModel2.GpsData);
                //Assert.IsNotNull(dbModel2.GpsData.Point);
                //Assert.AreEqual(point, dbModel2.GpsData.Point);

                session.Clear();

                var dbModel3 = session.Query<Model>()
                    .Where(m => m.PointForWrite == point)
                    .FirstOrDefault();

                Assert.IsNotNull(dbModel3);
                //Assert.IsNotNull(dbModel3.Point);
                //Assert.AreEqual(point, dbModel3.Point);
                //Assert.IsNotNull(dbModel3.GpsData);
                //Assert.IsNotNull(dbModel3.GpsData.Point);
                //Assert.AreEqual(point, dbModel3.GpsData.Point);

                var dbModel4 = session.Query<Model>()
                    .Where(m => m.PointForWrite.Intersects(point))
                    .FirstOrDefault();

                Assert.IsNotNull(dbModel4);

                //tx.Rollback();
            }
        }
    }
}
