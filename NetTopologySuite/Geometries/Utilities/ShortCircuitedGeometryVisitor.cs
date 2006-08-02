using System;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A visitor to Geometry elements which can
    /// be short-circuited by a given condition.
    /// </summary>
    public abstract class ShortCircuitedGeometryVisitor
    {
        private bool isDone = false;

        /// <summary>
        /// 
        /// </summary>
        public ShortCircuitedGeometryVisitor() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void ApplyTo(Geometry geom) 
        {
            for (int i = 0; i < geom.NumGeometries && ! isDone; i++) 
            {
                Geometry element = geom.GetGeometryN(i);
                if (!(element is GeometryCollection)) 
                {
                    Visit(element);
                    if (IsDone()) 
                    {
                        isDone = true;
                        return;
                    }
                }
                else ApplyTo(element);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        protected abstract void Visit(Geometry element);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsDone();
    }
}
