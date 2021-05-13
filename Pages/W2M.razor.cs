using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace CICalc.Pages {
    public class UploadResult {
        public bool Uploaded { get; set; }

        public string FileName { get; set; }

        public string StoredFileName { get; set; }

        public int ErrorCode { get; set; }
    }
    public partial class W2M : ComponentBase {
        private IList<File> files = new List<File>();
        private IList<UploadResult> uploadResults = new List<UploadResult>();
        private int maxAllowedFiles = 3;
        private bool shouldRender;

        protected override bool ShouldRender() => shouldRender;

        private async Task OnInputFileChange(InputFileChangeEventArgs e) {
            shouldRender = false;
            long maxFileSize = 1024 * 1024 * 15;
            var upload = false;

            using var content = new MultipartFormDataContent();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles)) {
                if (uploadResults.SingleOrDefault(
                    f => f.FileName == file.Name) is null) {
                    var fileContent = new StreamContent(file.OpenReadStream());

                    files.Add(
                        new File() {
                            Name = file.Name,
                        });

                    if (file.Size < maxFileSize) {
                        content.Add(
                            content: fileContent,
                            name: "\"files\"",
                            fileName: file.Name);

                        upload = true;
                    } else {
                        //Logger.LogInformation("{FileName} not uploaded", file.Name);

                        uploadResults.Add(
                            new UploadResult() {
                                FileName = file.Name,
                                ErrorCode = 6,
                                Uploaded = false,
                            });
                    }
                }
            }

            //if (upload) {
            //    var response = await HttpClient.PostAsync("/Filesave", content);

            //    var newUploadResults = await response.Content
            //        .ReadFromJsonAsync<IList<UploadResult>>();

            //    uploadResults = uploadResults.Concat(newUploadResults).ToList();
            //}

            shouldRender = true;
        }

        private static bool FileUpload(IList<UploadResult> uploadResults,
            string fileName, ILogger<W2M> logger, out UploadResult result) {
            if (uploadResults is null) {
                throw new System.ArgumentNullException(nameof(uploadResults));
            }

            if (string.IsNullOrEmpty(fileName)) {
                throw new System.ArgumentException($"“{nameof(fileName)}”不能是 Null 或为空", nameof(fileName));
            }

            if (logger is null) {
                throw new System.ArgumentNullException(nameof(logger));
            }

            result = uploadResults.SingleOrDefault(f => f.FileName == fileName);

            if (result is null) {
                logger.LogInformation("{FileName} not uploaded", fileName);
                result = new UploadResult();
                result.ErrorCode = 5;
            }

            return result.Uploaded;
        }

        private class File {
            public string Name { get; set; }
        }
    }
}