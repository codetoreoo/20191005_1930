using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WEBAPITEST.Controllers
{
    public class LineChatController : ApiController
    {
        [HttpPost]
        public IHttpActionResult POST()
        {
            string ChannelAccessToken = "YbgUy6ucRMSNGbpZ5Tb3o+AnK2dOaKCLsAoqU1mYL+w7wYaKVfwVj9oYBh3JJ0UsAF+wfQW6j+GqfYUSqQI7omQH5DBT8mrP1wLfdwoRb6E/baWJ8Dtt/8IQICuRn4E6tOKhXxrys5K9EOqghHhh2AdB04t89/1O/w1cDnyilFU=";
            try
            {
                //取得 http Post RawData(should be JSON)
                string postData = Request.Content.ReadAsStringAsync().Result;
                //剖析JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(postData);
                //回覆訊息
                string Message;
                Message = "你說了:" + ReceivedMessage.events[0].message.text;
                //回覆用戶
                isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, Message, ChannelAccessToken);
                //回覆API OK
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok();
            }
        }
    }
}