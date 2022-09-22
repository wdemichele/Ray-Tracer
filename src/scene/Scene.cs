using System;
using System.Collections.Generic;
using System.Drawing;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        const double INFINITY = 9999999999;
        const int MAXDEPTH = 10;
        const double biasVal = 1e-4;
        const double Kd = 0.7; // phong model diffuse weight 
        const double Ks = 0.3; // phong model specular weight 
        const double n = 10;   // phong specular exponent

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            const double MAXVIEWANGLE = 30*Math.PI/180;
            
            double aspect_ratio = outputImage.Width/outputImage.Height;
            Vector3 origin = new Vector3(0,0,0);
            double AA = options.AAMultiplier;
            
            double AA2 = AA*AA;
            
            for (int i=0; i<outputImage.Width; i++) {
                for (int j=0; j<outputImage.Height; j++) {  
                    Color hitColor = new Color();
                    // create ray

                    //for (double dx = -(AA-1.0)/AA; dx <= (AA-1.0)/AA; dx++) {
                        //for (double dy = -(AA-1.0)/AA; dy <= (AA-1.0)/AA; dy++) {

                    for (double dx = 0; dx < AA; dx++) {
                        for (double dy = 0; dy < AA; dy++) {
                            double pixel_loc_x = (i+dx/AA)/(outputImage.Width);
                            double pixel_loc_y = (j+dy/AA)/(outputImage.Height);
                            double pixel_loc_z = 1.0f;
                            double x_pos = (pixel_loc_x * 2.0) - 1.0;
                            double y_pos = 1 - (pixel_loc_y * 2.0);
                            x_pos = x_pos * Math.Tan(MAXVIEWANGLE) * aspect_ratio;
                            y_pos = y_pos * Math.Tan(MAXVIEWANGLE);
                            Vector3 dir = new Vector3(x_pos,y_pos,pixel_loc_z);
                            dir = dir.Normalized();                    
                            Ray ray = new Ray(origin, dir);
                            hitColor += castRay(ray, 0)/AA2;
                        }
                    }



                    outputImage.SetPixel(i,j,hitColor);             
                }
            }
        }

        public Color castRay(Ray ray, int depth) {
            
            double tnear = INFINITY;
            Color hitColor = new Color();
            if (depth > MAXDEPTH) {
                return hitColor;
            }
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit != null)
                {
                    if (hit.Position.Length() < tnear && hit.Position.Length() > 0) {
                        // && (depth > 0|| hit.Position.Z > 1)
                        hitColor = new Color();
                        tnear = hit.Position.Length();
                        if (entity.Material.Type == Material.MaterialType.Diffuse) {
                            hitColor += diffusion(hit, entity);
                        }
                        
                        else if (entity.Material.Type == Material.MaterialType.Refractive)
                        { 
                            Color refractionColor = new Color(); 
                            // compute fresnel
                            double kr; 
                            kr = fresnel(hit, entity); 
                            bool outside = hit.Incident.Dot(hit.Normal) < 0; 
                            Vector3 bias = biasVal * hit.Normal; 
                            // compute refraction if it is not a case of total internal reflection
                            if (kr < 1) { 
                                Vector3 refractionDirection = refraction(hit, entity).Normalized(); 
                                Vector3 refractionRayOrig = outside ? hit.Position - bias : hit.Position + bias;
                                Ray refractionRay = new Ray(refractionRayOrig, refractionDirection); 
                                refractionColor = castRay(refractionRay, depth + 1);

                                // Beer's law

                                if ((hit.Incident.Dot(hit.Normal)) > 0) {

                                    Color inversedcolor = new Color(1.0f, 1.0f, 1.0f) - entity.Material.Color;
                                    Color abscolor = inversedcolor * entity.Material.RefractiveIndex * (-hit.Position);
                                    
                                    Color trans = new Color(Math.Exp(abscolor.R),Math.Exp(abscolor.G),Math.Exp(abscolor.B));
                                    refractionColor *= trans;
                                }
                            } 
                    
                            Vector3 reflectionDirection = reflection(hit).Normalized(); 
                            Vector3 reflectionRayOrig = outside ? hit.Position + bias : hit.Position - bias;
                            Ray reflectionRay = new Ray(reflectionRayOrig, reflectionDirection);
                            Color reflectionColor = castRay(reflectionRay, depth+1); 
                    
                            // mix the two
                            hitColor = reflectionColor * kr + refractionColor * (1 - kr); 
                            break; 
                        }
                        else if (entity.Material.Type == Material.MaterialType.Glossy) {
                            hitColor += Phong(hit, entity, depth);
                        }
                        else if (entity.Material.Type == Material.MaterialType.Reflective) {
                            Vector3 reflectionDirection = reflection(hit);
                            Ray reflectionRay = new Ray(hit.Position + biasVal*hit.Normal, reflectionDirection);
                            hitColor = castRay(reflectionRay, depth+1);
                        }
                        
                        else {
                            hitColor = entity.Material.Color;
                        }
                    }
                }
            }

            if (hitColor.R < 0) {
                hitColor = new Color(0,hitColor.G,hitColor.B);
            }
            if (hitColor.G < 0) {
                hitColor = new Color(hitColor.R,0,hitColor.B);
            }
            if (hitColor.B < 0) {
                hitColor = new Color(hitColor.R,hitColor.G,0);
            }
            return hitColor;
        }


        public Color diffusion(RayHit hit, SceneEntity entity) {
            Color hitColor = new Color();
            foreach (PointLight light in this.lights) {                                        
                Vector3 lightDirection = (light.Position - hit.Position).Normalized();

                Ray shadowRay = new Ray(hit.Position + hit.Normal * biasVal, lightDirection);
                bool inShadow = false;

                foreach (SceneEntity entity2 in this.entities) {
                    if (entity2 != entity) {
                        RayHit rayhit2 = entity2.Intersect(shadowRay);
                        if (rayhit2 != null) {
                            if ((rayhit2.Position - hit.Position).Length()  < (light.Position - hit.Position).Length()) {
                                inShadow = true;
                            }
                        }
                    }
                }
                if (!inShadow) {
                    Color color = (entity.Material.Color*light.Color)*(hit.Normal.Dot(lightDirection));
                    hitColor += color;
                }
            }
            return hitColor;
        }

        public Vector3 refraction(RayHit hit, SceneEntity entity) 
        { 
            double cosi = Math.Clamp(hit.Normal.Dot(hit.Incident), -1, 1); 
            double etai = 1, etat = entity.Material.RefractiveIndex; 
            Vector3 n = hit.Normal; 
            if (cosi < 0) { 
                cosi = -cosi; 
                } 
            else { 
                etai = etat;
                etat = 1.0;
                n= -hit.Normal; 
                } 
            double eta = etai / etat; 
            double k = 1 - eta * eta * (1 - cosi * cosi); 
            return k < 0 ? new Vector3(0,0,0) : eta * hit.Incident + (eta * cosi - Math.Sqrt(k)) * n; 
        } 
        public Vector3 reflection(RayHit hit){
            Vector3 reflected = hit.Incident - 2.0 * hit.Incident.Dot(hit.Normal) * hit.Normal;
            return reflected;
        }

        public double fresnel(RayHit hit, SceneEntity entity) 
        { 
            double kr;
            double cosi = Math.Clamp(hit.Incident.Dot(hit.Normal),-1, 1); 
            double etai = 1.0, etat = entity.Material.RefractiveIndex; 
            if (cosi > 0) { 
                etai = etat;
                etat = 1.0; 
                } 
            // Compute sini using Snell's law
            double sint = etai / etat * Math.Sqrt(Math.Max(0, 1 - cosi * cosi)); 
            // Total internal reflection
            if (sint >= 1) { 
                kr = 1; 
            } 
            else { 
                double cost = Math.Sqrt(Math.Max(0, 1 - sint * sint)); 
                cosi = Math.Abs(cosi); 
                double Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost)); 
                double Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost)); 
                kr = (Rs * Rs + Rp * Rp) / 2.0; 
            } 
            return kr;
            // As a consequence of the conservation of energy, transmittance is given by:
            // kt = 1 - kr;
        }
        public Color Phong(RayHit hit, SceneEntity entity, int depth)
        { 
            Color diffuse = new Color();
            Color specular = new Color(); 
            Color hitColor = new Color();
            double specVec = 0.0;
            foreach (PointLight light in this.lights) { 
                Vector3 lightDirection = (light.Position - hit.Position).Normalized();

                Ray shadowRay = new Ray(hit.Position + hit.Normal * biasVal, lightDirection);
                bool inShadow = false;

                foreach (SceneEntity entity2 in this.entities) {
                    if (entity2 != entity) {
                        RayHit rayhit2 = entity2.Intersect(shadowRay);
                        if (rayhit2 != null) {
                            if ((rayhit2.Position - hit.Position).Length()  < (light.Position - hit.Position).Length()) {
                                inShadow = true;
                            }
                        }
                    }
                }
                if (!inShadow) {
                    Vector3 R = reflection(hit); 
                    specVec += Math.Pow(Math.Max(0, R.Dot(-hit.Incident)), n)/2; 
                    Ray reflectionRay = new Ray(hit.Position + biasVal*hit.Normal, R);
                    specular += castRay(reflectionRay, depth++);
                }
            }            
            specular = new Color(specular.R + specVec, specular.G +specVec, specular.B +specVec);
            diffuse = diffusion(hit, entity); 
            hitColor = diffuse * Kd + specular * Ks; 
            return hitColor;
        }       

    }
}