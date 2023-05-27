﻿using System.Numerics;

namespace Kinetic.Presentation.Filters
{
    public class LowPassFilter
    {
        private readonly double _alpha;
        private Vector3 _lastOutput;

        public LowPassFilter(double alpha)
        {
            _alpha = alpha;
        }

        public Vector3 Filter(Vector3 input)
        {
            _lastOutput.X = (float)(_alpha * _lastOutput.X + (1.0 - _alpha) * input.X);
            _lastOutput.Y = (float)(_alpha * _lastOutput.Y + (1.0 - _alpha) * input.Y);
            _lastOutput.Z = (float)(_alpha * _lastOutput.Z + (1.0 - _alpha) * input.Z);

            return _lastOutput;
        }
    }
}
