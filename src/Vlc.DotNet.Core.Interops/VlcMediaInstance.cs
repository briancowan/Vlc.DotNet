using System;
using System.Collections.Generic;
using System.Threading;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Vlc.DotNet.Core.Interops
{
    public sealed class VlcMediaInstance : InteropObjectInstance
    {
        private readonly VlcManager myManager;
        static readonly Dictionary<IntPtr, int> sInstanceCount = new Dictionary<IntPtr, int>();
        private static object _lock = new object();

        internal VlcMediaInstance(VlcManager manager, IntPtr pointer)
            : base(pointer)
        {
            myManager = manager;

            try
            {
                Monitor.TryEnter(_lock, 5000);
                {
                    if (pointer != IntPtr.Zero)
                    {
                        // keep a reference count for the media instance
                        if (!sInstanceCount.ContainsKey(pointer))
                            sInstanceCount[pointer] = 1;
                        else
                            sInstanceCount[pointer] = sInstanceCount[pointer] + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (Pointer != IntPtr.Zero)
                {
                    Monitor.TryEnter(_lock, 5000);
                    {
                        if (sInstanceCount.ContainsKey(this.Pointer))
                        {
                            // only release the instance if no more references
                            if (sInstanceCount[this.Pointer] > 0)
                            {
                                sInstanceCount[this.Pointer] = sInstanceCount[this.Pointer] - 1;
                                if (sInstanceCount[this.Pointer] == 0)
                                    myManager.ReleaseMedia(this);
                            }
                        }
                    }
                }
                base.Dispose(disposing);
            }
            catch ( Exception ex )
            {
                throw ex;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static implicit operator IntPtr(VlcMediaInstance instance)
        {
            return instance != null
                 ? instance.Pointer
                 : IntPtr.Zero;
        }
    }
}