using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NLogCustom
{
    public partial class CustomLogger : System.Web.UI.Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            BaseNLog blog = new BaseNLog();
            CustomizedNLog clog = new CustomizedNLog("549714084");
        }
    }

    public class BaseNLog {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public BaseNLog() {
            //寫法一
            logger.Info("---------------------------------------");
            logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            logger.Info("__開始:");
            logger.Trace("_Trace_not_show");
            logger.Debug("_Debug");
            logger.Info("__Info");
            logger.Warn("__Warn");
            logger.Error("_Error");
            logger.Fatal("_Fatal");
            //寫法二 :  呼叫 Log() method            
            logger.Log(LogLevel.Info, "__Info_ohter way to write code");
            logger.Info("---------------------------------------");
        }
    }

    public class SetLogger_MappedDiagnosticsContext : Logger
    {
        public void SetCustomerId(string CustomerId)
        {
            //https://github.com/nlog/NLog/wiki/Mdc-Layout-Renderer
            MappedDiagnosticsContext.Set("CustomerId", CustomerId);
        }
    }

    public class CustomizedNLog
    {
        private readonly SetLogger_MappedDiagnosticsContext logger = (SetLogger_MappedDiagnosticsContext)LogManager.GetCurrentClassLogger(typeof(SetLogger_MappedDiagnosticsContext));

        public CustomizedNLog(string account)
        {
            //set 
            logger.SetCustomerId(account);

            //record log
            logger.Info("---------------------------------------");
            logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            logger.Info("會員: {0} 已成功登入", account);
            logger.Info("---------------------------------------");
        }        
    }
}