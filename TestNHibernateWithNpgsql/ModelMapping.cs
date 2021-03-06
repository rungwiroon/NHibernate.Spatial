﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNHibernateWithNpgsql
{
    public class ModelMapping : ClassMapping<Model>
    {
        public ModelMapping()
        {
            Table("test_npgsql");

            Id(bd => bd.Id, map =>
            {
                map.Column("id");
                map.Generator(Generators.Assigned);
            });

            Property(g => g.Point, map =>
            {
                map.Column(cm =>
                {
                    cm.Name("point");
                });

                map.Type<NHibernate.Spatial.Type.PostGisNpgsql3GeometryType>(new
                {
                    srid = 4326,
                    subtype = "POINT"
                });
            });

            Component(bd => bd.GpsData, bg =>
            {
                bg.Property(g => g.DateTime, map => map.Column("gps_date_time"));

                bg.Property(g => g.Point, map =>
                {
                    map.Column(cm =>
                    {
                        cm.Name("gps_point");
                    });

                    map.Type<NHibernate.Spatial.Type.PostGisNpgsql3GeometryType>();
                });

                bg.Property(g => g.Heading, map => map.Column("gps_heading"));
                bg.Property(g => g.Speed, map => map.Column("gps_speed"));
                bg.Property(g => g.Altitude, map => map.Column("gps_altitude"));
                bg.Property(g => g.Accuracy, map => map.Column("gps_accuracy"));
            });
        }
    }
}
