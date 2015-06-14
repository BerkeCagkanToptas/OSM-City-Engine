using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.OpenStreetMap;
using System.IO;

namespace Assets.Scripts.SceneObjects
{
    class BuildingListModeller
    {
        private List<Material> materialList;

        public List<Building> buildingList;

        List<Way> buildingWayList;
        List<BuildingRelation> buildingRelationList;


        public BuildingListModeller(List<Way> buildingWay, List<BuildingRelation> buildingRelation, BuildingConfigurations config)
        {
            buildingWayList = buildingWay;
            buildingRelationList = buildingRelation;
            setMaterialList(config);


            buildingList = new List<Building>();

            for (int i = 0; i < buildingRelation.Count; i++)
            {
                float materialtexWidth = 10;
                Material mat = getMaterial(buildingRelation[i].tags, config, ref materialtexWidth);
                buildingList.Add(new Building(buildingRelation[i], config,mat,materialtexWidth));
              
            }
            for (int i = 0; i < buildingWay.Count; i++)
            {
                if(!buildingList.Exists(item => item.id == buildingWay[i].id))
                {
                    float materialtexWidth = 10;
                    Material mat = getMaterial(buildingWay[i].tags, config, ref materialtexWidth);
                    buildingList.Add(new Building(buildingWay[i], config, mat, materialtexWidth));
                }
                    
            }

        }

        public void renderBuildingList()
        {
            for(int i = 0; i < buildingList.Count ; i++)
            {
                buildingList[i].RenderBuilding();
            }         
        }

        private Material getMaterial(List<Tag> tagList, BuildingConfigurations config, ref float matWidth)
        {

            int skinindex;

            for (int i = 0; i < tagList.Count; i++)
            {
                if (tagList[i].k == "man_made" && tagList[i].v == "tower")
                {
                    matWidth = config.defaultSkins[5].width;
                    return materialList[5];              
                }
                if (tagList[i].k == "shop" && tagList[i].v == "kiosk")
                {
                    matWidth = config.defaultSkins[6].width;
                    return materialList[6];
                }
            }

            do
            {
                skinindex = UnityEngine.Random.Range(0, config.defaultSkins.Count);
            }
            while (!config.defaultSkins[skinindex].isActive);

            matWidth = config.defaultSkins[skinindex].width;
            return materialList[skinindex];

        }


        private  void setMaterialList(BuildingConfigurations buildingConfig)
        {
            materialList = new List<Material>();

            for (int k = 0; k < buildingConfig.defaultSkins.Count; k++)
            {

                Material matBuilding = (Material)Resources.Load("Materials/Building/Mat_DefaultPrefab", typeof(Material));
                Material mat = new Material(matBuilding);
                Texture2D colortex;
                Texture2D normaltex;
                Texture2D speculartex;

                byte[] fileData;

                if (buildingConfig.defaultSkins[k].colorTexturePath != "")
                {
                    fileData = File.ReadAllBytes(buildingConfig.defaultSkins[k].colorTexturePath);
                    colortex = new Texture2D(2, 2);
                    colortex.LoadImage(fileData);
                    mat.SetTexture("_MainTex", colortex);
                }

                if (buildingConfig.defaultSkins[k].normalTexturePath != "")
                {
                    fileData = File.ReadAllBytes(buildingConfig.defaultSkins[k].normalTexturePath);
                    normaltex = new Texture2D(2, 2);
                    normaltex.LoadImage(fileData);
                    mat.SetTexture("_BumpMap", normaltex);
                }

                if (buildingConfig.defaultSkins[k].specularTexturePath != "")
                {
                    fileData = File.ReadAllBytes(buildingConfig.defaultSkins[k].specularTexturePath);
                    speculartex = new Texture2D(2, 2);
                    speculartex.LoadImage(fileData);
                    mat.SetTexture("_SpecGlossMap", speculartex);
                }

                materialList.Add(mat);

            }

        }


    }
}
