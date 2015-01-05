using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.Tasks;

namespace timw255.Sitefinity.ImageOptimization.MVC.Controllers
{
    public class KrakenIOCallbackController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post([FromBody] FormDataCollection kCallbackFormData)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            if (Kraken.KrakenCallbackIds.ContainsKey(kCallbackFormData.Get("Id")))
            {
                ProcessCallback(kCallbackFormData);

                response.StatusCode = HttpStatusCode.OK;
            }

            response.StatusCode = HttpStatusCode.Forbidden;

            return response;
        }

        private void ProcessCallback(FormDataCollection kCallbackFormData)
        {
            string krakenCallbackId = kCallbackFormData.Get("id");
            string fileName = kCallbackFormData.Get("file_name");
            string krakedUrl = kCallbackFormData.Get("kraked_url");
            bool success = Boolean.Parse(kCallbackFormData.Get("success"));
            string error = kCallbackFormData.Get("error");

            Guid albumId = Kraken.KrakenCallbackIds[krakenCallbackId];

            Kraken.KrakenCallbackIds.Remove(krakenCallbackId);

            if (success == false || error != null)
            {
                return;
            }

            LibrariesManager _librariesManager = LibrariesManager.GetManager();

            _librariesManager.Provider.SuppressSecurityChecks = true;

            Album album = _librariesManager.GetAlbum(albumId);

            var albumProvider = (LibrariesDataProvider)album.Provider;

            Image image = _librariesManager.GetImage(Guid.Parse(Path.GetFileNameWithoutExtension(krakedUrl)));

            if (!Regex.IsMatch(krakedUrl, @"https?://(?:api-worker-\d|dl).kraken.io/" + krakenCallbackId + "/" + image.Id.ToString() + @"\.(?:jpg|jpeg|png|gif|svg)"))
            {
                return;
            }

            using (var webClient = new WebClient())
            using (var stream = webClient.OpenRead(krakedUrl))
            {   
                // Check out the master to get a temp version.
                Image temp = _librariesManager.Lifecycle.CheckOut(image) as Image;

                // Make the modifications to the temp version.
                _librariesManager.Upload(temp, stream, Path.GetExtension(fileName));

                temp.SetValue("Optimized", true);

                // Check in the temp version.
                // After the check in the temp version is deleted.
                _librariesManager.Lifecycle.CheckIn(temp);

                Image liveImage = (Image)_librariesManager.Lifecycle.GetLive(image);

                liveImage.FileId = image.FileId;
                liveImage.SetValue("Optimized", true);

                _librariesManager.SaveChanges();
            }

            _librariesManager.Provider.SuppressSecurityChecks = false;
        }
    }
}
