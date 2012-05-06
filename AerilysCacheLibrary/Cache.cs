using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace AerilysCacheLibrary
{
    public abstract class Cache
    {
        private static string directory = "Cache";
        public static bool HasItem(string key)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(Path.Combine(directory, key)))
                    return true;
                else
                    return false;
            }
        }

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
