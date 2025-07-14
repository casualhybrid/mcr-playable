using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System;

public static class Utilities 
{
    public static async Task<bool> IsInternetAvailable()
    {
        Task<Stream> t = null;

        try
        {
            WebClient webClient = new WebClient();

            t = webClient.OpenReadTaskAsync("https://unity.com/");

            await t;

            return t != null && t.Result != null;

        }
        catch (Exception ex)
        {
            UnityEngine.Console.Log($"Internet not available. Reason: {ex}");
            return false;
        }
        finally
        {
            if (t != null && t.IsCompletedSuccessfully && t.Result != null)
            {
                t.Result.Dispose();
            }
        }
    }
}
