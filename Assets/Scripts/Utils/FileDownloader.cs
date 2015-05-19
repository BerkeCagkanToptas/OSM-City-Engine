using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Net;
using System.Threading;

namespace Assets.Scripts.Utils
{
    class FileDownloader
    {

        struct parameterObject
        {
            public string url;
            public string savePath;
        }

        static void downloadfunction(object data)
        {
            parameterObject obj = (parameterObject)data;

            if (File.Exists(obj.savePath))
                return;

            using (WebDownload Client = new WebDownload(10000))
            {
                Client.DownloadFile(obj.url, obj.savePath);
            }
        }

        public static void downloadfromURL(string url, string savePath)
        {

            parameterObject obj = new parameterObject();
            obj.url = url;
            obj.savePath = savePath;

            downloadfunction(obj);

            //  Thread thread = new Thread(FileDownloader.downloadfunction);
            //  thread.Start(obj);
        }

    }


}
