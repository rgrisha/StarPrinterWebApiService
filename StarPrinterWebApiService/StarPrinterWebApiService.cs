
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using StarPrinterWebServiceAppNamespace;

public class StarPrinterWebApiService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private HttpListener _httpListener;
    private Thread _listenerThread;
    private bool _listenerIsWorking = true;
    private Templates templates;
    private Printer _printer;

    public StarPrinterWebApiService(Templates templates, Printer printer, HttpListener listener = null)
    {
        _httpListener = listener ?? new HttpListener();
        this.templates = templates;
        this._printer = printer;
    }

    public void Start(string urlPrefix)
    {
        _httpListener.Prefixes.Add(urlPrefix);  // Define the URL to listen on
        _listenerThread = new Thread(new ThreadStart(ListenForRequests));
        _listenerThread.Start();
    }

    private void ListenForRequests()
    {
        _httpListener.Start();
        while (_listenerIsWorking)
        {
            try
            {
                var context = _httpListener.GetContext(); // Block until a request comes in
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                // Log or handle any exceptions that occur while listening
                Console.WriteLine($"Error while processing request: {ex.Message}");
                continue;
            }
        }
    }

    public void ProcessRequest(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;
        string returnMessage = string.Empty;

        try
        {
            (int retCode, string message) = ProcessRequestImpl(request, response);
            response.StatusCode = retCode;
            returnMessage = message;
        }
        catch(Exception ex)
        {
            string errorId = Guid.NewGuid().ToString(); 
            logger.Error(ex, errorId);

            response.StatusCode = 500;
            returnMessage = CreateJsonResponse(new { error = "Error " + errorId });
        }

        // Write the JSON response
        byte[] buffer = Encoding.UTF8.GetBytes(returnMessage);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "application/json";
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private (int, string) ProcessRequestImpl(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (request.Url.AbsolutePath == "/api/printer/status" && request.HttpMethod == "GET")
        {
            var data = new { id = 1, name = "Test Data" };
            return (200, CreateJsonResponse(data));
        }
        else if (request.Url.AbsolutePath == "/api/printer/jobs" && request.HttpMethod == "POST")
        {

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string requestBody = reader.ReadToEnd();
                logger.Info("Received request: {requestBody}", requestBody);
                try
                {

                    TemplateId template = JsonConvert.DeserializeObject<TemplateId>(requestBody);
                    if (template.templateId == null)
                    {
                        return (400, CreateJsonResponse(new { error = "TEMPLATE_NOT_SPECIFIED" }));
                    }   

                    IPrintTemplate printTemplate = templates.findTemplate(template.templateId);
                    if (printTemplate == null)
                    {
                        return (400, CreateJsonResponse(new { error = "TEMPLATE_NOT_FOUND" }));
                    }   

                    printTemplate = printTemplate.fromJson(requestBody);

                    _printer.printTemplate(printTemplate);

                    var echoResponse = new { message = "OK" };
                    return (200, CreateJsonResponse(echoResponse));
                }
                catch (JsonSerializationException ex)
                {
                    return (400, CreateJsonResponse(new { error = "JSON_INVALID" }));
                }
            }
        }
        else
        {
            // Invalid endpoint
            return (404, CreateJsonResponse(new { error = "NOT_FOUND" }));
        }
   }

    private string CreateJsonResponse(object data)
    {
        // Serialize the object to JSON string
        return JsonConvert.SerializeObject(data);
    }

    public void Stop()
    {
        _listenerIsWorking = false;
        _httpListener.Stop();
        _listenerThread.Join();
    }

    private class TemplateId
    {
        public string templateId;
    }
}
