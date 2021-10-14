using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;

namespace Replanetizer.Utils
{
    public class Selection : INotifyCollectionChanged, ICollection<LevelObject>
    {
        private readonly HashSet<LevelObject> OBJECTS = new();

        public Vector3 pivot
        {
            get
            {
                if (pivotDirty)
                {
                    _pivot = CalculatePivotPoint();
                    pivotDirty = false;
                }

                return _pivot;
            }
        }

        private bool pivotDirty;
        private Vector3 _pivot;

        public bool isOnlySplines => splinesCount > 1 && nonSplinesCount == 0;
        private int splinesCount;
        private int nonSplinesCount;

        public int Count => OBJECTS.Count;
        public bool IsReadOnly => false;

        public IEnumerator<LevelObject> GetEnumerator() => OBJECTS.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(LevelObject obj) => OBJECTS.Contains(obj);

        public void CopyTo(LevelObject[] array, int arrayIndex)
        {
            OBJECTS.CopyTo(array, arrayIndex);
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Flag lazy properties for recalculation (e.g. when object positions change)
        /// </summary>
        public void SetDirty(bool dirty = true)
        {
            pivotDirty = dirty;
        }

        public void Clear()
        {
            OBJECTS.Clear();

            splinesCount = 0;
            nonSplinesCount = 0;
            SetDirty();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset
            ));
        }

        public void Set(LevelObject obj)
        {
            Clear();
            Add(obj);
        }

        public void Add(LevelObject obj)
        {
            OBJECTS.Add(obj);

            if (obj is Spline)
                splinesCount++;
            else
                nonSplinesCount++;

            SetDirty();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, obj
                )
            );
        }

        public void Add(IEnumerable<LevelObject> objects)
        {
            var newItems = new List<LevelObject>();

            foreach (var obj in objects)
            {
                newItems.Add(obj);
                OBJECTS.Add(obj);

                if (obj is Spline)
                    splinesCount++;
                else
                    nonSplinesCount++;
            }

            SetDirty();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, newItems
                )
            );
        }

        public bool Remove(LevelObject obj)
        {
            if (!OBJECTS.Remove(obj))
                return false;

            if (obj is Spline)
                splinesCount--;
            else
                nonSplinesCount--;

            SetDirty();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, obj
                )
            );
            return true;
        }


        public void Remove(IEnumerable<LevelObject> objects)
        {
            var removedItems = new List<LevelObject>();

            foreach (var obj in objects)
            {
                removedItems.Remove(obj);
                OBJECTS.Remove(obj);

                if (obj is Spline)
                    splinesCount--;
                else
                    nonSplinesCount--;
            }

            SetDirty();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removedItems
                )
            );
        }

        public void Toggle(LevelObject obj)
        {
            if (OBJECTS.Contains(obj))
                Remove(obj);
            else
                Add(obj);
        }

        public void ToggleOne(LevelObject obj)
        {
            if (OBJECTS.Contains(obj))
                Clear();
            else
                Set(obj);
        }

        public bool TryGetOne([NotNullWhen(true)] out LevelObject? obj)
        {
            obj = null;
            if (OBJECTS.Count != 1)
                return false;

            obj = OBJECTS.GetEnumerator().Current;
            return true;
        }

        private Vector3 CalculatePivotPoint()
        {
            var pivot = new Vector3();
            foreach (var obj in OBJECTS)
            {
                pivot += obj.position;
            }
            pivot /= OBJECTS.Count;
            return pivot;
        }
    }
}
