using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NHibernate.Spatial.Type;
using NHibernate.SqlCommand;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.Spatial.Dialect
{
    public class PostGis20DialectNpgsql3 : PostGis20Dialect
    {
        private static readonly IType geometryType = new CustomType(typeof(PostGisNpgsql3GeometryType), null);

        public override IType GeometryType
        {
            get { return geometryType; }
        }

        public PostGis20DialectNpgsql3()
            : base()
        {
            RegisterKeyword("text");
            RegisterGeometryTypeForIPointUsingReflection();
        }

        static void RegisterGeometryTypeForIPointUsingReflection()
        {
            var methods =
                typeof(TypeFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            var requiredOverload = methods.Where(
                x =>
                {
                    if (x.Name != "RegisterType") return false;

                    var args = x.GetParameters();
                    if (args.Length != 2) return false;

                    return args[0].ParameterType == typeof(IType)
                           && args[1].ParameterType == typeof(IEnumerable<string>);
                })
                .FirstOrDefault();

            if (requiredOverload == null)
            {
                throw new NotSupportedException(
                    "Could not find TypeFactory.RegisterType method overload in NHibernate. Please report this issue.");
            }

            requiredOverload.Invoke(
                null,
                new object[]
                {
                new CustomType(
                    typeof(PostGisNpgsql3GeometryType),
                    new Dictionary<string, string>
                    {
                        { "srid", "4326" },
                        { "subtype", "POINT" }
                    }),
                new[]
                {
                    typeof(IPoint).AssemblyQualifiedName, typeof(Point).AssemblyQualifiedName }
                });
        }

        public override SqlString GetSpatialRelationString(object geometry, SpatialRelation relation, object anotherGeometry, bool criterion)
        {
            switch (relation)
            {
                case SpatialRelation.Covers:
                    string[] patterns = new string[] {
                        "T*****FF*",
                        "*T****FF*",
                        "***T**FF*",
                        "****T*FF*",
                    };
                    SqlStringBuilder builder = new SqlStringBuilder();
                    builder.Add("(");
                    for (int i = 0; i < patterns.Length; i++)
                    {
                        if (i > 0)
                            builder.Add(" OR ");
                        builder
                            .Add(SpatialDialect.IsoPrefix)
                            .Add("Relate")
                            .Add("(")
                            .AddObject(geometry)
                            .Add(", ")
                            .AddObject(anotherGeometry)
                            .Add(", '")
                            .Add(patterns[i])
                            .Add("')")
                            .ToSqlString();
                    }
                    builder.Add(")");
                    return builder.ToSqlString();

                case SpatialRelation.CoveredBy:
                    return GetSpatialRelationString(anotherGeometry, SpatialRelation.Covers, geometry, criterion);

                default:
                    return new SqlStringBuilder(6)
                        .Add(SpatialDialect.IsoPrefix)
                        .Add(relation.ToString())
                        .Add("(")
                        .AddObject(geometry)
                        .Add(", ")
                        .AddObject(anotherGeometry)
                        .Add(")")
                        .ToSqlString();
            }
        }

        public override SqlString GetSpatialAnalysisString(object geometry, SpatialAnalysis analysis, object extraArgument)
        {
            switch (analysis)
            {
                case SpatialAnalysis.Buffer:
                    if (!(extraArgument is Parameter || new SqlString(SqlCommand.Parameter.Placeholder).Equals(extraArgument)))
                    {
                        extraArgument = Convert.ToString(extraArgument, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    return new SqlStringBuilder(6)
                        .Add(SpatialDialect.IsoPrefix)
                        .Add("Buffer(")
                        .AddObject(geometry)
                        .Add(", ")
                        .AddObject(extraArgument)
                        .Add(")")
                        .ToSqlString();

                case SpatialAnalysis.ConvexHull:
                    return new SqlStringBuilder()
                        .Add(SpatialDialect.IsoPrefix)
                        .Add("ConvexHull(")
                        .AddObject(geometry)
                        .Add(")")
                        .ToSqlString();

                case SpatialAnalysis.Difference:
                case SpatialAnalysis.Distance:
                case SpatialAnalysis.Intersection:
                case SpatialAnalysis.SymDifference:
                case SpatialAnalysis.Union:
                    return new SqlStringBuilder()
                        .Add(SpatialDialect.IsoPrefix)
                        .Add(analysis.ToString())
                        .Add("(")
                        .AddObject(geometry)
                        .Add(",")
                        .AddObject(extraArgument)
                        .Add(")")
                        .ToSqlString();

                default:
                    throw new ArgumentException("Invalid spatial analysis argument");
            }
        }
    }
}
