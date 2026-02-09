using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DerbyApp.Helpers
{
    public class GoogleDriveAccess
    {
        private const string _applicationName = "DerbyApp";
        private const string _imageFolderName = "Images";
        private DriveService _driveService;
        private string _folderId;

        public GoogleDriveAccess(Credentials credentials)
        {
            _ = GetDriveService(credentials);
        }

        private async Task GetDriveService(Credentials credentials)
        {
            UserCredential credential;

            ClientSecrets secrets = new()
            {
                ClientId = credentials.GoogleDriveUsername,
                ClientSecret = credentials.GoogleDrivePassword
            };

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                [DriveService.Scope.Drive],
                "user",
                CancellationToken.None,
                new FileDataStore("DriveApiTokenStorage") // Specify a folder name for token storage
            );

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });

            FilesResource.ListRequest request = _driveService.Files.List();
            request.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            request.Fields = "files(id, name)";
            IList<Google.Apis.Drive.v3.Data.File> folders = request.Execute().Files;
            if (folders != null && folders.Count > 0)
            {
                foreach (var folder in folders)
                {
                    if (folder.Name == _imageFolderName)
                    {
                        _folderId = folder.Id;
                        break;
                    }
                }
            }
        }

        public void DownloadAllFiles(string path)
        {
            FilesResource.ListRequest listRequest = _driveService.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(mimeType, name, id)";

            var files = listRequest.Execute().Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.MimeType != "application/vnd.google-apps.folder")
                    {
                        using var stream = new FileStream(Path.Combine(path, file.Name), FileMode.Create, FileAccess.Write);
                        _driveService.Files.Get(file.Id).Download(stream);
                    }
                }
            }
        }

        public string UploadFile(string fileName, Stream stream, string mimeType = "image/png")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = [_folderId]
            };

            FilesResource.CreateMediaUpload request;
            using (stream)
            {
                request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.SupportsAllDrives = true;
                request.Fields = "webViewLink";
                request.ProgressChanged += progress =>
                {
                    switch (progress.Status)
                    {
                        case UploadStatus.Failed:
                            Console.WriteLine($"Upload failed: {progress.Exception.Message}");
                            break;
                    }
                };
                request.Upload();
            }
            if (request != null) return request.ResponseBody.WebViewLink;
            else return "https://drive.google.com/file/d/1DHtfRFSuN279DoAP-bOmAKmuApYrFFyu/view?usp=drive_link";
        }
    }
}
