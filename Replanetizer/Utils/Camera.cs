// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Drawing.Text;
using LibReplanetizer.LevelObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Replanetizer.Utils
{
    public class Camera : ITransformable
    {
        public class Frustum
        {
            public Vector3[] planePoints = new Vector3[6];
            public Vector3[] planeNormals = new Vector3[6];
        }

        //Camera variables
        public float speed = 0.2f;
        private Vector3 _position = new Vector3();
        public Vector3 position { get { return _position; } set { matrixDirty = true; frustumDirty = true; _position = value; } }
        private Vector3 _rotation = new Vector3(0, 0, -0.75f);
        public Vector3 rotation { get { return _rotation; } set { matrixDirty = true; frustumDirty = true; _rotation = value; } }

        private float _fovy = 0.87f;
        public float fovy { get { return _fovy; } set { matrixDirty = true; frustumDirty = true; _fovy = value; } }
        private float _aspect = (float) 16 / 9;
        public float aspect { get { return _aspect; } set { matrixDirty = true; frustumDirty = true; _aspect = value; } }
        private float _near = 0.1f;
        public float near { get { return _near; } set { matrixDirty = true; frustumDirty = true; _near = value; } }
        private float _far = 1024.0f;
        public float far { get { return _far; } set { matrixDirty = true; frustumDirty = true; _far = value; } }

        /// <summary>
        /// The frustum is defined by 6 planes which are each defined by a point and a normal.
        /// The order of the planes is Near, Far, Right, Left, Up, Down.
        /// </summary>
        private bool frustumDirty = true;
        private Frustum frustum = new Frustum();

        private bool matrixDirty = true;
        private Matrix4 projectionMatrix = new Matrix4();
        private Matrix4 viewMatrix = new Matrix4();
        private Matrix4 worldViewMatrix = new Matrix4();

        private bool orbitActive;
        private Vector3 orbitPivot;
        private Vector3 orbitReference;
        private bool zoomActive;
        private Vector3 zoomPivot;
        private Vector3 zoomReference;

        private Matrix3 GetRotationMatrix()
        {
            return Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y) * Matrix3.CreateRotationZ(rotation.Z);
        }

        private static Vector3 LegacyTransform(Vector3 vec, Matrix3 mat)
        {
            Vector3 result = new Vector3();
            result.X = vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X;
            result.Y = vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y;
            result.Z = vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z;
            return result;
        }

        public Vector3 GetForward()
        {
            Vector3 forward = LegacyTransform(Vector3.UnitY, GetRotationMatrix());
            forward.Normalize();
            return forward;
        }

        public Vector3 TransformDirection(Vector3 direction, Vector3 sourceRotation, Vector3 destinationRotation)
        {
            Matrix3 sourceMatrix = Matrix3.CreateRotationX(sourceRotation.X) *
                Matrix3.CreateRotationY(sourceRotation.Y) *
                Matrix3.CreateRotationZ(sourceRotation.Z);
            Matrix3 destinationMatrix = Matrix3.CreateRotationX(destinationRotation.X) *
                Matrix3.CreateRotationY(destinationRotation.Y) *
                Matrix3.CreateRotationZ(destinationRotation.Z);

            Vector3 localDirection = LegacyTransform(direction, Matrix3.Invert(sourceMatrix));
            return LegacyTransform(localDirection, destinationMatrix);
        }

        public bool BeginOrbit(Vector3 clickedWorldPoint)
        {
            Vector3 reference = clickedWorldPoint - position;
            if (reference.Length <= near)
                return false;

            orbitPivot = clickedWorldPoint;
            orbitReference = reference;
            orbitActive = true;
            return true;
        }

        public void UpdateOrbit(Vector2 rotationDelta)
        {
            if (!orbitActive)
                return;

            Vector3 previousRotation = rotation;
            Rotate(rotationDelta);
            orbitReference = TransformDirection(orbitReference, previousRotation, rotation);
            position = orbitPivot - orbitReference;
        }

        public void EndOrbit()
        {
            orbitActive = false;
        }

        public bool BeginZoom(Vector3 clickedWorldPoint)
        {
            Vector3 reference = clickedWorldPoint - position;
            if (reference.Length <= near)
                return false;

            zoomPivot = clickedWorldPoint;
            zoomReference = reference;
            zoomActive = true;
            return true;
        }

        public void UpdateZoom(float mouseDeltaY, float zoomSpeed)
        {
            if (!zoomActive)
                return;

            float distance = zoomReference.Length;
            float zoomAmount = -mouseDeltaY * zoomSpeed;
            float newDistance = MathHelper.Clamp(distance * (1.0f - zoomAmount), near, far);
            Vector3 movement = zoomReference.Normalized() * (distance - newDistance);

            position += movement;
            zoomReference -= movement;
        }

        public void EndZoom()
        {
            zoomActive = false;
        }

        public void CancelNavigation()
        {
            orbitActive = false;
            zoomActive = false;
        }

        private void UpdateMatrices()
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, near, far);

            Vector3 forward = GetForward();
            viewMatrix = Matrix4.LookAt(position, position + forward, Vector3.UnitZ);

            worldViewMatrix = viewMatrix * projectionMatrix;
        }

        public Matrix4 GetViewMatrix()
        {
            if (matrixDirty)
            {
                UpdateMatrices();
                matrixDirty = false;
            }

            return viewMatrix;
        }

        public Matrix4 GetProjectionMatrix()
        {
            if (matrixDirty)
            {
                UpdateMatrices();
                matrixDirty = false;
            }

            return projectionMatrix;
        }

        public Matrix4 GetWorldViewMatrix()
        {
            if (matrixDirty)
            {
                UpdateMatrices();
                matrixDirty = false;
            }

            return worldViewMatrix;
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetRotation(Vector3 rotation)
        {
            this.rotation = rotation;
        }

        public void SetRotation(float pitch, float yaw)
        {
            SetRotation(new Vector3(pitch, 0, yaw));
        }

        public void MoveBehind(LevelObject levelObject, float distanceToObject = 5)
        {
            if (levelObject == null) return;

            float yaw = 0;

            // If object is moby, load its rotation.
            if (levelObject is Moby moby)
            {
                yaw = moby.rotation.Z;
                if (double.IsNaN(yaw)) yaw = 0;
            }

            yaw = yaw - (float) Math.PI / 2;
            SetRotation(-(float) Math.PI / 10, yaw);

            float ypos = (float) -Math.Cos(yaw);
            float xpos = (float) Math.Sin(yaw);

            SetPosition(new Vector3(
                levelObject.position.X + xpos * distanceToObject,
                levelObject.position.Y + ypos * distanceToObject,
                levelObject.position.Z + distanceToObject / 2
            ));

            if (levelObject is Spline spline && spline.GetVertexCount() > 0)
            {
                Vector3 splineFirstVertex = spline.GetVertex(0);
                SetPosition(new Vector3(
                    splineFirstVertex.X + xpos * distanceToObject,
                    splineFirstVertex.Y + ypos * distanceToObject,
                    splineFirstVertex.Z + distanceToObject / 2
            ));
            }
        }

        public void Translate(Vector3 vector)
        {
            position += vector;
        }

        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z));
        }

        public void TransformedTranslate(Vector3 vector)
        {
            position += LegacyTransform(vector, GetRotationMatrix());
        }

        public void Rotate(Vector3 vector)
        {
            rotation += vector;
        }

        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }

        public void Rotate(Vector2 rot)
        {
            Vector3 v = rotation;

            v.Z -= rot.X;
            v.X -= rot.Y;
            v.X = MathHelper.Clamp(v.X,
                MathHelper.DegreesToRadians(-89.9f), MathHelper.DegreesToRadians(89.9f));

            rotation = v;
        }

        public void Scale(Vector3 scale)
        {
            //Not used
        }

        public void Scale(float x, float y, float z)
        {
            //Not used
        }

        private void ComputeFrustum()
        {
            frustum = new Frustum();

            float hfar = MathF.Tan(fovy) * far;
            float wfar = hfar * aspect;

            Vector3 forward = LegacyTransform(Vector3.UnitY, GetRotationMatrix());
            forward.Normalize();

            Vector3 up = LegacyTransform(Vector3.UnitZ, GetRotationMatrix());
            up.Normalize();

            Vector3 right = Vector3.Cross(forward, up);

            Vector3 nc = position + forward * near;
            frustum.planePoints[0] = nc;
            frustum.planeNormals[0] = forward;

            Vector3 fc = position + forward * far;
            frustum.planePoints[1] = fc;
            frustum.planeNormals[1] = -forward;

            Vector3 a = (fc + right * wfar) - position;
            a.Normalize();
            frustum.planePoints[2] = position;
            frustum.planeNormals[2] = Vector3.Cross(up, a);

            Vector3 b = (fc - right * wfar) - position;
            b.Normalize();
            frustum.planePoints[3] = position;
            frustum.planeNormals[3] = Vector3.Cross(b, up);

            Vector3 c = (fc + up * hfar) - position;
            c.Normalize();
            frustum.planePoints[4] = position;
            frustum.planeNormals[4] = Vector3.Cross(c, right);

            Vector3 d = (fc - up * hfar) - position;
            d.Normalize();
            frustum.planePoints[5] = position;
            frustum.planeNormals[5] = Vector3.Cross(right, d);
        }

        public Frustum GetFrustum()
        {
            if (frustumDirty)
            {
                ComputeFrustum();
                frustumDirty = false;
            }

            return frustum;
        }
    }
}
