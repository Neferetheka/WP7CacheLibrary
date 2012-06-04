using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace AerilysCacheLibrary
{
    public abstract class Cache
    {
        private static string directory = "Cache";

        /// <summary>
        /// Permit to check if an item exists in cache
        /// </summary>
        /// <param name="key">key of the cached datas</param>
        /// <returns>true if exists. false otherwise</returns>
        public static bool HasItem(string key)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(Path.Combine(directory, key)))
                {
                    string filePath = Path.Combine(directory, key);
                    string filePathCache = Path.Combine(directory, key + "cache");
                    DateTime toExpire;
                    using (StreamReader reader = new StreamReader(store.OpenFile(filePathCache, FileMode.Open, FileAccess.Read)))
                    {
                        toExpire = DateTime.Parse(reader.ReadToEnd());
                    }

                    if (toExpire == null || DateTime.Now.CompareTo(toExpire) > 0)
                    {
                        store.DeleteFile(filePath);
                        store.DeleteFile(filePathCache);
                        return false;
                    }

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Get an item from the cache. 
        /// </summary>
        /// <param name="key">key of the cached datas</param>
        /// <returns>Null if empty or expired. Datas as string otherwise</returns>
        public static string GetItem(string key)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = Path.Combine(directory, key);
                    string filePathCache = Path.Combine(directory, key + "cache");

                    if (store.FileExists(filePath))
                    {
                        //On vérifie l'expiration
                        DateTime toExpire;
                        using (StreamReader reader = new StreamReader(store.OpenFile(filePathCache, FileMode.Open, FileAccess.Read)))
                        {
                            toExpire = DateTime.Parse(reader.ReadToEnd());
                        }

                        if (toExpire == null || DateTime.Now.CompareTo(toExpire) > 0)
                        {
                            store.DeleteFile(filePath);
                            store.DeleteFile(filePathCache);
                            return null;
                        }

                        using (StreamReader reader = new StreamReader(store.OpenFile(filePath, FileMode.Open, FileAccess.Read)))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    else
                        return null;
                }

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Put datas in cache
        /// </summary>
        /// <param name="key">key of the cached datas</param>
        /// <param name="value">value as string that you want in cache</param>
        /// <param name="timeToExpire">DateTime when the datas will expire</param>
        /// <returns>true if okay, false otherwise</returns>
        public static bool SetItem(string key, string value, DateTime timeToExpire)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.DirectoryExists(directory))
                        store.CreateDirectory(directory);

                    string filePath = Path.Combine(directory, key);
                    string filePathCache = Path.Combine(directory, key + "cache");

                    if (store.FileExists(filePath))
                    {
                        store.DeleteFile(filePath);
                        if (store.FileExists(filePathCache))
                            store.DeleteFile(filePathCache);
                    }

                    using (var isoFileStream = new IsolatedStorageFileStream(filePath, FileMode.OpenOrCreate, store))
                    {
                        using (var isoFileWriter = new StreamWriter(isoFileStream))
                        {
                            isoFileWriter.WriteLine(value);
                        }
                    }

                    using (var isoFileStream = new IsolatedStorageFileStream(filePathCache, FileMode.OpenOrCreate, store))
                    {
                        using (var isoFileWriter = new StreamWriter(isoFileStream))
                        {
                            isoFileWriter.WriteLine(timeToExpire.ToString());
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}