using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            double kEpsilon = 1e-6;
            // compute plane's normal
            Vector3 v0v1 = v1 - v0; 
            Vector3 v0v2 = v2 - v0; 
            // no need to normalize
            Vector3 N = v0v1.Cross(v0v2); // N 
            double area2 = N.Length(); 
        
            // Step 1: finding P
        
            // check if ray and plane are parallel ?
            double NdotRayDirection = N.Dot(ray.Direction); 
            if (Math.Abs(NdotRayDirection) < kEpsilon) // almost 0 
                return null; // they are parallel so they don't intersect ! 
        
            // compute d parameter using equation 2
            double d = N.Dot(v0); 
        
            // compute t (equation 3)
            //double t = (N.Dot(ray.Origin) + d) / NdotRayDirection;
            double t = (v0-ray.Origin).Dot(N)/ray.Direction.Dot(N);
            // check if the triangle is in behind the ray
            if (t < 0) return null; // the triangle is behind 
        
            // compute the intersection point using equation 1
            Vector3 P = ray.Origin + t * ray.Direction; 
        
            // Step 2: inside-outside test
            Vector3 C; // vector perpendicular to triangle's plane 
        
            // edge 0
            Vector3 edge0 = v1 - v0; 
            Vector3 vp0 = P - v0; 
            C = edge0.Cross(vp0); 
            if (N.Dot(C) < 0) return null; // P is on the right side 
        
            // edge 1
            Vector3 edge1 = v2 - v1; 
            Vector3 vp1 = P - v1; 
            C = edge1.Cross(vp1); 
            if (N.Dot(C) < 0)  return null; // P is on the right side 
        
            // edge 2
            Vector3 edge2 = v0 - v2; 
            Vector3 vp2 = P - v2; 
            C = edge2.Cross(vp2); 
            if (N.Dot(C) < 0) return null; // P is on the right side; 

            RayHit rayhit = new RayHit(P, N.Normalized(), ray.Direction, this.Material);
            return rayhit; // this ray hits the triangle 
        } 
        

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
