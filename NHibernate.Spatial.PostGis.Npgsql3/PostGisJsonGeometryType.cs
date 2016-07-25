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
    public class PostGisJsonGeometryType : GeometryTypeBase<string>
    {
        private readonly string GeometryFromGeoJsonFunctionStart = "ST_GeomFromGeoJSON(";
        private readonly string GeometryFromGeoJsonFunctionEnd = ")";

        /// <summary>
        /// Initializes a new instance of the <see cref="PostGisGeometryType"/> class.
        /// </summary>
        public PostGisJsonGeometryType()
            : base(NHibernateUtil.StringClob)
        {
        }

        //public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        //{
        //    int index = rs.GetOrdinal(names[0]);

        //    if (rs.IsDBNull(index))
        //    {
        //        return null;
        //    }

        //    else
        //    {
        //        var value = rs.GetValue(index);

        //        return value;
        //    }
        //}

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            IDbDataParameter parameter = (IDbDataParameter)cmd.Parameters[index];

            cmd.CommandText = cmd.CommandText.Replace(parameter.ParameterName, GeometryFromGeoJsonFunctionStart + parameter.ParameterName + GeometryFromGeoJsonFunctionEnd);

            // set the parameter value before the size check, since ODBC changes the size automatically

            parameter.Value = FromGeometry(value);

            if (parameter.Size > 0 && ((string)value).Length > parameter.Size)
                throw new HibernateException("The length of the string value exceeds the length configured in the mapping/parameter.");
        }

        /// <summary>
        /// Converts from GeoAPI geometry type to database geometry type.
        /// </summary>
        /// <param name="value">The GeoAPI geometry value.</param>
        /// <returns></returns>
        protected override string FromGeometry(object value)
        {
            IGeometry geometry = value as IGeometry;
            if (geometry == null)
            {
                return null;
            }
            // PostGIS can't parse a WKB of any empty geometry other than GeomtryCollection
            // (throws the error: "geometry requires more points")
            // and parses WKT of empty geometries always as GeometryCollection
            // (ie. "select AsText(GeomFromText('LINESTRING EMPTY', -1)) = 'GEOMETRYCOLLECTION EMPTY'").
            // Force GeometryCollection.Empty to avoid the error.
            if (!(geometry is IGeometryCollection) && geometry.IsEmpty)
            {
                geometry = GeometryCollection.Empty;
            }

            this.SetDefaultSRID(geometry);

            var json = new GeoJsonWriter().Write(geometry);
            return json;
        }

        /// <summary>
        /// Converts to GeoAPI geometry type from database geometry type.
        /// </summary>
        /// <param name="value">The databse geometry value.</param>
        /// <returns></returns>
        protected override IGeometry ToGeometry(object value)
        {
            string bytes = value as string;

            if (string.IsNullOrEmpty(bytes))
            {
                return null;
            }

            // Bounding boxes are not serialized as hexadecimal string (?)
            const string boxToken = "BOX(";
            if (bytes.StartsWith(boxToken))
            {
                // TODO: Optimize?
                bytes = bytes.Substring(boxToken.Length, bytes.Length - boxToken.Length - 1);
                string[] parts = bytes.Split(',');
                string[] min = parts[0].Split(' ');
                string[] max = parts[1].Split(' ');
                string wkt = string.Format(
                    "POLYGON(({0} {1},{0} {3},{2} {3},{2} {1},{0} {1}))",
                    min[0], min[1], max[0], max[1]);
                return new WKTReader().Read(wkt);
            }

            var geometry = new GeoJsonReader().Read<IGeometry>(bytes);
            return geometry;
        }
    }
}