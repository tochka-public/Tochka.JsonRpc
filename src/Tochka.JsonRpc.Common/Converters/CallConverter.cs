//TODO: server
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using Tochka.JsonRpc.Common.Models.Request;
// using Tochka.JsonRpc.Common.Models.Request.Untyped;
//
// namespace Tochka.JsonRpc.Common.Converters;
//
// /// <summary>
// ///     Handle dumb rule of Id present for requests and not present for notifications
// /// </summary>
// public class CallConverter : JsonConverter<ICall>
// {
//     public override void Write(Utf8JsonWriter writer, ICall value, JsonSerializerOptions options) => throw
//         // NOTE: used in server to parse requests, no need for serialization
//         new InvalidOperationException();
//
//     public override ICall? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         var readerClone = reader;
//         var hasId = false;
//         while (reader.Read())
//         {
//             if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == JsonRpcConstants.IdProperty)
//             {
//                 hasId = true;
//                 reader.Read();
//                 switch (reader.TokenType)
//                 {
//                     case JsonTokenType.String:
//                     case JsonTokenType.Number:
//                     case JsonTokenType.Null:
//                         var result = JsonSerializer.Deserialize<UntypedRequest>(ref readerClone, options);
//                 }
//             }
//         }
//
//
//     }
//
//     // public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//     // {
//     //     var jObject = JObject.Load(reader);
//     //     var idProperty = jObject[JsonRpcConstants.IdProperty];
//     //     var idType = idProperty?.Type;
//     //     switch (idType)
//     //     {
//     //         case JTokenType.String:
//     //         case JTokenType.Integer:
//     //         case JTokenType.Null:
//     //             var idValue = idProperty as JValue;
//     //             var result1 = jObject.ToObject<UntypedRequest>(serializer);
//     //             result1.RawJson = jObject.ToString();
//     //             result1.RawId = idValue;
//     //             return result1;
//     //
//     //         case null:
//     //             var result2 = jObject.ToObject<UntypedNotification>(serializer);
//     //             result2.RawJson = jObject.ToString();
//     //             return result2;
//     //
//     //         default:
//     //             throw new ArgumentOutOfRangeException(nameof(idType), idType, "Expected string, number, null or nothing as Id");
//     //     }
//     // }
//
//     public override bool CanConvert(Type typeToConvert) =>
//         typeToConvert == typeof(ICall<>)
//         || typeToConvert == typeof(IUntypedCall);
// }


