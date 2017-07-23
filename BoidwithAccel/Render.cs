using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using System.Collections;
using Windows.UI;
using Windows.Foundation;

namespace App1 {

    class Render {
        public Flock flock;
        public float roll;
        Random rnd;

        public Render(CanvasAnimatedControl sender) {
            flock = new Flock();
            rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 150; i++) {
                float t = rnd.Next(1000);
                flock.addBoid(new Boid(420, 250, t));
            }
        }

        public void draw(ICanvasAnimatedControl sender, CanvasDrawingSession canvas) {
            Color paint = Color.FromArgb(255, 50, 50, 50);
            Color paint2 = Color.FromArgb(255, 255, 255, 255);
            Rect rect = new Rect(0, 0, 840, 500);
            canvas.FillRectangle(rect, paint);
            canvas.DrawLine(0, 100, 840, 100, paint2);
            flock.run(roll, canvas);
        }

        public class Flock {
            ArrayList boids;

            public Flock() {
                boids = new ArrayList();
            }

            public void run(float x, CanvasDrawingSession canvas) {
                foreach (Boid b in boids) {
                    b.run(boids, x, canvas);
                }
            }

            public void addBoid(Boid b) {
                boids.Add(b);
            }
        }

        public class Boid {
            Vector2 position;
            Vector2 velocity;
            Vector2 acceleration;
            float r;
            float maxforce;    // Maximum steering force
            float maxspeed;    // Maximum spee
            float angle;
            float Width = 840;
            float Height = 500;

            public Boid(float x, float y, float t) {
                acceleration = new Vector2(0, 0);
                angle = t / 1000.0f * (float)Math.PI * 2.0f;
                velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                position = new Vector2(x, y);

                r = 2.0f;
                maxspeed = 1.5f;
                maxforce = 0.03f;
            }

            public void run(ArrayList boids, float x, CanvasDrawingSession canvas) {
                flock(boids);
                update(x, canvas);
                borders();
                render(canvas);
            }

            public void applyForce(Vector2 force) {
                acceleration = Vector2.Add(acceleration, force);
            }

            public void flock(ArrayList boids) {
                Vector2 sep = separate(boids);
                Vector2 ali = align(boids);
                Vector2 coh = cohesion(boids);

                sep = Vector2.Multiply(1.5f, sep);
                ali = Vector2.Multiply(1.0f, ali);
                coh = Vector2.Multiply(1.0f, coh);

                applyForce(sep);
                applyForce(ali);
                applyForce(coh);
            }

            public void update(float x, CanvasDrawingSession c) {
                velocity = Vector2.Add(velocity, acceleration); 
                if (velocity.Length() > maxspeed) {
                    velocity = Vector2.Normalize(velocity);
                    velocity = Vector2.Multiply(maxspeed, velocity);
                }
                position = Vector2.Add(position, velocity);
                position = Vector2.Add(position, new Vector2(x*5, 0));
                acceleration = Vector2.Multiply(0, acceleration);
            }

            Vector2 seek(Vector2 target) {
                Vector2 desired = Vector2.Subtract(target, position);

                desired = Vector2.Normalize(desired);
                desired = Vector2.Multiply(maxspeed, desired);

                Vector2 steer = Vector2.Subtract(desired, velocity);
                if (steer.Length() > maxforce) {
                    steer = Vector2.Normalize(steer);
                    steer = Vector2.Multiply(maxforce, steer);
                }
                return steer;
            }

            public void render(CanvasDrawingSession c) {
                Color paint = Color.FromArgb(255, 255, 255, 255);
                c.DrawCircle(position.X, position.Y, r, paint);
            }

            public void borders() {
                if (position.X < -r)
                    position.X = Width + r;
                if (position.X > Width + r)
                    position.X = -r;
                if (position.Y < 100) {
                    position.Y = 100;
                    velocity.Y = velocity.Y * (-1);
                }
                if (position.Y > Height) {
                    position.Y = Height;
                    velocity.Y = velocity.Y * (-1);
                }
            }

            Vector2 separate(ArrayList boids) {
                float desiredseparation = 250.0f;
                Vector2 steer = new Vector2(0, 0);
                float count = 0.0f;

                foreach (Boid other in boids) {
                    float d = dist(position, other.position);

                    if ((d > 0) && (d < desiredseparation)) {
                        Vector2 diff = Vector2.Subtract(other.position, position);
                        diff = Vector2.Normalize(diff);
                        diff = Vector2.Divide(diff, d);
                        steer = Vector2.Add(steer, diff);
                        count += 20;
                    }
                }

                if (count > 0) {
                    steer = Vector2.Divide(steer, count);
                }

                if (steer.Length() > 0) {
                    steer = Vector2.Normalize(steer);
                    steer = Vector2.Multiply(maxspeed, steer);
                    steer = Vector2.Subtract(velocity, steer);
                    if (steer.Length() > maxforce) {
                        steer = Vector2.Normalize(steer);
                        steer = Vector2.Multiply(maxforce, steer);
                    }
                }
                return steer;
            }

            Vector2 align(ArrayList boids) {
                float neighbordist = 500;
                Vector2 sum = new Vector2(0, 0);
                int count = 0;
                foreach (Boid other in boids) {
                    float d = dist(position, other.position);
                    if ((d > 0) && (d < neighbordist)) {
                        sum = Vector2.Add(sum, other.velocity);
                        count++;
                    }
                }
                if (count > 0) {
                    sum = Vector2.Divide(sum, count);
                    sum = Vector2.Normalize(sum);
                    sum = Vector2.Multiply(maxspeed, sum);
                    Vector2 steer = Vector2.Subtract(sum, velocity);
                    if (steer.Length() > maxforce) {
                        steer = Vector2.Normalize(steer);
                        steer = Vector2.Multiply(maxforce, steer);
                    }

                    return steer;
                } else {
                    return new Vector2(0, 0);
                }
            }

            Vector2 cohesion(ArrayList boids) {
                float neighbordist = 50;
                Vector2 sum = new Vector2(0, 0);
                int count = 0;
                foreach (Boid other in boids) {
                    float d = dist(position, other.position);
                    if ((d > 0) && (d < neighbordist)) {
                        sum = Vector2.Add(sum, other.position);
                        count++;
                    }
                }
                if (count > 0) {
                    sum = Vector2.Divide(sum, count);
                    return seek(sum);
                } else {
                    return new Vector2(0, 0);
                }
            }

            public float dist(Vector2 v1, Vector2 v2) {
                return (float)(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
            }
        }
    }
}