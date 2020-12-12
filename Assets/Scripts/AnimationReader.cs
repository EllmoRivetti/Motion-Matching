using MotionMatching.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

#region TreeParsing
class Node
{
    public string Key = String.Empty;
    public string[] Properties = new string[0];
    public Node Parent = null;
    public Node[] Childs = new Node[0];
    public Node(string k)
    {
        Key = k.Replace("\t", " ").Trim(' ');
    }
    public void AddChild(Node p)
    {
        Array.Resize(ref Childs, Childs.Length + 1);
        Childs[Childs.GetUpperBound(0)] = p;
    }
    public void AddProp(string t)
    {
        Array.Resize(ref Properties, Properties.Length + 1);
        Properties[Properties.GetUpperBound(0)] = t;
    }

    public Node[] GetNodeKey(string key)
    {
        Node[] tab = new Node[0];

        for(int i = 0; i < Childs.Length; i++)
        {
            if (Childs[i].Key.ToLower() == key.ToLower())
            {
                Array.Resize(ref tab, tab.Length + 1);
                tab[tab.GetUpperBound(0)] = Childs[i];
            }
        }
        return tab;
    }

    public string DisplayAllProperties()
    {
        string r = String.Empty;
        foreach (string p in Properties)
            r += p + Environment.NewLine;
        return r;
    }
    public string DisplayAllChilds()
    {
        string r = String.Empty;
        foreach (Node p in Childs)
            r += p.Key + Environment.NewLine;
        return r;
    }
    public override string ToString()
    {
        return $"Key : {Key} / Properties Count : {Properties.Length} / Childs count : {Childs.Length} / Parent name : {(Parent != null ? Parent.Key : "none")}";
    }
}

#endregion

public class AnimationReader : MonoBehaviour
{
    public enum RigBodyPartsRead
    {
        NOT_FOUND_PART,
        hip, abdomen, chest, neck, head, leftEye, rightEye, rCollar, rShldr, rForeArm, rHand, rThumb1, rThumb2, rIndex1, rIndex2,
        rMid1, rMid2, rRing1, rRing2, rPinky1,
        rPinky2, lCollar, lShldr, lForeArm, lHand, lThumb1, lThumb2, lIndex1, lIndex2,
        lMid1, lMid2, lRing1, lRing2, lPinky1, lPinky2, rButtock, rThigh, rShin, rFoot, lButtock, lThigh, lShin, lFoot
    }
    static readonly double FRAME_SIZE = 1924423250;
    Node currentNode = null;
    public string ObjectPath = "";

    void Start()
    {
        if (!string.IsNullOrEmpty(ObjectPath))
        {
            if (!ObjectPath.Contains("."))
                ObjectPath += ".fbx";
            //Debug.Log(Application.dataPath + "/Animations/" + ObjectPath);
            ReadFile(Application.dataPath + "/Animations/" + ObjectPath);


            
            Dictionary<RigBodyPartsRead, BoneData> a = GetBones(currentNode.GetNodeKey("Objects")[0]);
            //foreach (KeyValuePair<RigBodyPartsRead, BoneData> e in a)
            //    Debug.Log("Bone " + e.Key + $" : {e.Value.m_Position} {e.Value.m_Rotation} {e.Value.m_Scale}");
            DateTime tic = DateTime.Now;
            SortedDictionary<int, Dictionary<RigBodyPartsRead, BoneData>> b = GetFrameData(currentNode.GetNodeKey("Takes")[0].GetNodeKey("Take")[0]);
            DateTime toc = DateTime.Now;
            Debug.Log("Time elapsed : " + (toc - tic).ToString());
            Debug.Log("Main nodes : " + currentNode);
            Debug.Log("Bone dic size : " + a.Count);
            Debug.Log("Sorted dic size : " + b.Count);
            Debug.Log("Sorted dic 1 size : " + b[1].Count);

        }
    }

    SortedDictionary<int, Dictionary<RigBodyPartsRead, BoneData>> GetFrameData(Node takeNode)
    {
        SortedDictionary<int, Dictionary<RigBodyPartsRead, BoneData>> dic = new SortedDictionary<int, Dictionary<RigBodyPartsRead, BoneData>>();

        double keyValue;
        Node[] models, trs, aChannel;
        #region GetFramesCount
        double highestKeyCount = 0;
        foreach (Node model in takeNode.GetNodeKey("Model"))
        {
            Node channel = model.GetNodeKey("Channel")[0];
            trs = channel.GetNodeKey("Channel");
            for (int i = 0; i < trs.Length; i++) //trs
            {
                aChannel = trs[i].GetNodeKey("Channel");
                for (int j = 0; j < aChannel.Length; j++) //xyz
                {
                    string[] keys = aChannel[j].GetNodeKey("Key")[0].Properties;
                    for(int k = 0; k < keys.Length; k++)
                    {
                        keyValue = double.Parse(keys[k].Split(',')[0]);
                        if (keyValue > highestKeyCount)
                            highestKeyCount = keyValue;
                    }
                }
            }
        }
        #endregion

        // 1 + to start from 1
        int frames = 1 + (int)(highestKeyCount / FRAME_SIZE);
        Debug.Log(frames);
        double currentFrame;
        BoneData boneData;
        RigBodyPartsRead rigBodyParts;
        Dictionary<RigBodyPartsRead, BoneData> newDic;

        // The extremely consuming task : should go from 1 to frames
        for (int i = 1; i <= frames; i++)
        {
            currentFrame = (i - 1) * FRAME_SIZE;

            newDic = new Dictionary<RigBodyPartsRead, BoneData>();
            models = takeNode.GetNodeKey("Model");
            for(int j = 0; j < models.Length; j++)
            {
                rigBodyParts = GetRigFromName(models[j].Properties[1].Split('\"')[0]);
                //Debug.Log(rigBodyParts); // DEBUG
                if(rigBodyParts != RigBodyPartsRead.NOT_FOUND_PART)
                {
                    Node channel = models[j].GetNodeKey("Channel")[0];
                    trs = channel.GetNodeKey("Channel");
                    boneData = new BoneData();
                    for(int k = 0; k < trs.Length; k++) // trs
                    {
                        string currentTRS = trs[k].Properties[0].Replace("\"", string.Empty);
                        aChannel = trs[k].GetNodeKey("Channel");
                        for(int l = 0; l < aChannel.Length; l++) // xyz
                        {
                            string currentXYZ = aChannel[l].Properties[0].Replace("\"", string.Empty);


                            string[] keys = aChannel[l].GetNodeKey("Key")[0].Properties;
                            bool found = false;
                            // La boucle qui consomme le plus (puisqu'exécuté plein de fois
                            for (int m = 0; m < keys.Length; m++)
                            {
                                string[] splitted = keys[m].Split(',');
                                keyValue = double.Parse(splitted[0]);
                                if(keyValue == currentFrame)
                                {
                                    found = true;
                                    boneData = SetData(boneData, currentTRS, currentXYZ, float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat));
                                    break;
                                }
                            }

                            if (!found)
                            {
                                if (i == 1)
                                    boneData = SetData(boneData, currentTRS, currentXYZ, float.Parse(aChannel[l].GetNodeKey("Default")[0].Properties[0], CultureInfo.InvariantCulture.NumberFormat));
                                else
                                {
                                    //Debug.Log(rigBodyParts + " : " + currentTRS + " " + currentXYZ);
                                    boneData = SetData(boneData, currentTRS, currentXYZ, GetValue(dic, rigBodyParts, currentTRS, currentXYZ, i - 1));
                                }
                            }
                            else
                                aChannel[l].GetNodeKey("Key")[0].Properties = keys.Skip(1).ToArray();



                        }
                    }
                    newDic.Add(rigBodyParts, boneData);
                }

            }
            dic.Add(i, newDic);
        }
        return dic;
    }
    
    BoneData SetData(BoneData currentData, string trs, string xyz, float value)
    {
        if (trs == "T" && xyz == "X")
            currentData.m_Position.x = value;
        if (trs == "T" && xyz == "Y")
            currentData.m_Position.y = value;
        if (trs == "T" && xyz == "Z")
            currentData.m_Position.z = value;
        if (trs == "R" && xyz == "X")
            currentData.m_Rotation.x = value;
        if (trs == "R" && xyz == "Y")
            currentData.m_Rotation.y = value;
        if (trs == "R" && xyz == "Z")
            currentData.m_Rotation.z = value;
        if (trs == "S" && xyz == "X")
            currentData.m_Scale.x = value;
        if (trs == "S" && xyz == "Y")
            currentData.m_Scale.y = value;
        if (trs == "S" && xyz == "Z")
            currentData.m_Scale.z = value;
        return currentData;
    }
    float GetValue(SortedDictionary<int, Dictionary<RigBodyPartsRead, BoneData>> dic, RigBodyPartsRead r, string trs, string xyz, int frame)
    {

        //Debug.Log($"In set data {r} frame {frame} : {trs} {xyz}");
        Dictionary<RigBodyPartsRead, BoneData> currentDic = dic[frame];
        foreach(KeyValuePair<RigBodyPartsRead, BoneData> pair in currentDic)
        {
            if(pair.Key == r)
            {
                //Debug.Log($"In set data : {pair.Key} {trs} {xyz} / " + pair.Value.m_Position + " - " + pair.Value.m_Rotation + " - " + pair.Value.m_Scale);
                if (trs == "T" && xyz == "X")
                    return pair.Value.m_Position.x;
                if (trs == "T" && xyz == "Y")
                    return pair.Value.m_Position.y;
                if (trs == "T" && xyz == "Z")
                    return pair.Value.m_Position.z;
                if (trs == "R" && xyz == "X")
                    return pair.Value.m_Rotation.x;
                if (trs == "R" && xyz == "Y")
                    return pair.Value.m_Rotation.y;
                if (trs == "R" && xyz == "Z")
                    return pair.Value.m_Rotation.z;
                if (trs == "S" && xyz == "X")
                    return pair.Value.m_Scale.x;
                if (trs == "S" && xyz == "Y")
                    return pair.Value.m_Scale.y;
                if (trs == "S" && xyz == "Z")
                    return pair.Value.m_Scale.z;
            }
        }
        Debug.LogError("Bug GetValue");
        return 0f;
    }
    Dictionary<RigBodyPartsRead, BoneData> GetBones(Node objectNode)
    {
        Dictionary<RigBodyPartsRead, BoneData> dic = new Dictionary<RigBodyPartsRead, BoneData>();
        Node[] models = objectNode.GetNodeKey("Model");
        for(int i = 0; i < models.Length; i++)
        {
            string part = models[i].Properties[1].Split('\"')[0];
            RigBodyPartsRead enumPart = GetRigFromName(part);
            if (enumPart != RigBodyPartsRead.NOT_FOUND_PART)
            {
                Node properties = models[i].GetNodeKey("Properties60")[0];
                Node[] props = properties.GetNodeKey("Property");
                Vector3 t = Vector3.zero;
                Vector3 r = Vector3.zero;
                Vector3 s = Vector3.zero;
                for(int j = 0; j < props.Length; j++)
                {
                    if (props[j].Properties[0].Contains("Lcl Translation"))
                    {
                        List<string> strs = new List<string>(props[j].Properties[0].Split(','));
                        t = new Vector3(float.Parse(strs[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[4], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[5], CultureInfo.InvariantCulture.NumberFormat));
                    }
                    if (props[j].Properties[0].Contains("Lcl Rotation"))
                    {
                        List<string> strs = new List<string>(props[j].Properties[0].Split(','));
                        r = new Vector3(float.Parse(strs[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[4], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[5], CultureInfo.InvariantCulture.NumberFormat));
                    }
                    if (props[j].Properties[0].Contains("Lcl Scaling"))
                    {
                        List<string> strs = new List<string>(props[j].Properties[0].Split(','));
                        s = new Vector3(float.Parse(strs[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[4], CultureInfo.InvariantCulture.NumberFormat), float.Parse(strs[5], CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
                BoneData bone = new BoneData();
                bone.m_Position = t;
                bone.m_Rotation = r;
                bone.m_Scale = s;
                  
                dic.Add(enumPart, bone);
            }
        }


        return dic;
    }
    RigBodyPartsRead GetRigFromName(string name)
    {
        if (Enum.IsDefined(typeof(RigBodyPartsRead), name))
            return (RigBodyPartsRead)Enum.Parse(typeof(RigBodyPartsRead), name);
        return RigBodyPartsRead.NOT_FOUND_PART;
    }
    void ReadFile(string path)
    {
        using (var fileStream = File.OpenRead(path))
        {
            currentNode = new Node("path");
            bool waitNextLines = false;
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (!line.Contains(";"))
                    {
                        if (line.Contains("{"))
                        {
                            if (waitNextLines)
                            {
                                waitNextLines = false;
                                currentNode = currentNode.Parent;
                            }
                            string cLine = line.Split('{')[0];
                            List<string> splitPts = new List<string>(cLine.Split(':'));

                            Node p = new Node(splitPts[0]);
                            if (splitPts.Count >= 2)
                                for(int i = 1; i < splitPts.Count; i++)
                                    if(!string.IsNullOrWhiteSpace(splitPts[i]))
                                        p.AddProp(splitPts[i].Replace("\t", " ").Trim(' '));
                            p.Parent = currentNode;
                            currentNode.AddChild(p);
                            currentNode = p;

                        }
                        else if (line.Contains("}"))
                        {
                            currentNode = currentNode.Parent;
                        } 
                        else if(line.Contains(":"))
                        {
                            if (waitNextLines)
                            {
                                waitNextLines = false;
                                currentNode = currentNode.Parent;
                            }
                            List<string> splitPts = new List<string>(line.Split(':'));
                            Node node = new Node(splitPts[0]);
                            if (splitPts.Count >= 2)
                                for (int i = 1; i < splitPts.Count; i++)
                                    if (!string.IsNullOrWhiteSpace(splitPts[i]))
                                        node.AddProp(splitPts[i].Replace("\t", " ").Trim(' '));
                            else
                                waitNextLines = true;

                            node.Parent = currentNode;
                            currentNode.AddChild(node);
                            if (waitNextLines)
                                currentNode = node;

                        }
                        else if(!string.IsNullOrWhiteSpace(line) && !line.Contains(":") && waitNextLines)
                        {
                            currentNode.AddProp(line.Replace("\t", " ").Trim(' '));
                        }
                    }
                }
            }
        }


        Node[] tab = new Node[2];
        tab[0] = currentNode.GetNodeKey("Objects")[0];
        tab[1] = currentNode.GetNodeKey("Takes")[0];
        currentNode.Childs = tab;
    }

}
