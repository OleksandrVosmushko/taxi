using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Services
{
    public interface IUploadService
    {
        Task PutObjectToStorage(string key, object data);
    }
}
