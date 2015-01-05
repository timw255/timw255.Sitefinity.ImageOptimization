using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Libraries.Model;
using timw255.Sitefinity.ImageOptimization.Configuration;

namespace timw255.Sitefinity.ImageOptimization.Optimizer
{
    public class KrakenImageOptimizer : ImageOptimizerBase
    {
        private Kraken _krakenClient;

        private string _key;
        private string _secret;
        private bool _useCallbacks;
        private string _callbackUrl;
        private bool _useLossy;

        public KrakenImageOptimizer()
        {
            var settings = _config.Optimizers["KrakenImageOptimizer"].Parameters;

            _key = settings["apiKey"];
            _secret = settings["apiSecret"];
            _useCallbacks = Boolean.Parse(settings["useCallbacks"]);
            _callbackUrl = settings["callbackUrl"];
            _useLossy = Boolean.Parse(settings["useLossyOptimization"]);

            _krakenClient = new Kraken(_key, _secret);
        }

        public override Stream CompressImageData(Image image, Stream imageData, out string optimizedExtension)
        {
            KrakenRequest krakenRequest = new KrakenRequest();

            krakenRequest.Lossy = _useLossy;

            if (_useCallbacks)
            {
                krakenRequest.CallbackUrl = _callbackUrl;
            }
            else
            {
                krakenRequest.Wait = true;
            }

            krakenRequest.File = ((MemoryStream)imageData).ToArray();

            var response = _krakenClient.Upload(krakenRequest, image.Id.ToString(), image.Extension);

            if (_useCallbacks)
            {
                Kraken.KrakenCallbackIds.Add(response.Id, image.Album.Id);

                optimizedExtension = "";
                return null;
            }
            else
            {
                if (response.Success == false || response.Error != null)
                {
                    optimizedExtension = "";
                    return null;
                }

                using (var webClient = new WebClient())
                {
                    var stream = webClient.OpenRead(response.KrakedUrl);

                    optimizedExtension = Path.GetExtension(response.KrakedUrl);
                    return stream;
                }
            }
        }

        public Stream ProcessCallback(object data, out Guid imageId, out Guid albumId, out string optimizedExtension)
        {
            FormDataCollection kCallbackFormData = data as FormDataCollection;

            string krakenCallbackId = kCallbackFormData.Get("id");

            if (!Kraken.KrakenCallbackIds.ContainsKey(krakenCallbackId))
            {
                albumId = Guid.Empty;
                imageId = Guid.Empty;
                optimizedExtension = null;
                return null;
            }

            string fileName = kCallbackFormData.Get("file_name");
            string krakedUrl = kCallbackFormData.Get("kraked_url");
            bool success = Boolean.Parse(kCallbackFormData.Get("success"));
            string error = kCallbackFormData.Get("error");

            Kraken.KrakenCallbackIds.Remove(krakenCallbackId);

            if (success == false || error != null)
            {
                albumId = Guid.Empty;
                imageId = Guid.Empty;
                optimizedExtension = null;
                return null;
            }

            imageId = Guid.Parse(Path.GetFileNameWithoutExtension(krakedUrl));
            albumId = Kraken.KrakenCallbackIds[krakenCallbackId];

            if (!Regex.IsMatch(krakedUrl, @"https?://(?:api-worker-\d|dl).kraken.io/" + krakenCallbackId + "/" + imageId.ToString() + @"\.(?:jpg|jpeg|png|gif|svg)"))
            {
                albumId = Guid.Empty;
                imageId = Guid.Empty;
                optimizedExtension = null;
                return null;
            }

            using (var webClient = new WebClient())
            {
                optimizedExtension = Path.GetExtension(fileName);
                return webClient.OpenRead(krakedUrl);
            }
        }
    }
}
