using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatchetEdit {
    public static class Utilities {
        public static float toDegrees(float radians) {
            return radians * 180 / (float)Math.PI;
        }
        public static float toRadians(float angle) {
            return (float)Math.PI / 180 * angle;
        }
        public static float roundToDecimals(float value, float numberOfDecimals) {
            float multiplier = (float)Math.Pow(10, numberOfDecimals);
            return (float)Math.Ceiling(value * multiplier) / multiplier;
        }
    }
}
