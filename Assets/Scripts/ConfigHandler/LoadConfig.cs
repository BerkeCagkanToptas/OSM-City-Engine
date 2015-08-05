using Assets.Scripts.SceneObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.ConfigHandler
{
    class LoadConfig
    {

        public SceneSave sceneSave;

        public LoadConfig(string saveFilePath)
        {
            var serializer = new XmlSerializer(typeof(SceneSave));

            if (File.Exists(saveFilePath))
            {
                var encoding = Encoding.GetEncoding("UTF-8");
                using (var stream = new StreamReader(saveFilePath, encoding))
                {
                   sceneSave = serializer.Deserialize(stream) as SceneSave;
                }
            }
        }








    }
}
