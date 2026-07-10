using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CROSS.APIREST
{
    public interface IApiRest
    {
        public Task<T> PostAsync<T>(string idOperacion, string url, object request, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default);

        public Task<T> GetAsync<T>(string idOperacion, string url, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default);
        
        public Task<T> PostMultipartAsync<T>(string idOperacion, string url, object request, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default);
        

    }
}
