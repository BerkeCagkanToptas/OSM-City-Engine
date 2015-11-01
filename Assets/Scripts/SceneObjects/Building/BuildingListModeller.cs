using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.OpenStreetMap;
using System.IO;
using Assets.Scripts.Utils;

namespace Assets.Scripts.SceneObjects
{
    class BuildingListModeller
    {
        private List<Material> materialList;
        public List<Building> buildingList;

        public BuildingListModeller(List<Way> buildingWay, List<BuildingRelation> buildingRelation, BuildingConfigurations config)
        {
         
            setMaterialList(config);

            buildingList = new List<Building>();

            for (int i = 0; i < buildingRelation.Count; i++)
            {
                float materialtexWidth = 10;
                int materialID = -1;
                Material mat = getMaterial(buildingRelation[i].tags, config, ref materialtexWidth, ref materialID);
                try
                {
                    buildingList.Add(new Building(buildingRelation[i], config, mat, materialID, materialtexWidth));
                }
                catch (Exception ex)
                {
                    Debug.Log("<color=red>Building ERROR:</color>" + ex.Message);
                    continue;
                }
            }

            for (int i = 0; i < buildingWay.Count; i++)
            {
                if(!buildingList.Exists(item => item.id == buildingWay[i].id))
                {
                    float materialtexWidth = 10;
                    int materialID = -1;
                    Material mat = getMaterial(buildingWay[i].tags, config, ref materialtexWidth, ref materialID);
                    try
                    {
                        buildingList.Add(new Building(buildingWay[i], config, mat, materialID, materialtexWidth));
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("<color=red>Building ERROR:</color>" + ex.Message);
                        continue;
                    }
                }
                    
            }

        }

        public BuildingListModeller(List<Way> buildingWay, List<BuildingRelation> buildingRelation, BuildingConfigurations config, List<BuildingSave> buildingSave)
        {
            setMaterialList(config);

            buildingList = new List<Building>();

            for (int i = 0; i < buildingRelation.Count; i++)
            {                
                int saveIndex = buildingSave.FindIndex(item=> item.buildingID == buildingRelation[i].id);
                if (saveIndex == -1)
                    continue;
                float materialtexWidth = 10;
                int materialID = buildingSave[saveIndex].materialID;
                Material mat = getMaterial(materialID,config,ref materialtexWidth);
                try
                {

                    buildingList.Add(new Building(buildingRelation[i], config, mat, materialID, materialtexWidth));
                    buildingList[buildingList.Count - 1].facadeSkins = new List<FacadeSkin>(buildingSave[saveIndex].skins);
                }
                catch (Exception ex)
                {
                    Debug.Log("<color=red>Building ERROR:</color>" + ex.Message);
                    continue;
                }
            }

            for (int i = 0; i < buildingWay.Count; i++)
            {
                if (!buildingList.Exists(item => item.id == buildingWay[i].id))
                {
                    int saveIndex = buildingSave.FindIndex(item => item.buildingID == buildingWay[i].id);
                    if (saveIndex == -1)
                        continue;
                    float materialtexWidth = 10;
                    int materialID = buildingSave[saveIndex].materialID;
                    Material mat = getMaterial(materialID, config, ref materialtexWidth);
                    try
                    {
                        buildingList.Add(new Building(buildingWay[i], config, mat, materialID, materialtexWidth));
                        buildingList[buildingList.Count - 1].facadeSkins = new List<FacadeSkin>(buildingSave[saveIndex].skins);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("<color=red>Building ERROR:</color>" + ex.Message);
                        continue;
                    }
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

        private Material getMaterial(List<Tag> tagList, BuildingConfigurations config, ref float matWidth, ref int matID)
        {

            int skinindex;

            for (int i = 0; i < tagList.Count; i++)
            {
                if (tagList[i].k == "man_made" && tagList[i].v == "tower")
                {                    
                    skinindex = config.defaultSkins.FindIndex(item=> item.name == "Antic Stones");
                    matWidth = config.defaultSkins[skinindex].width;
                    matID = skinindex;
                    return materialList[skinindex];              
                }
                if (tagList[i].k == "shop" && tagList[i].v == "kiosk")
                {
                    skinindex = config.defaultSkins.FindIndex(item => item.name == "Kiosk");
                    matWidth = config.defaultSkins[skinindex].width;
                    matID = skinindex;
                    return materialList[skinindex];
                }
            }


            do
            {
                skinindex = UnityEngine.Random.Range(0, config.defaultSkins.Count);
            }
            while (!config.defaultSkins[skinindex].isActive);

            matWidth = config.defaultSkins[skinindex].width;
            matID = skinindex;
            return materialList[skinindex];
        }

        private Material getMaterial(int materialID, BuildingConfigurations config, ref float matWidth)
        {
            matWidth = config.defaultSkins[materialID].width;
            return materialList[materialID];
        }

        private  void setMaterialList(BuildingConfigurations buildingConfig)
        {
            materialList = new List<Material>();
          
            for (int k = 0; k < buildingConfig.defaultSkins.Count; k++)
            {
                BuildingMaterial bmat = buildingConfig.defaultSkins[k];
                Material mat = InGameTextureHandler.createMaterial2(bmat.colorTexturePath, bmat.normalTexturePath, bmat.specularTexturePath);
                materialList.Add(mat);
            }

        }


    }
}
