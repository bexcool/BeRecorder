using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BeRecorderWinUI3.Helpers
{
    public class FileHelper
    {
        static List<string> files = new List<string>();

        public async Task<List<string>> GetFiles(string path)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(path);

            GetFilesAsync(storageFolder);

            return files;
        }

        private async void GetFilesAsync(StorageFolder folder)
        {
            StorageFolder fold = folder;

            var items = await fold.GetItemsAsync();

            foreach (var item in items)
                if (item.GetType() == typeof(StorageFile))
                    files.Add(item.Path.ToString());
                else
                    GetFilesAsync(item as StorageFolder);
        }

        public static async Task<bool> CacheFileExists(string path)
        {
            var item = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(path);
            return item != null;
        }

        public static async Task<StorageFile> CacheCreateFile(string path)
        {
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
            return file;
        }

        public static async Task<StorageFile> CacheCreateFile(string path, CreationCollisionOption creationCollisionOption)
        {
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(path, creationCollisionOption);
            return file;
        }

        public static async Task<StorageFile> CacheCreateFileDontReplace(string path)
        {
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
            return file;
        }

        public static async Task<StorageFile> CacheGetFile(string path)
        {
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(path);
            return file;
        }

        public static async Task CacheWriteBytes(string path, byte[] bytes)
        {
            var fileStream = await (await CacheCreateFile(path, CreationCollisionOption.OpenIfExists)).OpenAsync(FileAccessMode.ReadWrite);

            using (var outputStream = fileStream.GetOutputStreamAt(0))
            {
                await outputStream.FlushAsync();

                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteBytes(bytes);
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }

            fileStream.Dispose();
        }

        public static async Task CacheWriteBytes(string path, byte[] bytes, CreationCollisionOption creationCollisionOption)
        {
            var fileStream = await (await CacheCreateFile(path, creationCollisionOption)).OpenAsync(FileAccessMode.ReadWrite);

            using (var outputStream = fileStream.GetOutputStreamAt(0))
            {
                await outputStream.FlushAsync();

                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteBytes(bytes);
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }

            fileStream.Dispose();
        }

        public static async Task CacheWriteText(string path, string text)
        {
            var fileStream = await (await CacheCreateFile(path, CreationCollisionOption.ReplaceExisting)).OpenAsync(FileAccessMode.ReadWrite);

            using (var outputStream = fileStream.GetOutputStreamAt(0))
            {
                await outputStream.FlushAsync();

                using (var dataWriter = new DataWriter(outputStream))
                {
                    dataWriter.WriteString(text);
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }

            fileStream.Dispose();
        }

        public static async Task CacheWriteText(string path, string text, CreationCollisionOption creationCollisionOption)
        {
            var fileStream = await (await CacheCreateFile(path, creationCollisionOption)).OpenAsync(FileAccessMode.ReadWrite);

            using (var outputStream = fileStream.GetOutputStreamAt(0))
            {
                await outputStream.FlushAsync();

                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteBytes(new UTF8Encoding().GetBytes(text));
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }

            fileStream.Dispose();
        }

        public static async Task<byte[]> CacheReadBytes(string path)
        {
            var fileStream = await (await CacheCreateFile(path, CreationCollisionOption.OpenIfExists)).OpenAsync(FileAccessMode.Read);

            using (var reader = new DataReader(fileStream))
            {
                await reader.LoadAsync((uint)fileStream.Size);

                var buffer = new byte[(int)fileStream.Size];

                reader.ReadBytes(buffer);

                return buffer;
            }
        }

        public static async Task<string> CacheReadText(string path)
        {
            var fileStream = await (await CacheCreateFile(path, CreationCollisionOption.OpenIfExists)).OpenAsync(FileAccessMode.Read);

            using (var reader = new DataReader(fileStream))
            {
                await reader.LoadAsync((uint)fileStream.Size);

                var buffer = new byte[(int)fileStream.Size];

                reader.ReadBytes(buffer);

                return new UTF8Encoding().GetString(buffer);
            }
        }
    }
}
