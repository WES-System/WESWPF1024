using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace WES.Helpers
{
    public class HttpHelper
    {
        /// <summary>
        /// 通过web api获取数据的方法
        /// </summary>
        /// <param name="url">api的url</param>
        /// <param name="method">请求类型,默认是get</param>
        /// <param name="postData">post请求所携带的数据</param>
        /// <returns></returns>
        public static string RequestData(string url, string method = "Get", string postData = null)
        {
            try
            {
                method = method.ToUpper();
                //设置安全通信协议   服务器有些强制使用tls1.2的安全通信协议,所以至少包含SecurityProtocolType.Tls12   如果沒有SecurityProtocolType.Tls12设置会报错:HttpWebRequest底层连接已关闭:传送时发生意外错误  
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                //创建请求实例
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 5000;

                //设置请求类型
                request.Method = method;
                //设置请求消息主体的编码方法
                request.ContentType = "application/json";

                //POST方式處理
                if (method == "POST")
                {
                    //用UTF8字符集对post请求携带的数据进行编码,可防止中文乱码
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    //指定客户端post请求携带的数据的长度
                    request.ContentLength = byteArray.Length;

                    //创建一个tream,用于写入post请求所携带的数据(该数据写入了请求体)
                    Stream stream = request.GetRequestStream();
                    stream.Write(byteArray, 0, byteArray.Length);
                    stream.Close();
                }

                //获取请求的响应实例
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //获取读取流实体,用来以UTF8字符集读取响应流中的数据
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                //进行数据读取
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                return retString;

            }
            catch (Exception ex)
            {
                return "{\"code\":\"1\",\"message\":\"" + ex.Message + "\"}";
            }
        }

        public static async Task<string> APIPost(string url, string data)
        {
            string result = string.Empty;
            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //使用FormUrlEncodedContent做HttpContent
                var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    //传递单个值
                    {"", data}//键名必须为空
                    //传递对象
                    //{"name","hello"},
                    //{"age","16"}
                 });

                //await异步等待回应
                var response = await http.PostAsync(url, content);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();
                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        static async void APIGet(string url)
        {
            //创建HttpClient（注意传入HttpClientHandler）
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                //await异步等待回应
                var response = await http.GetAsync(url);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        public static async Task<string> RCS2000Request(string URL, object Obj)
        {
            var json = JsonSerializer.Serialize(Obj);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(URL, data);
                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    //_logger.Error($"HTTP访问错误，url（{URL}）或许错误！", e);
                }
                return $"{{\"code\":\"1\",\"message\":\"HTTP访问错误，url（{URL}）或许错误！\"}}";
            }
        }

        public static void HttpPostTest()
        {
            //var client = new RestClient("http://localhost/home/getjson");
            //// client.Authenticator = new HttpBasicAuthenticator(username, password);

            //var request = new RestRequest("resource/{id}", Method.Post);
            //request.AddParameter("name", "value"); // 添加请求参数
            //request.AddUrlSegment("id", "123"); // 添加 token 

            ////添加HTTP头
            //request.AddHeader("header", "value");

            //// 添加文件
            ////request.AddFile(path);

            //// 执行请求
            //RestResponse response = client.Execute(request);
            //var content = response.Content; // 返回请求对象

            //// or automatically deserialize result
            //// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            ////RestResponse<Person> response2 = client.Execute<Person>(request);
            ////var name = response2.Data.Name;

            //////异步支持
            ////client.ExecuteAsync(request, response => {
            ////    Console.WriteLine(response.Content);
            ////});


            ////var asyncHandle = client.ExecuteAsync<Person>(request, response => {
            ////    Console.WriteLine(response.Data.Name);
            ////});

            ////asyncHandle.Abort();
        }
    }
}
