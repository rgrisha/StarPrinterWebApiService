using System.Net;
using System.Text;
using Moq;
using System.IO;
using Xunit;

namespace StarPrinterWebServiceTests
{


    public class HttpServiceTests
    {
        [Fact]
        public void Test_HelloEndpoint_ReturnsCorrectJson()
        {
            // Arrange
            var listenerMock = new Mock<HttpListener>();
            var contextMock = new Mock<HttpListenerContext>();
            var responseMock = new Mock<HttpListenerResponse>();
            var requestMock = new Mock<HttpListenerRequest>();

            // Set up the mock behavior
            contextMock.Setup(c => c.Request).Returns(requestMock.Object);
            contextMock.Setup(c => c.Response).Returns(responseMock.Object);

            listenerMock.Setup(l => l.GetContext()).Returns(contextMock.Object);

            var service = new StarPrinterWebApiService(listenerMock.Object);
            service.Start("http://localhost:5000/");

            // Simulate a request to the /api/hello endpoint
            requestMock.Setup(r => r.Url.AbsolutePath).Returns("/api/hello");
            requestMock.Setup(r => r.HttpMethod).Returns("GET");

            // Act
            service.ProcessRequest(contextMock.Object); // Directly calling the method to process the mock request

            // Assert: Check that the response is in the correct JSON format
            responseMock.Verify(r => r.OutputStream.Write(It.IsAny<byte[]>(), 0, It.IsAny<int>()), Times.Once);

            // Validate the response data
            var expectedResponse = "{\"message\":\"Hello from the REST API!\"}";
            responseMock.Verify(r => r.OutputStream.Write(It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedResponse), 0, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Test_EchoEndpoint_ReturnsEchoedData()
        {
            // Arrange
            var listenerMock = new Mock<HttpListener>();
            var contextMock = new Mock<HttpListenerContext>();
            var responseMock = new Mock<HttpListenerResponse>();
            var requestMock = new Mock<HttpListenerRequest>();
            var streamMock = new Mock<Stream>();

            // Set up the mock behavior
            contextMock.Setup(c => c.Request).Returns(requestMock.Object);
            contextMock.Setup(c => c.Response).Returns(responseMock.Object);
            contextMock.Setup(c => c.Request.InputStream).Returns(streamMock.Object);

            listenerMock.Setup(l => l.GetContext()).Returns(contextMock.Object);

            var service = new StarPrinterWebApiService(listenerMock.Object);
            service.Start("http://localhost:5000/");

            // Simulate a POST request to the /api/echo endpoint with "Hello World" data
            requestMock.Setup(r => r.Url.AbsolutePath).Returns("/api/echo");
            requestMock.Setup(r => r.HttpMethod).Returns("POST");
            streamMock.Setup(s => s.Read(It.IsAny<byte[]>(), 0, It.IsAny<int>())).Returns(13);
            streamMock.Setup(s => s.Position).Returns(0);

            // Simulate the data coming in as a byte array
            var requestData = Encoding.UTF8.GetBytes("Hello World");
            streamMock.Setup(s => s.Read(It.IsAny<byte[]>(), 0, It.IsAny<int>())).Returns(requestData.Length);

            // Act
            service.ProcessRequest(contextMock.Object); // Process the mock request

            // Assert: Check the response body
            var expectedResponse = "{\"message\":\"Received data\",\"data\":\"Hello World\"}";
            responseMock.Verify(r => r.OutputStream.Write(It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedResponse), 0, It.IsAny<int>()), Times.Once);
        }
    }

}