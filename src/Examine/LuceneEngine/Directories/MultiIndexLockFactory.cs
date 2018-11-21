using System;
using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Lock factory that wraps multiple factories
    /// </summary>
    
    public class MultiIndexLockFactory : LockFactory
    {
        private readonly LockFactory _master;
        private readonly LockFactory _child;

        private static readonly object Locker = new object();
        
        public MultiIndexLockFactory(Directory master, Directory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            _master = master.LockFactory;
            _child = child.LockFactory;
        }
        
        public MultiIndexLockFactory(LockFactory master, LockFactory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            lock (Locker)
            {
                _master = master;
                _child = child;
            }
        }

        
        public override Lock MakeLock(string lockName)
        {
            lock (Locker)
            {
                return new MultiIndexLock(_master.MakeLock(lockName), _child.MakeLock(lockName));
            }
        }

        
        public override void ClearLock(string lockName)
        {
            lock (Locker)
            {
                var isChild = false;
                try
                {
                    //try to release master
                    _master.ClearLock(lockName);

                    //if that succeeds try to release child
                    isChild = true;
                    _child.ClearLock(lockName);
                }
                catch (Exception)
                {
                    //if an error occurs above for the master still attempt to release child
                    if (!isChild)
                        _child.ClearLock(lockName);

                    throw;
                }
                
            }
        }
    }
}