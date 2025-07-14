using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TheKnights.AddressableSystem
{
    public static class AddressableLoader
    {
        // Assets that are loaded in the memory
        private static readonly Dictionary<string, AsyncOperationHandle> assetsInMemory = new Dictionary<string, AsyncOperationHandle>();

        #region LoadingAssets

        /// <summary>
        /// Attempt to load the a single asset like a prefab from addressable into the memory from the disk or remotley depending on wether the bundle has been downloaded or not.
        /// </summary>
        /// <param name="key">The addressable key for a specific asset</param>
        public static AddressableOperationHandle<T> LoadTheAsset<T>(string key)
        {
            AddressableOperationHandle<T> operationHandle = new AddressableOperationHandle<T>();

            AsyncOperationHandle<T> handler = Addressables.LoadAssetAsync<T>(key);
            operationHandle.handle = handler;

            Action WaitForAssetLoad = async () =>
            {
                try
                {
                    // Make sure this is executed asynchronously
                    await Task.Yield();
                    // Make sure this is executed asynchronously
                    await Task.Yield();
                    // Make sure this is executed asynchronously
                    await Task.Yield();
                    // Make sure this is executed asynchronously
                    await Task.Yield();

                    await handler.Task;

                    if (handler.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (!assetsInMemory.ContainsKey(key))
                        {
                            assetsInMemory.Add(key, handler);
                        }
                        else
                        {
                            assetsInMemory[key] = handler;
                        }

                        operationHandle.Result = handler.Task.Result;

                      //  try
                     //   {
                            operationHandle.SendCompletionEvent();
                    //    }
                       // catch(Exception e)
                      //  {
                      //      UnityEngine.Console.LogWarning("No subscriber found for asset downloaded. Releasing the handler " + key + "Exceptions " + e) ;
                      //      ReleaseThisAssetFromMemory(key);
                      // }
                    }
                    else
                    {
                        DownloadFailed(operationHandle, handler.OperationException);
                    }
                }
                catch (Exception exception)
                {
                    DownloadFailed(operationHandle, exception);
                }
            };

            WaitForAssetLoad();

            return operationHandle;
        }

        #endregion LoadingAssets

        #region DownloadingDependencies

        public static AddressableOperationHandle DownloadDependencies(IList<IResourceLocation> keys, bool autoRelease = false)
        {
            AddressableOperationHandle operationHandle = new AddressableOperationHandle();

            AsyncOperationHandle handler = Addressables.DownloadDependenciesAsync(keys, false);
            operationHandle.handle = handler;

            Action WaitForDownload = async () =>
            {
                try
                {
                    // Make sure this is executed asynchronously
                    await Task.Yield();

                    await handler.Task;

                    if (handler.Status == AsyncOperationStatus.Succeeded)
                    {
                        object result = handler.Task.Result;

                        if (autoRelease && handler.IsValid())
                            Addressables.Release(handler);

                        operationHandle.Result = result;

                        try
                        {
                            operationHandle.SendCompletionEvent();
                        }
                        catch
                        {
                            UnityEngine.Console.LogWarning("No subscriber found for asset downloaded. Releasing the handler");
                        }
                    }
                    else
                    {
                        DownloadFailed(operationHandle, handler.OperationException);
                    }
                }
                catch (Exception e)
                {
                    DownloadFailed(operationHandle, e);
                }
            };

            WaitForDownload();

            return operationHandle;
        }

        #endregion DownloadingDependencies

        #region GetDownloadSize

        /// <summary>
        /// Get the download size of the provided asset key
        /// </summary>
        /// <param name="key">The addressable key for a specific asset</param>
        public static AddressableOperationHandle GetDownloadSize(string key)
        {
            AddressableOperationHandle operationHandle = new AddressableOperationHandle();

            AsyncOperationHandle downloadsizeHandler = Addressables.GetDownloadSizeAsync(key);
            operationHandle.handle = downloadsizeHandler;

            Action WaitForDownloadSize = async () =>
            {
                try
                {
                    // Make sure this is executed asynchronously
                    await Task.Yield();

                    await downloadsizeHandler.Task;

                    if (downloadsizeHandler.Status == AsyncOperationStatus.Succeeded)
                    {
                        long downloadSize = (long)downloadsizeHandler.Result;
                        UnityEngine.Console.Log($"Download size of asset { key} is { downloadsizeHandler.Result }");

                        operationHandle.Result = downloadSize / 1000000f;

                        try
                        {
                            operationHandle.SendCompletionEvent();
                        }
                        catch
                        {
                            UnityEngine.Console.LogWarning("No subscriber for download size get");
                        }
                    }
                    else
                    {
                        DownloadFailed(operationHandle, downloadsizeHandler.OperationException);
                    }
                }
                catch (Exception exception)
                {
                    DownloadFailed(operationHandle, exception);
                }
            };

            WaitForDownloadSize();

            return operationHandle;
        }

        public static AddressableOperationHandle GetDownloadSize(IList<IResourceLocation> key)
        {
            AddressableOperationHandle operationHandle = new AddressableOperationHandle();

            AsyncOperationHandle downloadsizeHandler = Addressables.GetDownloadSizeAsync(key);
            operationHandle.handle = downloadsizeHandler;

            Action WaitForDownloadSize = async () =>
            {
                try
                {
                    // Make sure this is executed asynchronously
                    await Task.Yield();

                    await downloadsizeHandler.Task;

                    if (downloadsizeHandler.Status == AsyncOperationStatus.Succeeded)
                    {
                        long downloadSize = (long)downloadsizeHandler.Result;
                        UnityEngine.Console.Log($"Download size of asset { key} is { downloadsizeHandler.Result }");

                        operationHandle.Result = downloadSize / 1000000f;

                        try
                        {
                            operationHandle.SendCompletionEvent();
                        }
                        catch
                        {
                            UnityEngine.Console.LogWarning("No subscriber for download size get");
                        }
                    }
                    else
                    {
                        DownloadFailed(operationHandle, downloadsizeHandler.OperationException);
                    }
                }
                catch (Exception exception)
                {
                    DownloadFailed(operationHandle, exception);
                }
            };
            WaitForDownloadSize();

            return operationHandle;
        }

        #endregion GetDownloadSize

        #region ReleaseAssets

        /// <summary>
        /// Releases the loaded addressable from the memory for the key provided so it will be loaded from the disk the next time it's requested
        /// </summary>
        public static void ReleaseThisAssetFromMemory(string key)
        {
            try
            {
                AsyncOperationHandle asyncOperationHandle;
                asyncOperationHandle = assetsInMemory[key];
                Addressables.Release(asyncOperationHandle);

                UnityEngine.Console.Log($"Releasing handler of {key}");
            }
            catch
            {
                UnityEngine.Console.LogWarning($"Failed to release  {key}  asset from memory. It's handler couldn't be found in loadedAssetsDatabase");
            }
        }

        /// <summary>
        /// Releases all of the loaded addressables from the memory so they will be loaded from the disk the next time they're requested
        /// </summary>
        public static void ReleaseAllLoadedAssetsFromMemory()
        {
            foreach (var V in assetsInMemory)
            {
                if (V.Value.IsValid())
                    Addressables.Release(V.Value);
            }
        }

        #endregion ReleaseAssets

        #region DownloadingFailed

        private static void DownloadFailed<T>(AddressableOperationHandle<T> operationHandle, Exception exception)
        {
            UnityEngine.Console.Log($"Downloading failed because of :   {exception}");

            try
            {
                operationHandle.SendCompletionEvent();
            }
            catch (Exception E)
            {
                UnityEngine.Console.LogWarning($"No Subscriber for downloading failed event Reason: {E}");
            }
        }

        private static void DownloadFailed(AddressableOperationHandle operationHandle, Exception Exception)
        {
            UnityEngine.Console.Log($"Downloading dependencies failed because of : { Exception}");

            try
            {
                operationHandle.SendCompletionEvent();
            }
            catch (Exception E)
            {
                UnityEngine.Console.Log($"No Subscriber for downloading failed event Reason:  {E}");
            }
        }

        #endregion DownloadingFailed

        #region GetLocations

        public static AddressableOperationHandle GetLocations(IEnumerable keys)
        {
            AddressableOperationHandle operationHandle = new AddressableOperationHandle();
            IList<IResourceLocation> loadedLocations = new List<IResourceLocation>();
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
            operationHandle.handle = handle;

            Action WaitToDownloadLocations = async () =>
            {
                try
                {
                    // Make sure this is executed asynchronously
                    await Task.Yield();

                    await handle.Task;
                    var unloadedLocations = handle.Result;

                    foreach (var location in unloadedLocations)
                    {
                        loadedLocations.Add(location);
                    }

                    if (handle.IsValid())
                        Addressables.Release(handle);

                    operationHandle.Result = loadedLocations;
                    try
                    {
                        operationHandle.SendCompletionEvent();
                    }
                    catch
                    {
                        UnityEngine.Console.LogWarning("No Subscriber for resource locations downloaded");
                    }
                }
                catch (Exception exception)
                {
                    DownloadFailed(operationHandle, exception);
                }
            };
            WaitToDownloadLocations();
            return operationHandle;
        }

        #endregion GetLocations
    }
}