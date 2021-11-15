using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modzy;
    interface IApiClient
    {
        Task<T> RestHttpGetAsync<T>(string query);
    }

