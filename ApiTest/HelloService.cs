using System;
using ServiceStack;

namespace ApiTest
{
    [Route("/hello/{Name}", "GET", Summary = @"Hello somebody", Notes = @"Welcomes the caller")]
    [ApiVersion("2018-08-01")]
    public class Hello : IReturn<HelloResponse> {
        [ApiMember(Description = "Name of the caller", IsRequired = true)]
        public string Name { get; set; }

        [ApiMember(Description = "Age of the caller", IsRequired = false)]
        [ApiVersion("2018-08-21")]
        public string Age { get; set; }
    }

    public class HelloResponse {
        public string Result { get; set; }
    }

    public class HelloService : Service
    {
        public object Any(Hello request) 
        {
            return new HelloResponse { Result = "Hello, " + request.Name };
        }
    }
}