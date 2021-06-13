using SunriseLauncher.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SunriseLauncher.Services
{
    public class FileUpdater
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private HttpClient client = new HttpClient();

        public async Task<UpdateResult> UpdateAsync(Server server, bool force)
        {
            if (server.State == State.Updating)
                return new UpdateResult(true, null);

            try
            {
                server.State = State.Updating;
                server.ProgressState.Desc = "retrieving manfiest";
                server.CancellationTokenSource = new CancellationTokenSource();

                var manifest = MainfestFactory.Get(server.ManifestURL);
                if (manifest == null)
                {
                    Console.WriteLine("Unknown manifest schema at '{0}'", server.ManifestURL);
                    server.State = State.Ready;
                    return new UpdateResult(false, "Unknown manifest schema. You may still attempt to play, but you may be missing updates.");
                }

                var metadata = await manifest.GetMetadataAsync();
                if (metadata == null)
                {
                    Console.WriteLine("Could not retrieve manifest from '{0}'", server.ManifestURL);
                    server.State = State.Ready;
                    return new UpdateResult(false, "Could not retrieve manifest. You may still attempt to play, but you may be missing updates.");
                }

                if (!metadata.Verify())
                {
                    Console.WriteLine("Manifest metadata failed inspection '{0}'", server.ManifestURL);
                    server.State = State.Ready;
                    return new UpdateResult(false, "Manifest failed inspection. You may still attempt to play, but you may be missing updates.");
                }

                if (force || metadata.Version != server.Metadata.Version)
                {
                    var result = await Updatefiles(server, manifest, server.InstallPath);
                    if (!result.Success) return result;
                }
                else
                {
                    server.State = State.Ready;
                }

                if (server.State == State.Ready)
                {
                    server.Metadata = metadata;
                }

                //if update occurs which removes the selected launch option, default to first option available
                if (metadata.LaunchOptions.All(x => x.Name != server.Launch))
                {
                    server.Launch = metadata.LaunchOptions[0].Name;
                }
            }
            catch (Exception ex)
            {
                server.State = State.Error;
                Console.WriteLine("exception in UpdateAsync: {0}", ex.Message);
                if (ex.StackTrace != null)
                    Console.WriteLine(ex.StackTrace);
                return new UpdateResult(false, "UpdateAsync Exception");
            }
            finally
            {
                server.ProgressState.Desc = null;
                server.ProgressState.Progress = 0;
                server.ProgressState.ProgressMax = 0;
            }
            return new UpdateResult(true, null);
        }

        private async Task<UpdateResult> Updatefiles(Server server, IManifest manifest, string path)
        {
            server.ProgressState.Desc = "waiting in queue ...";
            await semaphore.WaitAsync();
            try
            {
                var token = server.CancellationTokenSource.Token;

                var files = await manifest.GetFilesAsync();
                if (files == null)
                {
                    server.State = State.Error;
                    return new UpdateResult(false, "Could not retrieve files from manifest.");
                }

                server.ProgressState.SetFiles(files.Count);
                var throttle = new SemaphoreSlim(4);
                var tasks = files.Select(async (file,i) =>
                {
                    try
                    {
                        await throttle.WaitAsync();
                        if (token.IsCancellationRequested) return;

                        if (!file.Verify())
                        {
                            server.State = State.Error;
                            //return new UpdateResult(false, "Manifest file failed inspection " + file.Path);
                        }

                        var result = await Updatefile(file, server, i);

                    }
                    finally
                    {
                        server.ProgressState.Update(i, true);
                        throttle.Release();
                    }

                });
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                server.State = State.Error;
                Console.WriteLine(string.Format("exception in updatefiles: {0}", ex.Message));
                return new UpdateResult(false, "UpdateFiles Exception");
            }
            finally
            {
                semaphore.Release();
                server.ProgressState.Desc = null;
            }

            server.State = State.Ready;
            return new UpdateResult(true, null);
        }

        private async Task<UpdateResult> Updatefile(ManifestFile file, Server server, int index)
        {
            if (await Checkfile(file, server, index))
                return new UpdateResult(true, null);

            var path = Path.Combine(server.InstallPath, file.Path);
            var tempfile = path + "~";
            var token = server.CancellationTokenSource.Token;

            Console.WriteLine("downloading {0}", path);
            Shuffler.Shuffle(file.Sources);
            foreach (var source in file.Sources)
            {
                Console.WriteLine("downloading from source '{0}'.", source.URL);
                try
                {
                    var state = new FileProgressState();
                    state.Desc = "downloading " + file.Path;
                    state.Max = file.Size;
                    server.ProgressState.Update(index, state);

                    using (var hash = Hashing.GetHashAlgorithm(file))
                    {
                        if (hash == null)
                            return new UpdateResult(false, "hash algorithm missing for " + file.Path);

                        var dirname = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(dirname)) Directory.CreateDirectory(dirname);

                        long size = 0;
                        byte[] checksum;
                        var response = await client.GetAsync(source.URL, HttpCompletionOption.ResponseHeadersRead);
                        if (response.IsSuccessStatusCode)
                        {
                            using (var filestream = new FileStream(tempfile, FileMode.Create))
                            using (var hashstream = new CryptoStream(filestream, hash, CryptoStreamMode.Write))
                            using (var reader = await response.Content.ReadAsStreamAsync())
                            {
                                size = await CopyToProgressFileAsync(reader, hashstream, 81920, server, index, token);
                                hashstream.FlushFinalBlock();
                                checksum = hash.Hash;
                            }

                            if (size == file.Size && Hashing.VerifyChecksum(checksum, file))
                            {
                                File.Move(tempfile, path, true);
                                return new UpdateResult(true, null);
                            }
                            else
                            {
                                Console.WriteLine("size or hash did not match manifest from source {0}", source.URL);
                                File.Delete(tempfile);
                            }
                        }
                        else
                        {
                            Console.WriteLine("cannot get file from source {0}", source.URL);
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == token)
                    {
                        Console.WriteLine("update stopped due to cancellation request");
                        return new UpdateResult(false, "");
                    }

                    Console.WriteLine("OperationCanceledException while downloading source {0}: {1}", source.URL, ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception while downloading source {0}: {1}", source.URL, ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("inner exception: {0}", ex.InnerException.Message);
                    }
                }
                finally
                {
                    //server.ProgressDesc = null;
                    //server.ProgressValueFile = 0;
                    //server.ProgressMaxFile = 0;
                }
            }
            return new UpdateResult(false, "Could not update file " + file.Path);
        }

        private async Task<bool> Checkfile(ManifestFile file, Server server, int index)
        {
            var path = Path.Combine(server.InstallPath, file.Path);
            Console.WriteLine("checking {0}", path);

            if (!File.Exists(path))
            {
                return file.Size == 0;
            }
            else if (file.Size == 0)
            {
                File.Delete(path);
                return true;
            }

            using (var hash = Hashing.GetHashAlgorithm(file))
            {
                if (hash == null) return false;

                byte[] checksum;
                long size = 0;
                try
                {
                    var state = new FileProgressState();
                    state.Desc = "verifying " + file.Path;
                    state.Max = file.Size;
                    server.ProgressState.Update(index, state);

                    using (FileStream filestream = new FileStream(path, FileMode.Open))
                    using (var hashstream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
                    {
                        size = await CopyToProgressFileAsync(filestream, hashstream, 81920, server, index, CancellationToken.None);
                        hashstream.FlushFinalBlock();
                        checksum = hash.Hash;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception while verifying {0}: {1}", file.Path, ex.Message);
                    return false;
                }

                return (size == file.Size && Hashing.VerifyChecksum(checksum, file));
            }
        }

        private async Task<long> CopyToProgressFileAsync(Stream fromStream, Stream destination, int bufferSize, Server server, int index, CancellationToken cancellationToken)
        {
            //server.ProgressValueFile = 0;
            var buffer = new byte[bufferSize];
            long size = 0;
            int count;
            while ((count = await fromStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
            {
                size += count;
                server.ProgressState.Update(index, size);
                await destination.WriteAsync(buffer, 0, count, cancellationToken);
            }
            return size;
        }
    }

    public struct UpdateResult
    {
        public bool Success;
        public string Message;

        public UpdateResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
