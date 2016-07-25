// Copyright 2007 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NHibernate.Spatial.
// NHibernate.Spatial is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// NHibernate.Spatial is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NHibernate.Spatial; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NpgsqlTypes;
using System;
using System.Data;
using System.Linq;

namespace NHibernate.Spatial.Type
{
    /// <summary>
    /// This PostGIS geometry type implementation uses strings to represent
    /// byte arrays in hexadecimal, instead of use byte array directly,
    /// due to a limitation in Npgsql driver.
    /// See http://gborg.postgresql.org/project/npgsql/bugs/bugupdate.php?1409
    /// </summary>
    [Serializable]
    public class PostGisNpgsql3GeometryType : PostGisGeometryType
    {
        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            int index = rs.GetOrdinal(names[0]);

            if (rs.IsDBNull(index))
            {
                return null;
            }

            else
            {
                var value = rs.GetValue(index);

                if(value is PostgisPoint)
                {
                    return ConvertToPoint((PostgisPoint)value);
                }

                else if(value is PostgisLineString)
                {
                    return ConvertToLineString((PostgisLineString)value);
                }

                else if(value is PostgisPolygon)
                {
                    return ConvertToPolygon((PostgisPolygon)value);
                }

                return value;
            }
        }

        public Point ConvertToPoint(PostgisPoint point)
        {
            var newPoint = new Point(point.X, point.Y);
            newPoint.SRID = (int)point.SRID;
            return newPoint;
        }

        public LineString ConvertToLineString(PostgisLineString lineString)
        {
            throw new NotImplementedException();
        }

        public Polygon ConvertToPolygon(PostgisPolygon polygon)
        {
            throw new NotImplementedException();
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            IDbDataParameter parameter = (IDbDataParameter)cmd.Parameters[index];

            cmd.CommandText = cmd.CommandText.Replace(parameter.ParameterName, parameter.ParameterName + "::geometry");

            // set the parameter value before the size check, since ODBC changes the size automatically

            parameter.Value = FromGeometry(value);

            if (parameter.Size > 0 && ((string)value).Length > parameter.Size)
                throw new HibernateException("The length of the string value exceeds the length configured in the mapping/parameter.");
        }
    }
}