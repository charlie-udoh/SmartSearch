﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSearch.Core.Interfaces
{
    public interface IDataService
    {
        Task<object> SearchData(string query, List<string> market, int limit, int skip);

        Task<bool> SaveData(string filePath, string documentType);

        string[] GetAllowedDocumentTypes();
    }
}