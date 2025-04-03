using Microsoft.Extensions.Configuration;

namespace MyTestProject.Service.Tests.Common;

public class ConfigurationFactory 
{
    protected ConfigurationFactory()
    {        
    }

    public static IConfiguration GetConfiguration() 
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("./appsettings.Test.json");
        return builder.Build();
    }
}