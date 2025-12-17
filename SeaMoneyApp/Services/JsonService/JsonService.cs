using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using SeaMoneyApp.Models;

namespace SeaMoneyApp.Services.JsonService;

public static class JsonService
{
    public static string Save<T>(T appSession, string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true,
            };
            var json = JsonSerializer.Serialize(appSession, options);
            File.WriteAllText(path, json);
            return json;
        }
        catch (Exception e)
        {
            throw new SaveToJsonException(path, e);
        }
    }
    public static async Task<string> SaveAsync<T>(T appSession, string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true,
                
                
            };
            var json = JsonSerializer.Serialize(appSession, options);
            await File.WriteAllTextAsync(path, json);
            return json;
        }
        catch (Exception e)
        {
            throw new SaveToJsonException(path, e);
        }
    }
    public static T Load<T>(string path)
    {
        try
        {
            
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IncludeFields = true,
            };
            
            var json = File.ReadAllText(path);
            var result =  JsonSerializer.Deserialize<T>(json, options);

            if (result is null) throw new LoadFromJsonException(path);

            return result;
        }
        catch (Exception e)
        {
            throw new LoadFromJsonException(path, e);
        }
    }
}