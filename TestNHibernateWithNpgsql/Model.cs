using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNHibernateWithNpgsql
{
    public class Model
    {
        public virtual long Id { get; set; }

        //public virtual IPoint Point
        //{
        //    get
        //    {
        //        return PointForWrite;
        //    }

        //    set
        //    {
        //        PointForWrite = value;
        //    }
        //}

        public virtual IPoint PointForWrite { get; set; }

        public virtual IPoint PointForRead { get; set; }

        public virtual GpsData GpsData { get; set; }
    }

    public class GpsData
    {
        public virtual DateTime DateTime { get; set; }

        public virtual IPoint Point
        {
            get
            {
                return PointForWrite;
            }

            set
            {
                PointForWrite = value;
            }
        }

        //public virtual IPoint PointForRead { get; set; }

        public virtual IPoint PointForWrite { get; set; }

        public virtual int Heading { get; set; }

        public virtual float Speed { get; set; }

        public virtual float? Altitude { get; set; }

        public virtual int Accuracy { get; set; }
    }
}
