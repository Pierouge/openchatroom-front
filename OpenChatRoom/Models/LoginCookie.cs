using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class LoginCookie(IConfiguration config, string serverIp)
{
    private readonly IConfiguration configuration = config;
    private readonly string Server = serverIp;
    private string Username = string.Empty;
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
        HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
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
        string ServerUrl = string.Concat(Server, ":", port, "/user/", Username, "/", Password);
        if (!(ServerUrl.Contains("http://")||ServerUrl.Contains("https://"))) ServerUrl = string.Concat("http://", ServerUrl);
        return false;
    }

    // To register, setting the visible name aswell
    public bool register(string visibleName)
    {
        string? port = configuration["PORT"];
        string ServerUrl = string.Concat(Server, ":", port, "/user");
        if (!(ServerUrl.Contains("http://")||ServerUrl.Contains("https://"))) ServerUrl = string.Concat("http://", ServerUrl);

        return false;
    }

    // To update the different fields of the cookie
    public async Task<bool> updateInfo(string usernm, string pass, bool newSalt)
    {
        Username = usernm;

        byte[] saltBytes = new byte[16];
        // Generate the new salt
        if (newSalt)
        {
            saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            Salt = Convert.ToBase64String(saltBytes);
        }
        // Fetch the previous salt
        else
        {
            string? port = configuration["PORT"];
            string ServerUrl = string.Concat(Server, ":", port, "/user/", Username);
            if (!(ServerUrl.Contains("http://") || ServerUrl.Contains("https://"))) ServerUrl = string.Concat("http://", ServerUrl);
            HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            HttpResponseMessage response = await httpClient.GetAsync(ServerUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Salt = await response.Content.ReadAsStringAsync();
                saltBytes = Convert.FromBase64String(Salt);
            }
            else return false;
        }

        // Hash the password 10k times
        using var pbkdf2 = new Rfc2898DeriveBytes(pass, saltBytes, 10_000, HashAlgorithmName.SHA256);
        byte[] hashBytes = pbkdf2.GetBytes(32);

        Password = Convert.ToBase64String(hashBytes);
        return true;
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

    