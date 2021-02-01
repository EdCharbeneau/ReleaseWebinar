using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Common.FormatProviders;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using pdfProviderNamespace = Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Rtf;
using Telerik.Windows.Documents.Flow.FormatProviders.Txt;
using Telerik.Windows.Documents.Flow.Model;
using Microsoft.JSInterop;
using System.Net.Http;

namespace ReleaseWebinar.Client
{
    public class FileConverter
    {
        //private IWebHostEnvironment Environment { get; set; }
        private IJSRuntime _js { get; set; }

        private readonly HttpClient _http;


        /// <summary>
        /// Convert the HTML string to a file, download the generated file in the browser
        /// </summary>
        /// <param name="htmlContent">The HTML content to export</param>
        /// <param name="fileName">The FileName of the downloaded file, its extension is used to fetch the exprot provider</param>
        /// <returns>Returns true if the operation succeeded, false if there was an exception</returns>
        public async Task<bool> ExportAndDownloadHtmlContent(string htmlContent, string fileName)
        {

            RadFlowDocument document = CreateHtmlDocument(htmlContent);
            try
            {
                // get the provider to export and then download the file
                string mimeType;
                IFormatProvider<RadFlowDocument> exportProvider = GetExportFormatProvider(fileName, out mimeType);

                byte[] exportFileBytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    exportProvider.Export(document, ms);
                    exportFileBytes = ms.ToArray();
                }

                // download the file in the browser
                await FileDownloader.Save(_js, exportFileBytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                //'System.Net.WebClient is not supported on this platform. Use System.Net.Http.HttpClient instead.'
                return false;
            }
            return true;
        }

        private RadFlowDocument CreateHtmlDocument(string htmlContent)
        {
            try
            {


                // prepare a document with the HTML content that we can use for conversion
                HtmlFormatProvider provider = new();
                HtmlImportSettings importSettings = new();

                importSettings.LoadImageFromUri += async (s, e) =>
                {
                    // Load the data representing the resource  
                    var httpResponse = await _http.GetAsync(e.Uri);
                    byte[] data = null;
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        data = await httpResponse.Content.ReadAsByteArrayAsync();
                    }
                    if (data != null)
                    {
                        // Pass the loaded data to the arguments  
                        string extension = e.Uri.Substring(e.Uri.Length - 3);
                        e.SetImageInfo(data, extension);
                    }
                };
                provider.ImportSettings = importSettings;
                RadFlowDocument document = provider.Import(htmlContent);
                return document;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Supported providers for the export operation based on the file name, also gets its MIME type as an out parameter
        /// </summary>
        /// <param name="fileName">the file name you wish to export, the provider is discerned based on its extensiom</param>
        /// <param name="mimeType">an out parameter with the MIME type for this file so you can download it</param>
        /// <returns>IFormatProvider<RadFlowDocument> that you can use to export the original document to a certain file format</returns>
        IFormatProvider<RadFlowDocument> GetExportFormatProvider(string fileName, out string mimeType)
        {
            // we get both the provider and the MIME type to use only one swtich-case
            IFormatProvider<RadFlowDocument> fileFormatProvider;
            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".docx":
                    fileFormatProvider = new DocxFormatProvider();
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".rtf":
                    fileFormatProvider = new RtfFormatProvider();
                    mimeType = "application/rtf";
                    break;
                case ".html":
                    fileFormatProvider = new HtmlFormatProvider();
                    mimeType = "text/html";
                    break;
                case ".txt":
                    fileFormatProvider = new TxtFormatProvider();
                    mimeType = "text/plain";
                    break;
                case ".pdf":
                    fileFormatProvider = new pdfProviderNamespace.PdfFormatProvider();
                    mimeType = "application/pdf";
                    break;
                default:
                    fileFormatProvider = null;
                    mimeType = string.Empty;
                    break;
            }

            if (fileFormatProvider == null)
            {
                throw new NotSupportedException("The chosen format cannot be exported with the supported providers.");
            }

            return fileFormatProvider;
        }

        // DI for the environment feature we need - path to the wwwroot folder to read the intial content,
        // and JS runtime for downloading the file to the browser
        public FileConverter(IJSRuntime js, HttpClient httpClient)
        {
            /*Environment = env;*/
            _js = js;
            _http = httpClient;
        }
    }



}