using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Poligonize
{
    /// <summary>  
    /// Example of using Polygonizer class to polygonize a set of fully noded linestrings.
    /// </summary>	
    public class PolygonizeExample
    {
        [STAThread]
        public static void main(String[] args)
        {
            PolygonizeExample test = new PolygonizeExample();
            try
            {
                test.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }


        internal virtual void Run()
        {
            IGeometryFactory<BufferedCoordinate2D> geoFactory =
                new GeometryFactory<BufferedCoordinate2D>(new BufferedCoordinate2DSequenceFactory());
            WktReader<BufferedCoordinate2D> rdr
                = new WktReader<BufferedCoordinate2D>(geoFactory, null);
            List<IGeometry<BufferedCoordinate2D>> lines
                = new List<IGeometry<BufferedCoordinate2D>>();

            // isolated edge
            lines.Add(rdr.Read("LINESTRING (0 0 , 10 10)"));
            lines.Add(rdr.Read("LINESTRING (185 221, 100 100)")); //dangling edge
            lines.Add(rdr.Read("LINESTRING (185 221, 88 275, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (185 221, 292 281, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 83 187, 185 221)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 325 168, 185 221)"));

            Polygonizer<BufferedCoordinate2D> polygonizer
                = new Polygonizer<BufferedCoordinate2D>();
            polygonizer.Add(lines);

            IList<IPolygon<BufferedCoordinate2D>> polys = polygonizer.Polygons;

            Console.WriteLine("Polygons formed (" + polys.Count + "):");
            foreach (object obj in polys)
            {
                Console.WriteLine(obj);
            }
        }
    }
}