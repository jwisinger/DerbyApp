using DerbyApp.RaceStats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DerbyApp.Helpers
{
    internal class ImageDownloader
    {
        struct RacersToUpdate
        {
            public string PhotoFileName;
            public List<Racer> RacerToUpdate;
        }
        private static readonly HttpClient _httpClient = new();
        private static readonly List<RacersToUpdate> _racersInProgress= [];

        public static void SetPhoto(Racer racer, string fileName)
        {
            byte[] fileBytes = File.ReadAllBytes(fileName);

            Image photo;
            using (MemoryStream ms = new(fileBytes))
            using (Image tmp = Image.FromStream(ms))
            {
                photo = new Bitmap(tmp);
            }

            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        racer.Photo = photo;
                    }
                    catch { }
                }));
            }
            else
            {
                racer.Photo = photo;
            }
        }

        public static async Task DownloadImageAsync(string imageUrl, string destinationPath, string fileName, Racer racer)
        {
#warning GOOGLE: This should really happen before it goes in the database
            imageUrl = imageUrl.Replace("file/d/", "uc?export=download&id=");
            imageUrl = imageUrl.Replace("/view?usp=drivesdk", "");

            fileName = Path.Combine(destinationPath, fileName);
            IEnumerable<RacersToUpdate> matchingRacers = _racersInProgress.Where(x => x.PhotoFileName == fileName);
            if (matchingRacers.Any())
            {
                matchingRacers.ToList()[0].RacerToUpdate.Add(racer);
            }
            else
            {
                RacersToUpdate toUpdate = new()
                {
                    PhotoFileName = fileName,
                    RacerToUpdate = [racer]
                };
                _racersInProgress.Add(toUpdate);
                try
                {
                    using var httpResponse = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
                    httpResponse.EnsureSuccessStatusCode();
                    using var mediaStream = await httpResponse.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    await mediaStream.CopyToAsync(fileStream);
                    fileStream.Close();
                }
                catch { }

                foreach (Racer r in toUpdate.RacerToUpdate)
                {
                    SetPhoto(r, fileName);
                }
            }
        }
    }
}
