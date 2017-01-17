﻿using DemoCore;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Media3D = System.Windows.Media.Media3D;

namespace OctreeDemo
{
    public class DataModel : ObservableObject
    {
        private MeshGeometry3D model = null;
        public MeshGeometry3D Model
        {
            set
            {
                SetValue<MeshGeometry3D>(ref model, value, nameof(Model));
            }
            get { return model; }
        }
        private Material orgMaterial;
        private Material material;
        public Material Material
        {
            set
            {
                SetValue<Material>(ref material, value, nameof(Material));
            }
            get
            {
                return material;
            }
        }

        private bool highlight = false;
        public bool Highlight
        {
            set
            {
                if (highlight == value) { return; }
                highlight = value;
                if (highlight)
                {
                    orgMaterial = material;
                    Material = PhongMaterials.Yellow;
                }
                else
                {
                    Material = orgMaterial;
                }
            }
            get
            {
                return highlight;
            }
        }

        public DataModel()
        {
            Material = PhongMaterials.Red;
        }
    }

    public class SphereModel : DataModel
    {
        private static readonly Random rnd = new Random();
        public SphereModel(Vector3 center, int radius, bool enableTransform = true)
        {
            Center = center;
            Radius = radius;
            CreateModel();
            isConstructed = true;
            if (enableTransform)
            {
                DynamicTransform = CreateAnimatedTransform1
                  (new Media3D.Vector3D(rnd.Next(-2, 2), rnd.Next(-2, 2), rnd.Next(-2, 2)),
                  new Media3D.Vector3D(rnd.Next(-1, 1), rnd.Next(-1, 1), rnd.Next(-1, 1)), center.ToVector3D(), rnd.Next(10, 100));
            }
            var color = rnd.NextColor();
            Material = new PhongMaterial() { DiffuseColor = color.ToColor4() };
        }

        private bool isConstructed = false;

        private Vector3 center;
        public Vector3 Center
        {
            set
            {
                if (SetValue<Vector3>(ref center, value, nameof(Center)))
                {
                    if (!isConstructed)
                    {
                        return;
                    }
                    CreateModel();
                }
            }
            get
            {
                return center;
            }
        }

        private int radius;
        public int Radius
        {
            set
            {
                if (SetValue<int>(ref radius, value, nameof(Radius)))
                {
                    if (!isConstructed)
                    {
                        return;
                    }
                    CreateModel();
                }
            }
            get
            {
                return radius;
            }
        }

        public Media3D.Transform3D DynamicTransform { get; private set; }

        private void CreateModel()
        {
            var builder = new MeshBuilder(true, false, false);
            int type = rnd.Next(0, 3);
            var center = Center;
            switch (type)
            {
                case 0:
                    builder.AddSphere(center, Radius, 12, 12);
                    break;
                case 1:
                    builder.AddBox(center, Radius, Radius, Radius);
                    break;
                case 2:
                    builder.AddPyramid(center, Radius, Radius, true);
                    break;
                case 3:
                    builder.AddPipe(center, center + new Vector3(0, 1, 0), 0, Radius*2, 12);
                    break;
            }
            this.Model = builder.ToMeshGeometry3D();
            //this.Model.UpdateOctree();
        }

        private static Media3D.Transform3D CreateAnimatedTransform1(Media3D.Vector3D translate, Media3D.Vector3D axis, Media3D.Vector3D center, double speed = 4)
        {
            var lightTrafo = new Media3D.Transform3DGroup();
            // lightTrafo.Children.Add(new Media3D.TranslateTransform3D(translate));

            var rotateAnimation = new Rotation3DAnimation
            {
                RepeatBehavior = RepeatBehavior.Forever,
                By = new Media3D.AxisAngleRotation3D(axis, 90),
                Duration = TimeSpan.FromSeconds(speed / 2),
                IsCumulative = true,
            };

            var rotateTransform = new Media3D.RotateTransform3D();
            rotateTransform.BeginAnimation(Media3D.RotateTransform3D.RotationProperty, rotateAnimation);

            lightTrafo.Children.Add(rotateTransform);

            var rotateAnimation1 = new Rotation3DAnimation
            {
                RepeatBehavior = RepeatBehavior.Forever,
                By = new Media3D.AxisAngleRotation3D(axis, 240),
                Duration = TimeSpan.FromSeconds(speed / 4),
                IsCumulative = true,
            };

            var rotateTransform1 = new Media3D.RotateTransform3D();
            rotateTransform1.CenterX = center.X;
            rotateTransform1.CenterY = center.Y;
            rotateTransform1.CenterZ = center.Z;
            rotateTransform1.BeginAnimation(Media3D.RotateTransform3D.RotationProperty, rotateAnimation1);

            lightTrafo.Children.Add(rotateTransform1);

            return lightTrafo;
        }
    }
}