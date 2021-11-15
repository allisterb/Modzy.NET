using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


///using Modzy.Models;

namespace Modzy;

public abstract class BaseClient : Runtime, IApiClient
{
    #region Constructors
    public BaseClient(string token, Uri restServerUrl, Uri gsqlServerUrl, string user, string pass) : base()
    {
        Token = token ?? throw new ArgumentException("Could not get the TigerGraph access token.");
        RestServerUrl = restServerUrl ?? throw new ArgumentException("Could not get the TigerGraph REST++ server URL.");
        GsqlServerUrl = gsqlServerUrl ?? throw new ArgumentException("Could not get the TigerGraph GSQL server URL.");
        User = user ?? throw new ArgumentException("Could not get the TigerGraph user name.");
        Pass = pass ?? throw new ArgumentException("Could not get the TigerGraph user password.");
        Info("Initialized REST++ client for {0} and GSQL client for {1}.", RestServerUrl, GsqlServerUrl);
    }
    #endregion

    #region Abstract members
    public abstract Task<T> RestHttpGetAsync<T>(string query);

    public abstract Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data);
    #endregion

    #region Properties
    public string Token { get; }

    public Uri RestServerUrl { get; set; }

    public Uri GsqlServerUrl { get; set; }

    public string User { get; set; }

    public string Pass { get; set; }

    #endregion

    #region Methods
    #endregion
}
