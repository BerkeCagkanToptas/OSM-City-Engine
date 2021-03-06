﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using System.Xml;
using System.IO;

namespace Assets.Scripts.OpenStreetMap
{


public struct Tag
{
	public string k;
	public string v;
}

public struct Node
{
    public ItemEnumerator.nodeType type;
	public string id;
    public float lat, lon;
    public float height;
    public Vector3 meterPosition;
	public List<Tag> tags;

    public Node(Node nd)
    {
        this.type = nd.type;
        this.id = nd.id;
        this.lat = nd.lat;
        this.lon = nd.lon;
        this.height = nd.height;
        this.meterPosition = nd.meterPosition;
        this.tags = new List<Tag>(nd.tags);
    }
}

public struct Member
{
	public string type;
	public string ref_;
	public string role;
}

public struct Relation
{
	public string id;
	public List<Member> members;
	public List<Tag> tags;
}

public struct BuildingRelation
{
    public string id;
    public float buildingHeight;
    public Way outerWall;
    public List<Way> innerHoles;
    public List<Tag> tags;
}

public struct Way
{
    public ItemEnumerator.wayType type;

	public string id;
    //If a way represent an area value is true else false
    public bool isArea;
    //nodes of way
	public List<Node> nodes;
	//tags of way
    public List<Tag> tags;

    public Way(Way w)
    {
        this.type = w.type;
        this.id = w.id;
        this.isArea = w.isArea;
        this.tags = new List<Tag>(w.tags);
        this.nodes = new List<Node>(w.nodes);
    }
};

public struct BBox
{
    public float left, bottom, top, right;
    public float meterLeft, meterBottom, meterTop, meterRight;

};

public struct OSMXml
{
    public BBox bbox;
    public List<Node> nodeList;
    public List<Way> wayList;
    public List<Relation> relationList;

    public List<BuildingRelation> buildingRelations;
    public List<Node> defaultobject3DList;
}

class OSMparser
{
    private List<Node> nodeList;
    private List<Way> wayList;

    public OSMXml parseOSM(string filename)
    {
        OSMXml osmxml = new OSMXml();
        osmxml.nodeList = new List<Node>();
        osmxml.wayList = new List<Way>();
        osmxml.relationList = new List<Relation>();
        osmxml.buildingRelations = new List<BuildingRelation>();
        osmxml.defaultobject3DList = new List<Node>();

        nodeList = new List<Node>();
        wayList = new List<Way>();

        StreamReader osmfile = new StreamReader(filename);

        if (osmfile == null)
        {
            Debug.Log("<color=red>Fatal error:</color>" + filename + " not set");
            return new OSMXml();
        }
     
        XmlDocument xmlfile = new XmlDocument();
        xmlfile.Load(osmfile);

        foreach (XmlNode node in xmlfile.DocumentElement.ChildNodes)
        {

            if (node.Name == "node")
            {
                Node nd = readNode(node);
                osmxml.nodeList.Add(nd);
                nodeList.Add(nd);
                if (nd.type != ItemEnumerator.nodeType.None)
                    osmxml.defaultobject3DList.Add(nd);
            }
            else if (node.Name == "way")
            {
                Way nd = readWay(node);
                osmxml.wayList.Add(nd);
                wayList.Add(nd);
            }
            else if (node.Name == "relation")
            {
                Relation r = readRelation(node);
                osmxml.relationList.Add(r);
                if (isBuildingRelation(r))
                    osmxml.buildingRelations.Add(generateBuildingRelation(r));
            }
            else if (node.Name == "bounds")
                osmxml.bbox = readBounds(node);
          
        }
       
        return osmxml;
    }

    public BBox readBBox(string filename)
    {
        //TextAsset osmfile = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        StreamReader osmfile = new StreamReader(filename);

        if (osmfile == null)
            Debug.Log("<color=red>Fatal error:</color>" + filename + " not set");

        XmlDocument xmlfile = new XmlDocument();
        //xmlfile.Load(new StringReader(osmfile.text));
        xmlfile.Load(osmfile);

        foreach (XmlNode node in xmlfile.DocumentElement.ChildNodes)
        {
            if (node.Name == "bounds")
                return readBounds(node);
        }

        return new BBox();
    }

    private BBox readBounds(XmlNode node)
    {
        BBox bb = new BBox();

        foreach (XmlAttribute attr in node.Attributes)
        {
            if (attr.Name == "minlat")
                bb.bottom = float.Parse(attr.Value);
            else if (attr.Name == "minlon")
                bb.left = float.Parse(attr.Value);
            else if (attr.Name == "maxlat")
                bb.top = float.Parse(attr.Value);
            else if (attr.Name == "maxlon")
                bb.right = float.Parse(attr.Value);
        }
        return bb;
    }

    private Node readNode(XmlNode node)
    {
        Node nd = new Node();

       foreach(XmlAttribute attr in node.Attributes)
       {
           if (attr.Name == "id")
               nd.id = attr.Value;
           else if (attr.Name == "lat")
               nd.lat = float.Parse(attr.Value);
           else if (attr.Name == "lon")
               nd.lon = float.Parse(attr.Value);
       }

       if(node.HasChildNodes)
       {
           nd.tags = new List<Tag>();    
           foreach(XmlNode child in node.ChildNodes)
           {
               if (child.Name == "tag")
                   nd.tags.Add(readTag(child));
           }
       }

       nd.type = ItemEnumerator.getNodeType(nd);

       return nd;
    }

    private Way readWay(XmlNode way)
    {
        Way w = new Way();
        w.isArea = false;
        w.id = way.Attributes[0].Value;
        w.nodes = new List<Node>();
        w.tags = new List<Tag>();

        foreach(XmlNode child in way.ChildNodes)
        {
            if(child.Name == "nd")
            {
                string refid = child.Attributes[0].Value;
                w.nodes.Add(nodeList.Find(item => item.id == refid));
            }
            else if (child.Name == "tag")
            {
                w.tags.Add(readTag(child));
                if (w.tags[w.tags.Count-1].k == "area")
                    w.isArea = true;
            }
        }

        w.type = ItemEnumerator.getWayTpe(w);

        return w;
    }

    private Relation readRelation(XmlNode relation)
    {
        Relation r = new Relation();
        
        r.id = relation.Attributes[0].Value;
        r.members = new List<Member>();
        r.tags = new List<Tag>();

        foreach(XmlNode child in relation.ChildNodes)
        {
            if (child.Name == "member")
            {
                Member mem = new Member(); 

                foreach(XmlAttribute a in child.Attributes)
                {
                    if (a.Name == "type")
                        mem.type = a.Value;
                    else if (a.Name == "ref")
                        mem.ref_ = a.Value;
                    else if (a.Name == "role")
                        mem.role = a.Value;
                }
                r.members.Add(mem);
            }
            else if (child.Name == "tag")
                r.tags.Add(readTag(child));
        }

        return r;
    }

    private Tag readTag(XmlNode tag)
    {
        Tag tg = new Tag();

        foreach(XmlAttribute a in tag.Attributes)
        {
            if (a.Name == "k")
                tg.k = a.Value;
            else if (a.Name == "v")
                tg.v = a.Value;
        }
        return tg;
    }

    private BuildingRelation generateBuildingRelation(Relation relation)
    {
        BuildingRelation buildingRelation = new BuildingRelation();  
        buildingRelation.tags = relation.tags;
        buildingRelation.innerHoles = new List<Way>();

        for (int k = 0; k < relation.members.Count; k++)
        {
            for(int j=0 ; j < wayList.Count ; j++)
            {
                if (relation.members[k].ref_ == wayList[j].id)
                {
                    if (relation.members[k].role == "inner")
                    {
                        buildingRelation.innerHoles.Add(wayList[j]);
                        buildingRelation.id = wayList[j].id;
                    }
                    else if (relation.members[k].role == "outer")
                    buildingRelation.outerWall = wayList[j];
                }
            }
        }


        for (int k = 0; k < relation.tags.Count; k++)
        {
            if (relation.tags[k].k == "building:levels")
                buildingRelation.buildingHeight = float.Parse(relation.tags[k].v) * 3.0f;
        }







            return buildingRelation;
    }

    private bool isBuildingRelation(Relation r)
    {
        for(int k=0 ; k < r.tags.Count ; k++)
        {
            if (r.tags[k].k == "building")
                return true;
        }

        return false;
    }


}




}
