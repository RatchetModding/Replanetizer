using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;

namespace Replanetizer.Utils
{
    /// <summary>
    /// A container for LevelObjects that the user can select. Allows for adding
    /// and removing individual objects from the selection without resetting it.
    /// </summary>
    public class Selection : INotifyCollectionChanged, ICollection<LevelObject>
    {
        private readonly HashSet<LevelObject> OBJECTS = new();

        /// <summary>
        /// The mean point of all selected objects (averaged positions)
        /// </summary>
        public Vector3 mean
        {
            get
            {
                if (meanDirty)
                {
                    _mean = CalculateMean();
                    meanDirty = false;
                }

                return _mean;
            }
        }

        private bool meanDirty;
        private Vector3 _mean;

        /// <summary>
        /// Whether the selection contains only splines
        /// </summary>
        public bool isOnlySplines => splinesCount > 1 && nonSplinesCount == 0;
        private int splinesCount;
        private int nonSplinesCount;

        /// <summary>
        /// The most recently selected object. Note that this will be null if
        /// the most recently selected object was deselected, even if there are
        /// other objects still selected. This is a consequence of using a
        /// HashSet implementation for quick membership testing.
        /// </summary>
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

        /// <summary>
        /// Occurs when one or more objects are added to or removed from the
        /// selection, or when the selection is cleared.
        /// </summary>
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
            meanDirty = dirty;
        }

        /// <summary>
        /// Remove all objects from the selection
        /// </summary>
        public void Clear()
        {
            OBJECTS.Clear();
            newestObject = null;

            splinesCount = 0;
            nonSplinesCount = 0;
            SetDirty();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset
            ));
        }

        /// <summary>
        /// Clear the selection and add only the given object
        /// </summary>
        public void Set(LevelObject obj)
        {
            Clear();
            Add(obj);
        }

        /// <summary>
        /// Add an object to the selection
        /// </summary>
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

        /// <summary>
        /// Add several objects to the collection at once
        /// </summary>
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

        /// <summary>
        /// Remove an object from the selection
        /// </summary>
        /// <returns>whether the object was removed from the selection</returns>
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

        /// <summary>
        /// Remove several objects from the selection at once
        /// </summary>
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

        /// <summary>
        /// Toggle selection of an object without affecting the other selections
        /// </summary>
        public void Toggle(LevelObject obj)
        {
            if (OBJECTS.Contains(obj))
                Remove(obj);
            else
                Add(obj);
        }

        /// <summary>
        /// Toggle selection of just one object, clearing any other selections
        /// </summary>
        public void ToggleOne(LevelObject obj)
        {
            if (OBJECTS.Count == 1 && OBJECTS.Contains(obj))
                Clear();
            else
                Set(obj);
        }

        /// <summary>
        /// Get the single selected object if it is the only one in the selection
        /// </summary>
        /// <param name="obj">the single selected object or null if there are zero
        /// or multiple objects selected</param>
        /// <returns>true if the single selected object was set, else false</returns>
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

        /// <summary>
        /// Calculate the mean point of all selected object
        /// </summary>
        private Vector3 CalculateMean()
        {
            var mean = new Vector3();
            int count = 0;

            foreach (var obj in OBJECTS)
            {
                if (obj is TerrainFragment) continue;

                mean += obj.position;
                count++;
            }

            mean /= count;
            return mean;
        }
    }
}
