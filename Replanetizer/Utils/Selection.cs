using System.Collections.ObjectModel;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;

namespace Replanetizer.Utils
{
    public class Selection : ObservableCollection<LevelObject>
    {
        public Vector3 pivot
        {
            get
            {
                if (_pivotDirty)
                {
                    _pivot = CalculatePivotPoint();
                    _pivotDirty = false;
                }

                return _pivot;
            }
        }

        private bool _pivotDirty;
        private Vector3 _pivot;

        public bool isOnlySplines => splinesCount > 1 && nonSplinesCount == 0;
        private int splinesCount;
        private int nonSplinesCount;

        private void SetDirty(bool dirty)
        {
            _pivotDirty = dirty;
        }

        public new void Clear()
        {
            splinesCount = 0;
            nonSplinesCount = 0;
            SetDirty(true);
            base.Clear();
        }
        public new bool Add(LevelObject obj)
        {
            if (Contains(obj))
                return false;

            if (obj is Spline)
                splinesCount++;
            else
                nonSplinesCount++;

            SetDirty(true);
            base.Add(obj);
            return true;
        }

        public void Set(LevelObject obj)
        {
            Clear();
            Add(obj);
        }

        public new bool Remove(LevelObject obj)
        {
            if (!base.Remove(obj))
                return false;

            if (obj is Spline)
                splinesCount--;
            else
                nonSplinesCount--;

            SetDirty(true);
            return true;
        }

        public bool Toggle(LevelObject obj)
        {
            return Add(obj) || Remove(obj);
        }

        private Vector3 CalculatePivotPoint()
        {
            var pivot = new Vector3();
            foreach (var obj in this)
            {
                pivot += obj.position;
            }
            pivot /= Count;
            return pivot;
        }
    }
}
