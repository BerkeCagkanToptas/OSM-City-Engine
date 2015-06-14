using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.ConfigHandler
{
    public class HighwayConfigurations
    {
       [XmlAttribute("type")]
       public string type;
       [XmlAttribute("size")]
       public float size;
       [XmlElement("materialPath")]
       public string materialPath;
       [XmlElement("hasLeftSideWalk")]
       public bool leftSidewalk;
       [XmlElement("hasRightSideWalk")]
       public bool rightSidewalk;
       [XmlElement("LeftSideWalkSize")]
       public float leftSidewalkSize;
       [XmlElement("RightSideWalkSize")]
       public float rightSidewalkSize;
    }

    public class RiverConfigurations
    {
        [XmlAttribute("size")]
        public float size;
        [XmlAttribute("materialPath")]
        public string materialPath;
    }

    public class BuildingMaterial
    {
        [XmlAttribute("isActive")]
        public bool isActive;
        [XmlAttribute("name")]
        public string name;

        [XmlElement("textureWidth")]
        public float width;
        [XmlElement("colorTextPath")]
        public string colorTexturePath;
        [XmlElement("normalTextPath")]
        public string normalTexturePath;
        [XmlElement("specularTextPath")]
        public string specularTexturePath;
    }

    public class BuildingConfigurations
    {
        public float minheight, maxheight;
        public Vector3 defaultColor;

        [XmlArray("defaultSkins")]
        [XmlArrayItem("Skin")]
        public List<BuildingMaterial> defaultSkins;
       

    }

    public class AreaConfigurations
    {
        public Vector3 defaultColor;
    }

    public class BarrierConfigurations
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("ResourcePath")]
        public string Path;

        public float width;
        public float height;
    }

    [XmlRoot("InitialConfigurations")]
    public class InitialConfigurations
    {
        [XmlArray("HighwayConfigurations")]
        [XmlArrayItem("Highway")]
        public List<HighwayConfigurations> highwayConfig;
        [XmlElement("RiverConfigurations")]
        public RiverConfigurations riverConfig;
        [XmlElement("BuildingConfigurations")]
        public BuildingConfigurations buildingConfig;
        [XmlElement("AreaConfigurations")]
        public AreaConfigurations areaConfig;
        [XmlArray("BarrierConfigurations")]
        [XmlArrayItem("Barrier")]
        public List<BarrierConfigurations> barrierConfig;
    }


    class InitialConfigLoader
    {

        public InitialConfigLoader()
        {
            if(!Directory.Exists(Path.Combine(Application.persistentDataPath, "ConfigFiles/" )))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath,"ConfigFiles/"));
        }

        public void saveInitialConfig(string path, InitialConfigurations config)
        {
            var serializer = new XmlSerializer(typeof(InitialConfigurations));
            var encoding = Encoding.GetEncoding("UTF-8");

            if (File.Exists(path))
                File.Delete(path);

            using (var stream = new StreamWriter(path, false, encoding))//new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, config);
            }
        }

        public InitialConfigurations loadInitialConfig()
        {
            var serializer = new XmlSerializer(typeof(InitialConfigurations));
         
            string path = Path.Combine(Application.persistentDataPath, "ConfigFiles/initialConfig.xml");
            if(File.Exists(path))
            {
                var encoding = Encoding.GetEncoding("UTF-8");
                using (var stream = new StreamReader(path,encoding)) //new FileStream(path, FileMode.Open))
                {             
                        return serializer.Deserialize(stream) as InitialConfigurations;
                }
            }
            else
            {
                    InitialConfigurations config = fillConfig();                
                    saveInitialConfig(Path.Combine(Application.persistentDataPath, "ConfigFiles/initialConfig.xml"), config);
                    return config;
            }

        }

        
        public InitialConfigurations fillConfig()
        {
            InitialConfigurations config = new InitialConfigurations();

            //HIGHWAY CONFIGURATIONS
            config.highwayConfig = new List<HighwayConfigurations>();
            string[] highwayTypes = new string[] {"HighwayResidential", "HighwayPrimary", "HighwaySecondary", "HighwayTertiary","HighwayTertiaryLink", 
                                                  "HighwayUnclassified", "HighwayService", "HighwayPath", "HighwayFootway",
                                                  "HighwayPavement", "Railway", "River"};
            float[] highwaySizes = new float[] {6.0f, 10.0f, 10.0f, 8.0f, 8.0f, 7.0f, 4.0f, 6.0f, 4.0f, 2.0f, 1.5f, 8.0f };

            string[] highwayMaterials = new string[] {"Materials/Highway/Mat_Residential", "Materials/Highway/Mat_Primary",
            "Materials/Highway/Mat_Secondary","Materials/Highway/Mat_Tertiary", "Materials/Highway/Mat_TertiaryLink", "Materials/Highway/Mat_Unclassified",
            "Materials/Highway/Mat_Service", "Materials/Highway/Mat_Path", "Materials/Highway/Mat_Footway",
            "Materials/Highway/Mat_Pavement", "Materials/Highway/Mat_Railway","Materials/Highway/Mat_River" };

            for(int k=0 ; k < highwayTypes.Length ; k++)
            {
                HighwayConfigurations hc = new HighwayConfigurations();
                hc.size = highwaySizes[k];
                hc.type = highwayTypes[k];
                hc.materialPath = highwayMaterials[k];
                config.highwayConfig.Add(hc);
            }

            //RIVER CONFIGURATIONS [NOT USED]
            config.riverConfig = new RiverConfigurations();
            config.riverConfig.materialPath = "Materials/River/??";
            config.riverConfig.size = 6.0f;

            //AREA CONFIGURATIONS [NOT USED]
            config.areaConfig = new AreaConfigurations();
            config.areaConfig.defaultColor = new Vector3(0.3f, 0.3f, 0.3f);

            //BARRIER CONFIGURATIONS
            config.barrierConfig = new List<BarrierConfigurations>();

            float[] barrierHeight = new float[] {2.0f,1.5f,15.0f,3.0f, 2.0f };
            float[] barrierWidth = new float[] { 0.1f,0.3f,3.0f,0.3f, 0.5f };
            string[] barrierTypes = new string[]{"Fence", "Wall", "City Wall", "Retaining Wall", "Default"};
            string[] barrierPrefabs = new string[]{"Materials/Barrier/chainlink", "Materials/Barrier/BrickWall",
                                                   "Materials/Barrier/CityWall", "Materials/Barrier/RetainingWall", 
                                                   "Materials/Barrier/Default"};
            for (int k = 0; k < barrierTypes.Length; k++)
            {
                BarrierConfigurations bar = new BarrierConfigurations();
                bar.width = barrierWidth[k];
                bar.height = barrierHeight[k];
                bar.Path = barrierPrefabs[k];
                bar.name = barrierTypes[k];
                config.barrierConfig.Add(bar);
            }

            //BUILDING CONFIGURATIONS
            config.buildingConfig = new BuildingConfigurations();
            config.buildingConfig.defaultColor = new Vector3(0.5f, 0.5f, 0.5f);
            config.buildingConfig.minheight = 15.0f;
            config.buildingConfig.maxheight = 15.0f;
            config.buildingConfig.defaultSkins = new List<BuildingMaterial>();

            string[] skinNames = new string[] {"White Concrete","Dark Brick","Light Brick","White Concrete 2","Dark Brick 2", "Stone Tower", "Kiosk"};
            float[] skinSizes = new float[] {16.0f,16.0f,12.0f,8.0f,8.0f,30.0f,2.0f};
            List<string> colorTexturePath = new List<string>();
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/buildingTexture1.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/buildingTexture2.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/buildingTexture3.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/buildingTexture4.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/buildingTexture5.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/towerTexture.jpg");
            colorTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/KioskTexture.jpg");

            List<string> normalTexturePath = new List<string>();
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building1Normal.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building2Normal.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building3Normal.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building4Normal.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building5Normal.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/NormalMapTower.png");
            normalTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/NormalMapKiosk.png");

            List<string> specularTexturePath = new List<string>();
            specularTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building1Specular.png");
            specularTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building2Specular.png");
            specularTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building3Specular.png");
            specularTexturePath.Add(Application.dataPath + "/Resources/Textures/Building/Building4Specular.png");
            specularTexturePath.Add("");
            specularTexturePath.Add("");
            specularTexturePath.Add("");

            for (int k = 0; k < skinNames.Length; k++)
            {
                BuildingMaterial mat = new BuildingMaterial();
                mat.name = skinNames[k];
                mat.width = skinSizes[k];
                mat.colorTexturePath = colorTexturePath[k];
                mat.normalTexturePath = normalTexturePath[k];
                mat.specularTexturePath = specularTexturePath[k];
                mat.isActive = true;

                config.buildingConfig.defaultSkins.Add(mat);
            }

            config.buildingConfig.defaultSkins[5].isActive = false;
            config.buildingConfig.defaultSkins[6].isActive = false;

            return config;

        }

    }
}
