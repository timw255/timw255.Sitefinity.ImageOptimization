using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Libraries.Model;
using timw255.Sitefinity.ImageOptimization.Configuration;

namespace timw255.Sitefinity.ImageOptimization.Optimizer
{
    public class KrakenIOImageOptimizer : IImageOptimizer
    {
        private ImageOptimizationConfig _config;

        private Kraken _krakenClient;

        private string _key;
        private string _secret;
        private bool _useCallbacks;
        private string _callbackUrl;
        private bool _useLossy;

        public Guid AlbumId { get; set; }

        public KrakenIOImageOptimizer()
        {
            _config = Config.Get<ImageOptimizationConfig>();

            var settings = _config.Optimizers["KrakenIOImageOptimizer"].Parameters;

            _key = settings["apiKey"];
            _secret = settings["apiSecret"];
            _useCallbacks = Boolean.Parse(settings["useCallbacks"]);
            _callbackUrl = settings["callbackUrl"];
            _useLossy = Boolean.Parse(settings["useLossyOptimization"]);

            _krakenClient = new Kraken(_key, _secret);
        }

        public Stream OptimizeImage(Image image, Stream imageData, out string optimizedExtension)
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
                Kraken.KrakenCallbackIds.Add(response.Id, AlbumId);

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
    }
}
