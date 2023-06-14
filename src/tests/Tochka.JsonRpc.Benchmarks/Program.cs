using System.Reflection;
using System.Text;
using BenchmarkDotNet.Running;
using Tochka.JsonRpc.Benchmarks;

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run();
// const string plainRequest = """
//         {
//             "jsonrpc": "2.0",
//             "method": "process",
//             "id": "123",
//             "params": {
//                 "data": {
//                     "bool_field": true,
//                     "string_field": "123",
//                     "int_field": 123,
//                     "double_field": 1.23,
//                     "enum_field": "two",
//                     "array_field": [
//                         1,
//                         2,
//                         3
//                     ],
//                     "nullable_field": null
//                 }
//             }
//         }
//         """;
// var benchmark = new GetRequestBenchmark();
// benchmark.Setup();
// benchmark.Request = new StringContent(plainRequest, Encoding.UTF8, "application/json");
// var result = await benchmark.EdjCase();
// Console.WriteLine(await result.Content.ReadAsStringAsync());
