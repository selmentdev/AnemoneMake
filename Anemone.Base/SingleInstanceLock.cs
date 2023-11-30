// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Anemone.Base;

public sealed class SingleInstanceLock : IDisposable
{
    private Mutex? m_Mutex;

    public SingleInstanceLock(string name, string path, bool wait)
    {
        //
        // Create a unique name for the mutex.
        //

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(path)).ToHex();

        this.m_Mutex = new Mutex(true, $@"Global\{name}_{hash}", out var created);

        if (!created)
        {
            if (wait)
            {
                try
                {
                    this.m_Mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                    //
                    // The mutex was abandoned. This is not an error.
                    //
                }
            }
            else
            {
                throw new InvalidOperationException("The application is already running.");
            }
        }
    }

    public void Dispose()
    {
        if (this.m_Mutex != null)
        {
            this.m_Mutex.ReleaseMutex();
            this.m_Mutex.Dispose();
            this.m_Mutex = null;
        }
    }
}
