using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.UnitTests
{
    [TestFixture]
	public class CoordinateTransformTests : CoordinateTransformTestsBase
    {
        public CoordinateTransformTests()
        {
            Verbose = true;
        }

        [Test]
	    public void TestCentralMeridianParse()
	    {
            const string strSouthPole = "PROJCS[\"South_Pole_Lambert_Azimuthal_Equal_Area\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Lambert_Azimuthal_Equal_Area\"],PARAMETER[\"False_Easting\",0],PARAMETER[\"False_Northing\",0],PARAMETER[\"Central_Meridian\",-127],PARAMETER[\"Latitude_Of_Origin\",-90],UNIT[\"Meter\",1]]";

            var pCoordSysFactory = new CoordinateSystemFactory();
            var pSouthPole = pCoordSysFactory.CreateFromWkt(strSouthPole);
            Assert.IsNotNull(pSouthPole);
        }
        
        [Test]
		public void TestAlbersProjection()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("central_meridian", -96),
			                         new ProjectionParameter("latitude_of_center", 23),
			                         new ProjectionParameter("standard_parallel_1", 29.5),
			                         new ProjectionParameter("standard_parallel_2", 45.5),
			                         new ProjectionParameter("false_easting", 0),
			                         new ProjectionParameter("false_northing", 0)
			                     };
            var projection = CoordinateSystemFactory.CreateProjection("Albers Conical Equal Area", "albers", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			var trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new double[] { -75, 35 };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 1885472.7, 1535925 };
            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), TransformationError("Albers", expected, pUtm, false));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Albers", pGeo, pGeo2, true));
		}

		[Test]
		public void TestAlbersProjectionFeet()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("central_meridian", -96),
			                         new ProjectionParameter("latitude_of_center", 23),
			                         new ProjectionParameter("standard_parallel_1", 29.5),
			                         new ProjectionParameter("standard_parallel_2", 45.5),
			                         new ProjectionParameter("false_easting", 0),
			                         new ProjectionParameter("false_northing", 0)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Albers Conical Equal Area", "albers", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Foot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new double[] { -75, 35 };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 1885472.7 / LinearUnit.Foot.MetersPerUnit, 1535925 / LinearUnit.Foot.MetersPerUnit };
            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.1), TransformationError("Albers", expected, pUtm, false));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Albers", pGeo, pGeo2, true));
        }

		[Test]
		public void TestMercator_1SP_Projection()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 0),
			                         new ProjectionParameter("central_meridian", 110),
			                         new ProjectionParameter("scale_factor", 0.997),
			                         new ProjectionParameter("false_easting", 3900000),
			                         new ProjectionParameter("false_northing", 900000)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new double[] { 120, -3 };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 5009726.58, 569150.82 };
            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), TransformationError("Mercator_1SP", expected, pUtm, false));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Mercator_1SP", pGeo, pGeo2, true));
		}
		[Test]
		public void TestMercator_1SP_Projection_Feet()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 0),
			                         new ProjectionParameter("central_meridian", 110),
			                         new ProjectionParameter("scale_factor", 0.997),
			                         new ProjectionParameter("false_easting", 3900000/LinearUnit.Foot.MetersPerUnit),
			                         new ProjectionParameter("false_northing", 900000/LinearUnit.Foot.MetersPerUnit)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Foot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new[] { 120d, -3d };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 5009726.58 / LinearUnit.Foot.MetersPerUnit, 569150.82 / LinearUnit.Foot.MetersPerUnit };
            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), TransformationError("Mercator_1SP", expected, pUtm, false));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Mercator_1SP", pGeo, pGeo2, true));
		}
		[Test]
		public void TestMercator_2SP_Projection()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Krassowski 1940", 6378245.0, 298.3, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Krassowski 1940", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Krassowski 1940", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 42),
			                         new ProjectionParameter("central_meridian", 51),
			                         new ProjectionParameter("false_easting", 0),
			                         new ProjectionParameter("false_northing", 0)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Mercator_2SP", "Mercator_2SP", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("Pulkovo 1942 / Mercator Caspian Sea", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new[] { 53d, 53d };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 165704.29, 5171848.07 };
            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), TransformationError("Mercator_2SP", expected, pUtm, false));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Mercator_2SP", pGeo, pGeo2, true));
        }
		[Test]
		public void TestTransverseMercator_Projection()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Airy 1830", 6377563.396, 299.32496, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Airy 1830", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Airy 1830", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 49),
			                         new ProjectionParameter("central_meridian", -2),
			                         new ProjectionParameter("scale_factor", 0.9996012717 /* 0.9996*/),
			                         new ProjectionParameter("false_easting", 400000),
			                         new ProjectionParameter("false_northing", -100000)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("OSGB 1936 / British National Grid", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new[] { 0.5, 50.5 };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);
            //"POINT(577393.372775651 69673.621953601)"
            var expected = new[] { 577274.98, 69740.49 };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.01), TransformationError("TransverseMercator", expected, pUtm));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 1E-6), TransformationError("TransverseMercator", pGeo, pGeo2, true));
		}
		[Test]
		public void TestLambertConicConformal2SP_Projection()
		{
		    var ellipsoid = /*Ellipsoid.Clarke1866;*/
                CoordinateSystemFactory.CreateFlattenedSphere("Clarke 1866", 20925832.16, 294.97470, LinearUnit.USSurveyFoot);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 27.833333333),
			                         new ProjectionParameter("central_meridian", -99),
			                         new ProjectionParameter("standard_parallel_1", 28.3833333333),
			                         new ProjectionParameter("standard_parallel_2", 30.2833333333),
			                         new ProjectionParameter("false_easting", 2000000/LinearUnit.USSurveyFoot.MetersPerUnit),
			                         new ProjectionParameter("false_northing", 0)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Lambert Conic Conformal (2SP)", "lambert_conformal_conic_2sp", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("NAD27 / Texas South Central", gcs, projection, LinearUnit.USSurveyFoot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, coordsys);

			var pGeo = new[] { -96, 28.5 };
			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			var expected = new[] { 2963503.91 / LinearUnit.USSurveyFoot.MetersPerUnit, 254759.80 / LinearUnit.USSurveyFoot.MetersPerUnit };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), TransformationError("LambertConicConformal2SP", expected, pUtm));
		    Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("LambertConicConformal2SP", pGeo, pGeo2, true));

		}

		[Test]
		public void TestGeocentric()
		{
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("ETRF89 Geographic", AngularUnit.Degrees, HorizontalDatum.ETRF89, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
			var gcenCs = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("ETRF89 Geocentric", HorizontalDatum.ETRF89, LinearUnit.Metre, PrimeMeridian.Greenwich);
			var ct = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcs, gcenCs);
			var pExpected = new[] { 2 + 7.0 / 60 + 46.38 / 3600, 53 + 48.0 / 60 + 33.82/3600 }; // Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			var pExpected3D = new[] { pExpected[0], pExpected[1], 73.0 };
			var p0 = new[] { 3771793.97, 140253.34, 5124304.35 };
			var p1 = ct.MathTransform.Transform(pExpected3D);
			var p2 = ct.MathTransform.Inverse().Transform(p1);
			Assert.IsTrue(ToleranceLessThan(p1, p0, 0.01));
			Assert.IsTrue(ToleranceLessThan(p2, pExpected, 0.00001));
        }

		[Test]
		public void TestDatumTransform()
		{
			//Define datums, set parameters
            var wgs72 = HorizontalDatum.WGS72;
            wgs72.Wgs84Parameters = new Wgs84ConversionInfo(0, 0, 4.5, 0, 0, 0.554, 0.219);
            var ed50 = HorizontalDatum.ED50;
            ed50.Wgs84Parameters = new Wgs84ConversionInfo(-81.0703, -89.3603, -115.7526,
                                                           -0.48488, -0.02436, -0.41321,
                                                           -0.540645); //Parameters for Denmark
			//Define geographic coordinate systems
			var gcsWGS72 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("WGS72 Geographic", AngularUnit.Degrees, wgs72, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			var gcsWGS84 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("WGS84 Geographic", AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			var gcsED50 = CoordinateSystemFactory.CreateGeographicCoordinateSystem("ED50 Geographic", AngularUnit.Degrees, ed50, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			//Define geocentric coordinate systems
			var gcenCsWGS72 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("WGS72 Geocentric", wgs72, LinearUnit.Metre, PrimeMeridian.Greenwich);
			var gcenCsWGS84 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("WGS84 Geocentric", HorizontalDatum.WGS84, LinearUnit.Metre, PrimeMeridian.Greenwich);
			var gcenCsED50 = CoordinateSystemFactory.CreateGeocentricCoordinateSystem("ED50 Geocentric", ed50, LinearUnit.Metre, PrimeMeridian.Greenwich);

			//Define projections
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_origin", 0),
			                         new ProjectionParameter("central_meridian", 9),
			                         new ProjectionParameter("scale_factor", 0.9996),
			                         new ProjectionParameter("false_easting", 500000),
			                         new ProjectionParameter("false_northing", 0)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);
			var utmED50 = CoordinateSystemFactory.CreateProjectedCoordinateSystem("ED50 UTM Zone 32N", gcsED50, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
			var utmWGS84 = CoordinateSystemFactory.CreateProjectedCoordinateSystem("WGS84 UTM Zone 32N", gcsWGS84, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

            ////Set up coordinate transformations
            //var ctForw = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsWGS72, gcenCsWGS72); //Geographic->Geocentric (WGS72)
            //var ctWGS84_Gcen2Geo = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcenCsWGS84, gcsWGS84);  //Geocentric->Geographic (WGS84)
            //var ctWGS84_Geo2UTM = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsWGS84, utmWGS84);  //UTM ->Geographic (WGS84)
            //var ctED50_UTM2Geo = _coordinateTransformationFactory.CreateFromCoordinateSystems(utmED50, gcsED50);  //UTM ->Geographic (ED50)
            //var ctED50_Geo2Gcen = _coordinateTransformationFactory.CreateFromCoordinateSystems(gcsED50, gcenCsED50); //Geographic->Geocentric (ED50)

			//Test datum-shift from WGS72 to WGS84
			//Point3D pGeoCenWGS72 = ctForw.MathTransform.Transform(pLongLatWGS72) as Point3D;
			var pGeoCenWGS72 = new[] {3657660.66, 255768.55, 5201382.11};
			var geocen_ed50_2_Wgs84 = CoordinateTransformationFactory.CreateFromCoordinateSystems(gcenCsWGS72, gcenCsWGS84);
			var pGeoCenWGS84 = geocen_ed50_2_Wgs84.MathTransform.Transform(pGeoCenWGS72);
			//Point3D pGeoCenWGS84 = wgs72.Wgs84Parameters.Apply(pGeoCenWGS72);
		    var pExpected = new[] {3657660.78, 255778.43, 5201387.75};
            Assert.IsTrue(ToleranceLessThan(pExpected, pGeoCenWGS84, 0.01), TransformationError("Datum WGS72->WGS84", pExpected, pGeoCenWGS84));

			var utm_ed50_2_Wgs84 = CoordinateTransformationFactory.CreateFromCoordinateSystems(utmED50, utmWGS84);
			var pUTMED50 = new double[] {600000, 6100000};
			var pUTMWGS84 = utm_ed50_2_Wgs84.MathTransform.Transform(pUTMED50);
            pExpected = new[] { 599928.6, 6099790.2};
            Assert.IsTrue(ToleranceLessThan(pExpected, pUTMWGS84, 0.1), TransformationError("Datum ED50->WGS84", pExpected, pUTMWGS84));
			//Perform reverse
			ICoordinateTransformation utm_Wgs84_2_Ed50 = CoordinateTransformationFactory.CreateFromCoordinateSystems(utmWGS84, utmED50);
			pUTMED50 = utm_Wgs84_2_Ed50.MathTransform.Transform(pUTMWGS84);
		    pExpected = new double[] {600000, 6100000};
            Assert.IsTrue(ToleranceLessThan(pExpected, pUTMED50, 0.1), TransformationError("Datum", pExpected, pUTMED50));
			//Assert.IsTrue(Math.Abs((pUTMWGS84 as Point3D).Z - 36.35) < 0.5);
			//Point pExpected = Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			//ED50_to_WGS84_Denmark: datum.Wgs84Parameters = new Wgs84ConversionInfo(-89.5, -93.8, 127.6, 0, 0, 4.5, 1.2);

		}

		[Test]
		public void TestKrovak_Projection()
		{
			var ellipsoid = CoordinateSystemFactory.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			var datum = CoordinateSystemFactory.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			var gcs = CoordinateSystemFactory.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			var parameters = new List<ProjectionParameter>(5)
			                     {
			                         new ProjectionParameter("latitude_of_center", 49.5),
			                         new ProjectionParameter("longitude_of_center", 42.5),
			                         new ProjectionParameter("azimuth", 30.28813972222222),
			                         new ProjectionParameter("pseudo_standard_parallel_1", 78.5),
			                         new ProjectionParameter("scale_factor", 0.9999),
			                         new ProjectionParameter("false_easting", 0),
			                         new ProjectionParameter("false_northing", 0)
			                     };
		    var projection = CoordinateSystemFactory.CreateProjection("Krovak", "Krovak", parameters);

			var coordsys = CoordinateSystemFactory.CreateProjectedCoordinateSystem("WGS 84", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			var trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			// test case 1
			var pGeo = new[] { 12d, 48d };
			var expected = new[] { -953172.26, -1245573.32 };

			var pUtm = trans.MathTransform.Transform(pGeo);
			var pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

		    Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), TransformationError("Krovak", expected, pUtm));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Krovak", pGeo, pGeo2, true));

			// test case 2
			pGeo = new double[] { 18, 49 };
			expected = new double[] { -499258.06, -1192389.16 };

			pUtm = trans.MathTransform.Transform(pGeo);
			pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), TransformationError("Krovak", expected, pUtm));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), TransformationError("Krovak", pGeo, pGeo2));
		} 

	    [Test]
        public void TestUnitTransforms()
        {
			var nadUTM = SRIDReader.GetCSbyID(2868); //UTM Arizona Central State Plane using Feet as units
			var wgs84GCS = SRIDReader.GetCSbyID(4326); //GCS WGS84
			var trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84GCS, nadUTM);

			var p0 = new[] { -111.89, 34.165 };
            //var expected = new[] { 708066.19058, 1151461.51413 };
            var expected = new[] { 708066.19057935325, 1151426.4460563776 };

			
			var p1 = trans.MathTransform.Transform(p0);
			var p2 = trans.MathTransform.Inverse().Transform(p1);

			Assert.IsTrue(ToleranceLessThan(p1, expected, 0.013), TransformationError("Unit", expected, p1));
			//WARNING: This accuracy is too poor!
            Assert.IsTrue(ToleranceLessThan(p0, p2, 0.0001), TransformationError("Unit", expected, p1, true));
        }

        [Test, Description("Accuracy very poor!")]
        public void TestPolyconicTransforms()
        {
            var wgs84GCS = SRIDReader.GetCSbyID(4326); //GCS WGS84
            var wkt =
                //"PROJCS[\"SAD69 / Brazil Polyconic (deprecated)\",GEOGCS[\"SAD69\",DATUM[\"South_American_Datum_1969\",SPHEROID[\"GRS 1967\",6378160,298.247167427,AUTHORITY[\"EPSG\",\"7036\"]],TOWGS84[-57,1,-41,0,0,0,0],AUTHORITY[\"EPSG\",\"6291\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4291\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Polyconic\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",-54],PARAMETER[\"false_easting\",5000000],PARAMETER[\"false_northing\",10000000],AUTHORITY[\"EPSG\",\"29100\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]";
                //"PROJCS[\"SAD69 / Brazil Polyconic\",GEOGCS[\"SAD69\",DATUM[\"South_American_Datum_1969\",SPHEROID[\"GRS 1967 Modified\",6378160,298.25,AUTHORITY[\"EPSG\",\"7050\"]],TOWGS84[-57,1,-41,0,0,0,0],AUTHORITY[\"EPSG\",\"6618\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4618\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Polyconic\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",-54],PARAMETER[\"false_easting\",5000000],PARAMETER[\"false_northing\",10000000],AUTHORITY[\"EPSG\",\"29101\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]";
                  "PROJCS[\"SAD69 / Brazil Polyconic\",GEOGCS[\"SAD69\",DATUM[\"South_American_Datum_1969\",SPHEROID[\"GRS 1967 (SAD69)\", 6378160, 298.25, AUTHORITY[\"EPSG\", \"7050\"]],AUTHORITY[\"EPSG\", \"6618\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]],UNIT[\"degree\", 0.01745329251994328, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4618\"]],UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], PROJECTION[\"Polyconic\"],PARAMETER[\"latitude_of_origin\", 0], PARAMETER[\"central_meridian\", -54],PARAMETER[\"false_easting\", 5000000], PARAMETER[\"false_northing\", 10000000],AUTHORITY[\"EPSG\", \"29101\"], AXIS[\"X\", EAST], AXIS[\"Y\", NORTH]]";
                var sad69 = CoordinateSystemFactory.CreateFromWkt(wkt);

            var trans = CoordinateTransformationFactory.CreateFromCoordinateSystems(wgs84GCS, sad69);
            var p0 = new[] { -50.085, -14.32 };
            var expected = new[] { 5422386.5795,    8412674.8723 };
                          //"POINT(5422386.57956145 8412722.92229278)"
            var p1 = trans.MathTransform.Transform(p0);
            trans.MathTransform.Invert();
            var p2 = trans.MathTransform.Transform(p1);

            Assert.IsTrue(ToleranceLessThan(p1, expected, 50), TransformationError("Polyconic", expected, p1));
            Assert.IsTrue(ToleranceLessThan(p0, p2, 0.0001), TransformationError("Polyconic", expected, p1, true));
        }

        [Test]
        public void TestCassiniSoldner()
        {
            var csSource = GeographicCoordinateSystem.WGS84;
            var csTarget = CoordinateSystemFactory.CreateFromWkt(
                "PROJCS[\"DHDN / Soldner Berlin\",GEOGCS[\"DHDN\",DATUM[\"Deutsches_Hauptdreiecksnetz\",SPHEROID[\"Bessel 1841\",6377397.155,299.1528128,AUTHORITY[\"EPSG\",\"7004\"]],TOWGS84[598.1,73.7,418.2,0.202,0.045,-2.455,6.7],AUTHORITY[\"EPSG\",\"6314\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4314\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Cassini_Soldner\"],PARAMETER[\"latitude_of_origin\",52.41864827777778],PARAMETER[\"central_meridian\",13.62720366666667],PARAMETER[\"false_easting\",40000],PARAMETER[\"false_northing\",10000],AUTHORITY[\"EPSG\",\"3068\"],AXIS[\"x\",NORTH],AXIS[\"y\",EAST]]");

            Test("CassiniSoldner", csSource, csTarget, 
                 new[] { 13.408055555556, 52.518611111111 }, 
                 new[] { 25244.540, 21300.969 }, 0.3, 1.0E-5);

            /*
            var ct = CoordinateTransformationFactory.CreateFromCoordinateSystems(csSource, csTarget);
            var pgeo = new[] {13.408055555556, 52.518611111111};
            var pcs = ct.MathTransform.Transform(pgeo);
            
            //Evaluated using DotSpatial.Projections
            var pcsExpected = new[] {25244.540, 21300.969};

            Assert.IsTrue(ToleranceLessThan(pcsExpected, pcs, 0.3), TransformationError("CassiniSoldner", pcsExpected, pcs));
            var pgeo2 = ct.MathTransform.Inverse().Transform(pcs);
            Assert.IsTrue(ToleranceLessThan(pgeo, pgeo2, 1.0E-5), TransformationError("CassiniSoldner", pgeo, pgeo2));
             */
        }

        [Test]
        public void TestHotineObliqueMercator()
        {
            var csSource = GeographicCoordinateSystem.WGS84;
            var csTarget = CoordinateSystemFactory.CreateFromWkt(
               "PROJCS[\"NAD83(NSRS2007) / Alaska zone 1\",GEOGCS[\"NAD83(NSRS2007)\",DATUM[\"NAD83_National_Spatial_Reference_System_2007\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6759\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4759\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Hotine_Oblique_Mercator\"],PARAMETER[\"latitude_of_center\",57],PARAMETER[\"longitude_of_center\",-133.6666666666667],PARAMETER[\"azimuth\",323.1301023611111],PARAMETER[\"rectified_grid_angle\",323.1301023611111],PARAMETER[\"scale_factor\",0.9999],PARAMETER[\"false_easting\",5000000],PARAMETER[\"false_northing\",-5000000],AUTHORITY[\"EPSG\",\"3468\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]");
            //61.216667�, -149.883333�
            //"POINT(4136805.82642057 -4424019.78560519)"
            Test("HotineObliqueMercator", csSource, csTarget,
                 new[] { -149.883333, 61.216667 },
                 new[] { 4136805.826, -4424019.786 }, 0.01, 1.0E-5);

        }
    }
}
