using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResumeTextExtract.Models;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Azure;
using Spire.Pdf.Exporting.Text;
using System.Text;
using Spire.Pdf;
using System.Globalization;
using System.Collections;

namespace ResumeTextExtract.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("02e06cff2f1648cb907a656830c9cf6d");
        private static readonly Uri endpoint = new Uri("https://resumetextextraction.cognitiveservices.azure.com/");
        private static int matchperc = 0;
        private string candidateName;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MultiFile()
        {
            MultipleFilesModel model = new MultipleFilesModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult MultiUpload(MultipleFilesModel model)
        {
            if (ModelState.IsValid)
            {
                model.IsResponse = true;
                if (model.Files.Count > 0)
                {
                    foreach (var file in model.Files)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);


                        string fileNameWithPath = Path.Combine(path, file.FileName);

                        if (Directory.Exists(fileNameWithPath))
                             Directory.Delete(fileNameWithPath);

                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        string docPath = path + '/' + file.FileName;
                        string extractedText = "";
                        var client = new TextAnalyticsClient(endpoint, credentials);
                        if (ext == ".pdf")
                            extractedText += ReadTextfromPdfFile(docPath);
                        else
                            extractedText += ReadTextfromFile(docPath);

                        //var data = ExtractKeys(client, extractedText);
                        var modelValue = model.SkillSet;
                        var data = ExtractKeys(client, extractedText,".Net","C#");
                        //model.Data = "";
                        //foreach (string s in data)
                        //{

                        //    model.Data += "\t" +s + "\t";
                        //}
                        model.DataMessage = file.FileName +" MatchPerc  " + data + "%";
                        // model.DataMessage = "Match Percentage" + Convert.ToString(matchperc) + "%";
                    }
                    model.IsSuccess = true;
                    model.Message = "Files upload successfully" ;
                }
                else
                {
                    model.IsSuccess = false;
                    model.Message = "Please select files";
                }
            }
            return View("MultiFile", model);
        }
        [HttpPost("FileUpload")]
        public IActionResult UploadFile(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            string filepath = "";
            var filePaths = new List<string>();
            foreach(var formFile in files)
            {
                if(formFile.Length > 0)
                {
                    filepath = "C:\\Users\\Sravank\\source\\repos\\ResumeTextExtract\\ResumeTextExtract\\wwwroot\\Uploads";

                    string path = Path.Combine(filepath, formFile.FileName);
                    filePaths.Add(filepath);
                    using(var stream = new FileStream(filepath,FileMode.Create))
                    {
                        formFile.CopyToAsync(stream);
                    }

                    var text = ReadTextfromFile(path);

                }
            }
            return View();
        }
        static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
        //public static IEnumerable<string> SplitText(this String value, int desiredLength)
        //{
        //    var characters = StringInfo.GetTextElementEnumerator(value);
        //    while (characters.MoveNext())
        //        yield return String.Concat(Take(characters, desiredLength));
        //}

        //private static IEnumerable<string> Take(TextElementEnumerator enumerator, int count)
        //{
        //    for (int i = 0; i < count; ++i)
        //    {
        //        yield return (string)enumerator.Current;

        //        if (!enumerator.MoveNext())
        //            yield break;
        //    }
        //}

        public static int ExtractKeys(TextAnalyticsClient client,string text,string domain,string skill)
        {
            Dictionary<string, string> candDict = new Dictionary<string, string>();
            List<string> lstkeys = new List<string>();
            Dictionary<string, string> extractedSkills = new Dictionary<string, string>();
            if (text.Length > 5000)
            {
                var textsplitted2 = TextSplit.SplitText(text, 5000).ToList();
                foreach (string str in textsplitted2)
                {
                    var response = client.RecognizeEntities(str).Value;
                    if (response.Any(x => x.Text.Contains(domain)) && !extractedSkills.ContainsValue(domain))
                    {
                        extractedSkills.Add("Domain", domain);
                        matchperc += 10;
                    }
                    else if (response.Any(x => x.Text.Contains(skill)) && !extractedSkills.ContainsValue(skill))
                    {
                        extractedSkills.Add("Skill", skill);
                        matchperc += 10;
                    }

                }

                //{
                   // extractedSkills.Add(response.Select(x => x.Category).ToString(), response.Select(x => x.Text).ToString());
                    
                //}
                //foreach(string s in textsplitted)
                //{
                //    var response = client.RecognizeEntities(s).Value;
                //    if (response.Any(x => x.Category == "Skill" && x.Text.Contains(".Net")))
                //    {
                //        extractedSkills.Add(response.Select(x => x.Category).ToString(), response.Select(x => x.Text).ToString());
                //    }
                //}
                //var textsplit = text.Split(" ", text.Length / 5000);


                //if(response.val)
            }
            else
            {
                var response1 = client.RecognizeEntities(text).Value;
                if (response1.Any(x => x.Text.Contains(domain)) || response1.Any(x => x.Text.Contains(skill)))
                    matchperc += 10;
            }
            // var text2 = text.Substring(text.Length / 3 , text.Length);

            //var response = client.ExtractKeyPhrases(text1);
            //var response1 = client.RecognizeEntities(text);

            //foreach(var key in response1.Value)
            //{
            //    if (response.Value.Contains(".Net"))
            //        matchperc += matchperc + 10;
            //    else if (response.Value.Contains("C#"))
            //        matchperc += matchperc + 20;
            //}
            //foreach (var word in text2)
            //{
            //    var response = client.ExtractKeyPhrases(word);
            //    var response1 = client.RecognizeEntities(word);
            //    if (response.Value.Contains(".Net"))
            //        matchperc += matchperc + 10;
            //    else if (response.Value.Contains("C#"))
            //        matchperc += matchperc + 20;



            //    foreach (string keyphrase in response.Value)
            //    {

            //        lstkeys.Add(keyphrase);
            //    }
            //}


            //if (response.ToString().Contains(".Net"))

            //return lstkeys;
            return matchperc;
        }
        public string ReadTextfromFile(string path)
        {
            Document doc = new Document();
            doc.LoadFromFile(path);
            string text1 = doc.GetText();
            return text1;
        }

        public string ReadTextfromPdfFile(string path)
        {
            SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            StringBuilder str = new StringBuilder();
            PdfDocument pdfDoc = new PdfDocument();
            pdfDoc.LoadFromFile(path);
            foreach(PdfPageBase page in pdfDoc.Pages)
            {
                str.Append(page.ExtractText(strategy));
            }
            return str.ToString();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
