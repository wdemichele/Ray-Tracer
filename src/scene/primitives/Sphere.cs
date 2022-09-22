using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        { 
            double t0, t1; // solutions for t if the ray intersects 

            // geometric solution
            Vector3 L = this.center - ray.Origin; 
            double tca = L.Dot(ray.Direction); 
            // if (tca < 0) return false;
            double d2 = L.Dot(L) - tca * tca; 
            if (d2 > this.radius*this.radius) return null; 
            double thc = Math.Sqrt(this.radius*this.radius - d2); 
            t0 = tca - thc; 
            t1 = tca + thc; 

            if (t0 > t1) {
            double temp = t0;
            t0 = t1;
            t1 = temp; 
            }
    
            if (t0 < 0.0) { 
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 < 0) return null; // both t0 and t1 are negative 
            } 
    
            double t = t0;

            Vector3 position = ray.Origin + t*ray.Direction;

            Vector3 normal = (position-this.center).Normalized();

            RayHit rayhit = new RayHit(position, normal, ray.Direction, this.Material);
            return rayhit;
        }
        public bool solveQuadratic(double a, double b, double c, double x0, double x1) 
        { 
            double discr = b * b - 4 * a * c; 
            if (discr < 0) return false; 
            else if (discr == 0) x0 = x1 = - 0.5 * b / a; 
            else { 
                double q = (b > 0) ? 
                    -0.5 * (b + Math.Sqrt(discr)) : 
                    -0.5 * (b - Math.Sqrt(discr)); 
                x0 = q / a; 
                x1 = c / q; 
            } 
            if (x0 > x1) {
                double temp = x0;
                x0 = x1;
                x1 = temp; 
            }        
            return true; 
        } 

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }

        public double Radius { get { return this.radius; } }
    }

}
