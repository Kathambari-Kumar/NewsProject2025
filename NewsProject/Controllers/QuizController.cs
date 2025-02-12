using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsProject.Extensions;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using NewsProject.Services;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NewsProject.Controllers
{
    public class QuizController : Controller
    {
       private readonly IQuizService _quizService;
       private readonly IConfiguration _configuration;

        public QuizController(IQuizService quizService, IConfiguration configuration)
        {
            _quizService = quizService;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            // Fetch all quiz IDs from the database
            var allQuizIds = await _quizService.GetAllQuizIdsAsync();

            // Shuffle the list of IDs and take the first 10
            var randomIds = allQuizIds.OrderBy(_ => Guid.NewGuid()).Take(10).ToList();

            if (!randomIds.Any()) // Check if no quiz IDs were found
            {
                return View("QuizNotFound"); // Or any appropriate view in case of an error
            }

            // Store the selected quiz IDs in session
            HttpContext.Session.Set("QuizIds", randomIds);

            // Set the current question index to 1
            HttpContext.Session.Set("CurrentQuestionIndex", 1);

            // Fetch the first question
            var quiz = await _quizService.GetQuizByIdAsync(randomIds.FirstOrDefault());

            if (quiz == null) // If no quiz was found
            {
                return View("QuizNotFound"); // Or any appropriate view
            }

            // Create and populate the view model
            var viewModel = new QuizViewModel
            {
                Quiz = quiz,
                CurrentQuestionNumber = 1 // The first question
            };

            // Pass the quiz question and current question number to the view
            ViewBag.CurrentQuestionNumber = 1; // First question is question 1
            ViewBag.TotalQuestions = randomIds.Count;

            // Pass the view model to the view
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetNextQuestion()
        {
            var randomIds = HttpContext.Session.Get<List<int>>("QuizIds");

            if (randomIds == null || !randomIds.Any())
            {
                return Json(new { message = "No more questions." });
            }

            int currentIndex = HttpContext.Session.Get<int>("CurrentQuestionIndex");

            if (currentIndex >= randomIds.Count)
            {
                return Json(new
                {
                    message = "Successfully completed the Quiz, Congratulations! want to take another quiz? click on the link below. ",
                    retryQuizLink = Url.Action("Index", "Quiz"),
                    completed = true // Indicator for completion
                });
            }

            var quiz = await _quizService.GetQuizByIdAsync(randomIds[currentIndex]);

            if (quiz == null)
            {
                return Json(new { message = "Error loading the question." });
            }

            HttpContext.Session.Set("CurrentQuestionIndex", currentIndex + 1);

            return Json(new
            {
                question = quiz.Question,
                option1 = quiz.Option1,
                option2 = quiz.Option2,
                option3 = quiz.Option3,
                correctAnswer = quiz.Answer,
                currentQuestionNumber = currentIndex + 1,
                totalQuestions = randomIds.Count,
                imageLink = quiz.ImageLink,
                completed = false // Indicator for ongoing quiz
            });
        }

        public async Task<IActionResult> QuizComplete()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuiz()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz(Quiz quiz)
        {
            // Blob Storage Process
            string imgurl = string.Empty;
            string connectionString = _configuration["AzureBlobConnectionString"];
            string containerName = _configuration["AzureBlobContainerName"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            BlobClient blobClient = containerClient.GetBlobClient(quiz.File.FileName);
            Stream outputStream = new MemoryStream();
            using (var stream = quiz.File.OpenReadStream())
            {
                using (Image image = Image.Load(stream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Stretch,
                        Size = new Size(800, 350)
                    }));
                    var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                    image.Save(outputStream, jpegEncoder);
                    outputStream.Position = 0;
                    blobClient.Upload(outputStream, true);
                    stream.Dispose();
                    imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                }
            }
            quiz.ImageLink= imgurl; // store the URL as ImageLink
            await _quizService.CreateQuizAsync(quiz);
            TempData["Result"] = "The question set is added successfully!!";
            return RedirectToAction("AdminEditorAuthorFrontPage", "Admin");
        }
        public async Task<IActionResult> QuizList()
        {
            var quizes = await _quizService.GetAllQuizListAsync();
            return View(quizes);
        }

        [HttpGet]
        public async Task<IActionResult> EditQuiz(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuiz(Quiz quiz)
        {
            // Blob Storage Process
            if (quiz.File != null)
            {
                string imgurl = string.Empty;
                string connectionString = _configuration["AzureBlobConnectionString"];
                string containerName = _configuration["AzureBlobContainerName"];
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.CreateIfNotExists();

                BlobClient blobClient = containerClient.GetBlobClient(quiz.File.FileName);

                Stream outputStream = new MemoryStream();
                using (var stream = quiz.File.OpenReadStream())
                {
                    using (Image image = Image.Load(stream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Stretch,
                            Size = new Size(800, 350)
                        }));
                        var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                        image.Save(outputStream, jpegEncoder);
                        outputStream.Position = 0;
                        blobClient.Upload(outputStream, true);
                        stream.Dispose();
                        imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                    }
                }
                quiz.ImageLink = imgurl; // store the URL as ImageLink            
            }

            await _quizService.UpdateQuizAsync(quiz);
            TempData["Result"] = "The quiz record is updated successfully!!";
            return RedirectToAction("QuizList");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(Quiz quiz)
        {
            await _quizService.DeleteQuizAsync(quiz);
            TempData["Result"] = "The quiz record is deleted successfully!!";
            return RedirectToAction("QuizList");
        }
        public async Task<IActionResult> QuizDetails(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            return View(quiz);
        }
    }
}
