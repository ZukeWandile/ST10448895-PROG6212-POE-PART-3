using Microsoft.AspNetCore.Hosting;

namespace ST10448895_CMCS_PROG.Tests
{
    public class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "ST10448895_CMCS_PROG";
        public string WebRootPath { get; set; } = "wwwroot";
        public string ContentRootPath { get; set; } = "contentroot";
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
    }
}
