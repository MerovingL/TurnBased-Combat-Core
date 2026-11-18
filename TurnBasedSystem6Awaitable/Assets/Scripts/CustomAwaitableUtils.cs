using System;
using System.Threading;
using UnityEngine;

public static class CustomAwaitableUtils 
{
    public static async Awaitable AwaitableUntil(Func<bool> condition, CancellationToken token)
    {
        while (!condition())
        {
            token.ThrowIfCancellationRequested();
            await Awaitable.NextFrameAsync();
        }
    }
    public static async Awaitable AwaitableWhile(Func<bool> condition, CancellationToken token)
    {
        while (condition())
        {
            token.ThrowIfCancellationRequested();
            await Awaitable.NextFrameAsync();
        }
    }
}
