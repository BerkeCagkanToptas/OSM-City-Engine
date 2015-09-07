using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OpenStreetMap
{
    public class ItemEnumerator
    {
        public enum wayType
        {
            building,
            highway,
            area,
            barrier,
            river,
            none
        }

        public enum nodeType
        {
            PhoneBox,
            Tree,
            TrafficSign,
            DrinkingFountain,
            StreetLamp,
            PostBox,
            None
        }

        //Seperate WayType of OSM into our City Engine object types
        public static wayType getWayTpe(Way way)
        {
            if (way.tags == null)
                return wayType.none;

            foreach (Tag t in way.tags)
            {
                if (t.k == "building")
                    return wayType.building;
                else if (t.k == "barrier")
                    return wayType.barrier;
                else if (t.k == "highway" || t.k == "railway")
                {
                    if (t.v == "footway")
                        return wayType.none;

                    return wayType.highway;
                }
                else if (t.k == "landuse" || t.k == "leisure")
                    return wayType.area;
                else if (t.k == "amenity")
                {
                    for (int k = 0; k < way.tags.Count; k++)
                    {
                        if (way.tags[k].k == "building")
                            return wayType.building;
                    }
                    return wayType.area;
                }
                else if (t.k == "waterway" && t.v == "river")
                    return wayType.highway; //return wayType.river;

                else if (t.k == "historic" && t.v == "monument")
                    return wayType.area;
                else if (t.k == "historic" && (t.v == "citywalls" || t.v == "city_gate"))
                    return wayType.barrier;

            }


            return wayType.none;
        }

        public static nodeType getNodeType(Node node)
        {
            if (node.tags == null)
                return nodeType.None;

            for(int i = 0 ; i < node.tags.Count ; i++)
            {
                if (node.tags[i].k == "natural" && node.tags[i].v == "tree")
                    return nodeType.Tree;
                if (node.tags[i].k == "amenity")
                {
                    if(node.tags[i].v == "drinking_water")
                        return nodeType.DrinkingFountain;
                    if(node.tags[i].v == "post_box")
                        return nodeType.PostBox;
                    if(node.tags[i].v == "telephone")
                        return nodeType.PhoneBox;
                }
                if(node.tags[i].k == "highway")
                {
                    if (node.tags[i].v == "traffic_signals")
                        return nodeType.TrafficSign;
                    if (node.tags[i].v == "street_lamp")
                        return nodeType.StreetLamp;
                }

            }

            return nodeType.None;

        }

    }
}
