using System;
using System.Security.Cryptography;
using System.Text;

public class LoginCookie(IConfiguration config, string serverIp)
{
    private readonly IConfiguration configuration = config;
    private readonly string Server = serverIp;
    private string Username = string.Empty;
    private string visibleName = string.Empty;
    private string Password = string.Empty;
    private string Salt = string.Empty;

    //Response Codes for the http requests
    public enum ResponseCodes
    {
        ResponseOK,
        UnknownHost,
        UnknownException
    }

    //Check the different exceptions that might happen during a HTTP Request, throws the exception if not a HTTP exception
    private static ResponseCodes checkException(Exception ex)
    {
        if (ex is AggregateException aggrEx)
        {
            foreach (var innerEx in aggrEx.Flatten().InnerExceptions)
            {
                if (innerEx is HttpRequestException httpEx)
                {
                    return errorToResponseCode(httpEx.HttpRequestError);
                }
                throw aggrEx;
            }
        }
        else if (ex is HttpRequestException httpEx)
        {
            return errorToResponseCode(httpEx.HttpRequestError);
        }
        throw ex;
    }

    // The switch statement that translates HTTP error codes to ResponseCodes
    private static ResponseCodes errorToResponseCode(HttpRequestError requestError)
    {
        return requestError switch
        {
            HttpRequestError.NameResolutionError => ResponseCodes.UnknownHost,
            _ => ResponseCodes.UnknownException,
        };
    }

    // Check if the server exists
    public ResponseCodes checkConnection()
    {
        string? port = configuration["PORT"];
        string ServerUrl = string.Concat(Server, ":", port, "/check");
        if (!(ServerUrl.Contains("http://")||ServerUrl.Contains("https://"))) ServerUrl = string.Concat("http://", ServerUrl);
        HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        try
        {
            HttpResponseMessage response = httpClient.GetAsync(ServerUrl).Result;
            if (response.IsSuccessStatusCode) return ResponseCodes.ResponseOK;
            else return ResponseCodes.UnknownException;
        }

        // Check what happened wrong here (if HttpRequestException)
        catch (Exception ex)
        {
            return checkException(ex);
        }
    }

    // To login
    public bool login()
    {
        string? port = configuration["PORT"];
        return false;
    }

    // To register, setting the visible name aswell
    public bool register(string visibleNm)
    {
        string? port = configuration["PORT"];
        visibleName = visibleNm;
        return false;
    }

    // To update the different fields of the cookie
    public bool updateInfo(string usernm, string pass)
    {
        Username = usernm;

        return false;
    }

    // To get a Cookie from a cookie string
    public static LoginCookie? fromStringToCookieTransform()
    {
        return null;
    }

    // To get the cookie string from the current cookie
    public string fromCookieToStringTransform()
    {
        return string.Empty;
    }
    
}

    