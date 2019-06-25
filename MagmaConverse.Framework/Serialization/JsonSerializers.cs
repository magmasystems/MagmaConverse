using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// ReSharper disable ConvertPropertyToExpressionBody

namespace MagmaConverse.Framework.Serialization
{
    [Flags]
    public enum JsonSerializationSettings
    {
        Default = 0x00,
        NoEmbeddedTypes = 0x01,
    }

    /// <summary>
    /// Serialize or Deserialize Json
    /// </summary>
    public static class Json
    {
        internal static ILog Logger = LogManager.GetLogger(typeof(Json));

        static Json()
        {
            const string key = "Json.Trace";

            if (ConfigurationManager.AppSettings[key] == null)
                return;

            string trace = ConfigurationManager.AppSettings[key];
            if (SerializerSettings != null)
            {
                SerializerSettings.TraceWriter = (trace.Equals("true", StringComparison.InvariantCultureIgnoreCase)) ? new LogTraceWriter() : null;
            }
        }

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TraceWriter = null,
            //	TraceWriter = new LogTraceWriter(),

            // Inclue null values in the Json
            NullValueHandling = NullValueHandling.Include,

            Converters = new List<JsonConverter> { new IsoDateTimeConverter(), new StringEnumConverter(), },

            // If we use the Auto type handling, then Json.Net will put a $type string in the Json, and we will be able to deserialize Json objects into their .Net types.
            // If we use TypeNameHandling.None, then the $type string will not be put in the Json, but non .Net clients will have to ignore the $type value.
            // http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_TypeNameHandling.htm
            TypeNameHandling = (ConfigurationManager.AppSettings["Json.TypeNameHandling"] == null)
                                  ? TypeNameHandling.Auto
                                  : (TypeNameHandling)Enum.Parse(typeof(TypeNameHandling), ConfigurationManager.AppSettings["Json.TypeNameHandling"])
        };

        public static readonly JsonSerializerSettings SerializerSettingsWithNoTypeHandling = new JsonSerializerSettings
        {
            TraceWriter = null,
            //	TraceWriter = new LogTraceWriter(),

            // Inclue null values in the Json
            NullValueHandling = NullValueHandling.Include,

            Converters = new List<JsonConverter> { new IsoDateTimeConverter(), new StringEnumConverter(), },

            TypeNameHandling = TypeNameHandling.None
        };

        private static JsonSerializerSettings BuildSettings(JsonSerializationSettings settings = JsonSerializationSettings.Default)
        {
            if (settings == JsonSerializationSettings.Default)
                return SerializerSettings;

            if ((settings & JsonSerializationSettings.NoEmbeddedTypes) != 0)
            {
                return SerializerSettingsWithNoTypeHandling;
            }

            return null;
        }

        public static object Deserialize(string json, Type type, JsonSerializationSettings settings = JsonSerializationSettings.Default)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, BuildSettings(settings));
            }
            catch (Exception exc)
            {
                Logger.Error("Cannot deserialize the Json", exc);
                throw;
            }
        }

        public static T Deserialize<T>(string json, JsonSerializationSettings settings = JsonSerializationSettings.Default)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, BuildSettings(settings));
            }
            catch (Exception exc)
            {
                Logger.Error("Cannot deserialize the Json", exc);
                throw;
            }
        }

        public static string Serialize<T>(T obj, JsonSerializationSettings settings = JsonSerializationSettings.Default)
        {
            return Serialize(obj, obj.GetType(), settings);
        }

        public static string Serialize(object obj, Type type, JsonSerializationSettings settings = JsonSerializationSettings.Default)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented, BuildSettings(settings));
            }
            catch (Exception exc)
            {
                Logger.Error("Cannot serialize the Json", exc);
                throw;
            }
        }
    }


    /*
      This solves the problem of reading single-valued arrays in JSON
        http://michaelcummings.net/mathoms/using-a-custom-jsonconverter-to-fix-bad-json-results#.U1-2R_ldXnI

      public class foo
      {
        [JsonConverter(typeof(SingleValueArrayConverter))]
        public List<OrderItem> items;	 
    */
    public class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new Object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T> { instance };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }

    public class LogTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter
        {
            // trace all messages. nlog can handle filtering
            get { return TraceLevel.Verbose; }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            Json.Logger.Info(message);
        }
    }
}



