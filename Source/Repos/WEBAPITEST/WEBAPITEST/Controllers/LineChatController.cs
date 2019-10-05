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
            string ChannelAccessToken = "mG2oxKhG0aBbsw+uuyMXj5N0lEPS6WFjuVyZl8BJrXRJBdOgdRnBXirPpR93wfLjAF+wfQW6j+GqfYUSqQI7omQH5DBT8mrP1wLfdwoRb6E8vW0/jRrn9czwjuA6jMIf5hCA8zRv6APq5AWdhI2bWgdB04t89/1O/w1cDnyilFU=";
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