//using NetFusion.Rest.Client;
//using System;
//using Xunit;
//using NetFusion.Common.Extensions;

//namespace WebTests.Rest.ClientRequests
//{
//    public class TempTest
//    {
//        [Fact]
//        public void TestRequestClientBuilder()
//        {
//            var client = RequestClientBuilder.ForBaseAddress("http://localhost:8086")
//                .ForEntryPoint("v1.0/device/entries")
//                .AddRequestCorrelationId()
//                .BeforeEachRequest(settings =>
//                {
//                    Console.WriteLine(settings.ToIndentedJson());
//                })
//                .Build();

//            try
//            {
//                var e = client.GetApiEntry().Result;
//            }
//            catch
//            {

//            }



//            var e2 = client.GetApiEntry().Result; ;

//        }
//    }
//}
