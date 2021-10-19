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

        public Vector3 median
        {
            get
            {
                if (medianDirty)
                {
                    _median = CalculateMedian();
                    medianDirty = false;
                }

                return _median;
            }
        }

        private bool medianDirty;
        private Vector3 _median;

        public bool isOnlySplines => splinesCount > 1 && nonSplinesCount == 0;
        private int splinesCount;
        private int nonSplinesCount;

        public LevelObject? newestObject { get; private set; }

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
            medianDirty = dirty;
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
            newestObject = obj;

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
                newestObject = obj;

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
            if (ReferenceEquals(obj, newestObject))
                newestObject = null;

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
                if (ReferenceEquals(obj, newestObject))
                    newestObject = null;

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

        // Toggle selection of an object without affecting the other selections
        public void Toggle(LevelObject obj)
        {
            if (OBJECTS.Contains(obj))
                Remove(obj);
            else
                Add(obj);
        }

        // Toggle selection of just one object, clearing any other selections.
        public void ToggleOne(LevelObject obj)
        {
            if (OBJECTS.Count == 1 && OBJECTS.Contains(obj))
                Clear();
            else
                Set(obj);
        }

        public bool TryGetOne([NotNullWhen(true)] out LevelObject? obj)
        {
            obj = null;
            if (OBJECTS.Count != 1)
                return false;

            using var enumerator = OBJECTS.GetEnumerator();
            enumerator.MoveNext();
            obj = enumerator.Current;
            return true;
        }

        private Vector3 CalculateMedian()
        {
            var median = new Vector3();
            foreach (var obj in OBJECTS)
            {
                median += obj.position;
            }
            median /= OBJECTS.Count;
            return median;
        }
    }
}
