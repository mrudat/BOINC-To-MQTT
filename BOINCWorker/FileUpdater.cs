using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Xml.Linq;

namespace BOINCWorker;

internal class FileUpdater(IFileSystem fileSystem, string dataPath, string FileName)
{
    private readonly IFile File = fileSystem.File;
    private readonly IFileStreamFactory FileStream = fileSystem.FileStream;

    readonly string filePath = fileSystem.Path.Combine(dataPath, FileName);
    readonly string temporaryFilePath = fileSystem.Path.Combine(dataPath, $"{FileName}.tmp");
    readonly string backupFilePath = fileSystem.Path.Combine(dataPath, $"{FileName}.bak");

    internal async Task Update(Action<XDocument> UpdateAction, CancellationToken cancellationToken = default)
    {
        // hang on to the file handle to prevent anyone else from writing to this file.
        using (var readFileHandle = await Task.Run(() => FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), cancellationToken))
        {
            Task? removeBackupFileTask = File.Exists(backupFilePath) ? Task.Run(() => File.Delete(backupFilePath), cancellationToken) : null;

            var document = await XDocument.LoadAsync(readFileHandle, LoadOptions.PreserveWhitespace, cancellationToken);

            UpdateAction(document);

            using (var writeFileHandle = await Task.Run(() => FileStream.New(temporaryFilePath, FileMode.Create, FileAccess.Write, FileShare.None), cancellationToken))
            {
                await document.SaveAsync(writeFileHandle, SaveOptions.None, cancellationToken);
            }

            if (removeBackupFileTask != null)
                await removeBackupFileTask;

            await Task.Run(() => File.Replace(temporaryFilePath, filePath, backupFilePath), cancellationToken);
        }

        await Task.Run(() => File.Delete(backupFilePath), cancellationToken);
    }
}