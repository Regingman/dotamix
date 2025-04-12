using Google.Apis.Auth.OAuth2;
using Google.Apis.Forms.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text.Json;

namespace dotamix.Services
{
    public class GoogleFormsService
    {
        private readonly string _credentialsPath;
        private readonly string _tokenPath;
        private readonly string[] _scopes = { FormsService.Scope.FormsResponsesReadonly };
        private FormsService _service;

        public GoogleFormsService(IConfiguration configuration)
        {
            _credentialsPath = configuration["GoogleForms:CredentialsPath"];
            _tokenPath = configuration["GoogleForms:TokenPath"];
        }

        private async Task InitializeServiceAsync()
        {
            if (_service != null) return;

            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(_tokenPath, true));

            _service = new FormsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public async Task<List<Dictionary<string, string>>> GetFormResponsesAsync(string formId)
        {
            await InitializeServiceAsync();

            var responses = await _service.Forms.Responses.List(formId).ExecuteAsync();
            var result = new List<Dictionary<string, string>>();

            foreach (var response in responses.Responses)
            {
                var answers = new Dictionary<string, string>();
                foreach (var answer in response.Answers)
                {
                    var value = answer.Value.TextAnswers?.Answers?.FirstOrDefault()?.Value ?? "";
                    answers[answer.Key] = value;
                }
                result.Add(answers);
            }

            return result;
        }
    }
} 