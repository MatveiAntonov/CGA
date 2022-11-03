using System;
using System.Numerics;

namespace GraphicsLibrary.Camera {
    public class CameraModel {
        public float r { get; set; } = 4;
        public float fi { get; set; } = 0;
        public float tet { get; set; } = (float)Math.PI / 2;

        public Vector3 camPos { get; set; }
        public Vector3 camDir { get; set; }
        public Vector3 up { get; set; } = new Vector3(0, 1, 0);
        public Vector3 camUp { get; set; }
        public  Vector3 camRight { get; set; }
        public Vector3 camTarget { get; set; } = new Vector3(0, 0, 0);

        public CameraModel() {
            camPos = new Vector3(XCalc(), YCalc(), ZCalc());

            camDir = Vector3.Normalize(Vector3.Subtract(camPos, camTarget));
            camRight = Vector3.Normalize(Vector3.Cross(up, camDir));
            camUp = up;
        }

        public void CountParams() {
            camPos = new Vector3(XCalc(), YCalc(), ZCalc());
            camDir = Vector3.Normalize(Vector3.Subtract(camPos, camTarget));
            camRight = Vector3.Normalize(Vector3.Cross(up, camDir));
            camUp = up;
        }

        public float XCalc () {
            return r * (float)Math.Sin((double)fi) * (float)Math.Sin(tet);
        }

        public float YCalc() {
            return r * (float)Math.Cos(tet);
        }

        public float ZCalc() {
            return r * (float)Math.Cos(fi) * (float)Math.Sin(tet);
        }
    }
}
