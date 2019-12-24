namespace Tree.Pages
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        #region Fields

        private readonly ILogger<ErrorModel> _logger;

        #endregion

        #region Properties

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        #endregion

        #region Constructors

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Methods

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }

        #endregion
    }
}
