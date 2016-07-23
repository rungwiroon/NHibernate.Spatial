using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Spatial.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestNHibernateWithNpgsql
{
    internal class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        private static object _locker = new object();

        public static ISessionFactory SessionFactory
        {
            get
            {
                lock (_locker)
                {
                    if (_sessionFactory == null)
                    {
                        var configuration = new Configuration();

                        //_configuration.SetProperty("nhibernate.envers.default_schema", "audit");

                        configuration.AddAuxiliaryDatabaseObject(new SpatialAuxiliaryDatabaseObject(configuration));

                        var mapper = new ModelMapper();
                        mapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());

                        HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
                        configuration.AddMapping(mapping);

                        configuration.AddAssembly(typeof(Model).Assembly);

                        //_configuration.IntegrateWithEnvers();

                        configuration.Configure();

                        _sessionFactory = configuration.BuildSessionFactory();
                    }

                    return _sessionFactory;
                }
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public static IStatelessSession OpenStatelessSession()
        {
            return SessionFactory.OpenStatelessSession();
        }

        public static void GenerateSchema()
        {
            Configuration cfg = new Configuration();

            //cfg.SetProperty("nhibernate.envers.default_schema", "audit");

            cfg.AddAuxiliaryDatabaseObject(new SpatialAuxiliaryDatabaseObject(cfg));

            var mapper = new ModelMapper();
            mapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());

            HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            cfg.AddMapping(mapping);

            cfg.AddAssembly(typeof(Model).Assembly);

            //cfg.IntegrateWithEnvers();

            cfg.Configure();

            new NHibernate.Tool.hbm2ddl.SchemaExport(cfg)
                .SetDelimiter(";")
                //.SetOutputFile("schema.sql")
                .Execute(false, true, false);
        }
    }
}
