using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Tools.Models;

namespace Tools.Pages {
    public class UploadResult {
        public bool Uploaded { get; set; }

        public string FileName { get; set; }

        public string StoredFileName { get; set; }

        public int ErrorCode { get; set; }
    }
    public partial class QuestionBankConverter : ComponentBase {
        private string tmp { get; set; }
        private IList<File> files = new List<File>();
        private IList<UploadResult> uploadResults = new List<UploadResult>();
        private int maxAllowedFiles = 3;
        private bool shouldRender;
        private W2MModel model = new W2MModel();
        private QuestionBank SourceType { get; set; }
        private QuestionBank TargetType { get; set; }
        private List<byte[]> InputFiles { get; set; }

        public QuestionBankConverter() : base() {
            InputFiles = new List<byte[]>();
        }

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
            string fileName, ILogger<QuestionBankConverter> logger, out UploadResult result) {
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

        async Task LoadFiles(InputFileChangeEventArgs e) {
            var isLoading = true;
            var exceptionMessage = string.Empty;

            try {
                foreach (var file in e.GetMultipleFiles(maxAllowedFiles)) {
                    //using var reader =
                    //    new StreamReader(file.OpenReadStream());
                    using (var stream = new MemoryStream()) {
                        await file.OpenReadStream().CopyToAsync(stream);
                        stream.Position = 0;
                        InputFiles.Add(stream.ToArray());
                    }

                    //loadedFiles.Add(file, await reader.ReadToEndAsync());
                }
            } catch (Exception ex) {
                exceptionMessage = ex.Message;
            }

            isLoading = false;
        }

        private void SelectSource_OnChange(ChangeEventArgs e) {
            var value = e.Value.ToString();
            if (string.IsNullOrEmpty(value)) {
                return;
            }
            int number;
            if (int.TryParse(value, out number)) {
                SourceType = (QuestionBank)number;
            }
        }

        private void SelectTarget_OnChange(ChangeEventArgs e) {
            var value = e.Value.ToString();
            if (string.IsNullOrEmpty(value)) {
                return;
            }
            int number;
            if (int.TryParse(value, out number)) {
                TargetType = (QuestionBank)number;
            }
        }

        private void Process() {

        }

        private class File {
            public string Name { get; set; }
        }

        class W2MModel {
            public QuestionBank questionBank { get; set; }
        }
    }
}