namespace StaticSite.Middlewares
{
    /// <summary>
    /// 该中间件静态化页面
    /// </summary>
    public class StaticPageMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly string StaticPagePath = Path.Combine(Environment.CurrentDirectory, "static");

        private static TimeSpan StaticPageTtl = TimeSpan.FromHours(2);


        public StaticPageMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) is false)
            {
                await _next.Invoke(context);
                return;
            }

            string pageFilePath = GenerateStaticPageFilePath(context.Request.Path);

            // 如果静态文件过期
            if (File.Exists(pageFilePath)
                && DateTime.UtcNow - new FileInfo(pageFilePath).LastWriteTimeUtc > StaticPageTtl)
            {
                File.Delete(pageFilePath);
            }

            if (File.Exists(pageFilePath))
            {
                await WriteWithFileAsync(context.Response.Body, pageFilePath);
                return;
            }

            var originalStream = context.Response.Body;
            using var stream = new MemoryStream();
            context.Response.Body = stream;

            // TODO 需要防止同时请求时候的，同时读写同一文件
            await _next.Invoke(context);

            stream.Position = 0;
            await SaveFileAsync(stream, pageFilePath);

            stream.Position = 0;
            await stream.CopyToAsync(originalStream);
            context.Response.Body = originalStream;

        }


        private string GenerateStaticPageFilePath(string requestPath)
        {
            var filePath = Path.Join(StaticPagePath, requestPath);

            if (filePath.EndsWith("/"))
                filePath = filePath[..(filePath.Length - 1)] + Path.DirectorySeparatorChar + "#index#";

            return filePath;
        }


        private static async Task WriteWithFileAsync(Stream stream, string pageFilePath)
        {
            var page = new FileStream(pageFilePath, FileMode.Open, FileAccess.Read);
            await page.CopyToAsync(stream);
        }


        private async Task SaveFileAsync(MemoryStream stream, string requestPath)
        {
            var directory = Path.GetDirectoryName(requestPath);
            Directory.CreateDirectory(directory!);

            // TODO 需要处理同时保存相同页面的问题
            var savePath = Path.Combine(StaticPagePath, requestPath);
            using var file = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(file);
        }
    }
}
