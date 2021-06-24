using System;
using System.Collections.Generic;
using Summer.Base;
using Summer.Base.Model;

namespace Spring.Collection.Reference
{
    public sealed class ReferenceCollection
    {
        private readonly Queue<IReference> references;
        private readonly Type referenceType;
        private int usingReferenceCount;
        private int acquireReferenceCount;
        private int releaseReferenceCount;
        private int addReferenceCount;
        private int removeReferenceCount;

        public ReferenceCollection(Type referenceType)
        {
            references = new Queue<IReference>();
            this.referenceType = referenceType;
            usingReferenceCount = 0;
            acquireReferenceCount = 0;
            releaseReferenceCount = 0;
            addReferenceCount = 0;
            removeReferenceCount = 0;
        }

        public Type ReferenceType
        {
            get { return referenceType; }
        }

        public int UnusedReferenceCount
        {
            get { return references.Count; }
        }

        public int UsingReferenceCount
        {
            get { return usingReferenceCount; }
        }

        public int AcquireReferenceCount
        {
            get { return acquireReferenceCount; }
        }

        public int ReleaseReferenceCount
        {
            get { return releaseReferenceCount; }
        }

        public int AddReferenceCount
        {
            get { return addReferenceCount; }
        }

        public int RemoveReferenceCount
        {
            get { return removeReferenceCount; }
        }

        public T Acquire<T>() where T : class, IReference, new()
        {
            if (typeof(T) != referenceType)
            {
                throw new GameFrameworkException("Type is invalid.");
            }

            usingReferenceCount++;
            acquireReferenceCount++;

            if (references.Count <= 0)
            {
                addReferenceCount++;
                return new T();
            }

            lock (references)
            {
                if (references.Count > 0)
                {
                    return (T) references.Dequeue();
                }
            }

            addReferenceCount++;
            return new T();
        }

        public IReference Acquire()
        {
            usingReferenceCount++;
            acquireReferenceCount++;
            lock (references)
            {
                if (references.Count > 0)
                {
                    return references.Dequeue();
                }
            }

            addReferenceCount++;
            return (IReference) Activator.CreateInstance(referenceType);
        }

        public void Release(IReference reference)
        {
            reference.Clear();
            lock (references)
            {
                if (references.Contains(reference))
                {
                    throw new GameFrameworkException("The reference has been released.");
                }

                references.Enqueue(reference);
            }

            releaseReferenceCount++;
            usingReferenceCount--;
        }

        public void Add<T>(int count) where T : class, IReference, new()
        {
            if (typeof(T) != referenceType)
            {
                throw new GameFrameworkException("Type is invalid.");
            }

            lock (references)
            {
                addReferenceCount += count;
                while (count-- > 0)
                {
                    references.Enqueue(new T());
                }
            }
        }

        public void Add(int count)
        {
            lock (references)
            {
                addReferenceCount += count;
                while (count-- > 0)
                {
                    references.Enqueue((IReference) Activator.CreateInstance(referenceType));
                }
            }
        }

        public void Remove(int count)
        {
            lock (references)
            {
                if (count > references.Count)
                {
                    count = references.Count;
                }

                removeReferenceCount += count;
                while (count-- > 0)
                {
                    references.Dequeue();
                }
            }
        }

        public void RemoveAll()
        {
            lock (references)
            {
                removeReferenceCount += references.Count;
                references.Clear();
            }
        }
    }
}