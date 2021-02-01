using Assets.Helpers;
using MotionMatching.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

#region TreeParsing
[Serializable]
public class Node
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

public static class AnimationReader
{
    static readonly double FRAME_SIZE = 1924423250;
	public static SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> GetFrameData(string path, bool forceRead = false)
    {
        #region FileStuff
        string[] sp = path.Split('/');
        string fileName = sp[sp.Length - 1].Split('.')[0];
        string savePath = Path.Combine(Application.dataPath, "Saves", fileName + ".bin");
        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector ss = new SurrogateSelector();

        Vector3SerializationSurrogate v3_ss = new Vector3SerializationSurrogate();
        ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3_ss);
        bf.SurrogateSelector = ss;
        #endregion

        SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> dic = new SortedDictionary<int, Dictionary<RigBodyParts, BoneData>>();

        if (File.Exists(savePath) && !forceRead)
        {
            using (FileStream fs = File.OpenRead(savePath))
                dic = (SortedDictionary<int, Dictionary<RigBodyParts, BoneData>>)bf.Deserialize(fs);
            //Debug.Log("File loaded from save !");
        } else
        {
            Node takeNode = ReadFile(path);

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
                        for (int k = 0; k < keys.Length; k++)
                        {
                            keyValue = double.Parse(keys[k].Split(',')[0]);
                            if (keyValue > highestKeyCount)
                                highestKeyCount = keyValue;
                        }
                    }
                }
            }
            #endregion

            #region inits
            // 1 + to start from 1
            int frames = 1 + (int)(highestKeyCount / FRAME_SIZE);
            Debug.Log(frames);
            double currentFrame;
            BoneData boneData;
            RigBodyParts rigBodyParts;
            Dictionary<RigBodyParts, BoneData> newDic;
            #endregion

            // the consuming loop
            for (int i = 1; i <= frames; i++)
            {
                currentFrame = (i - 1) * FRAME_SIZE;

                newDic = new Dictionary<RigBodyParts, BoneData>();
                models = takeNode.GetNodeKey("Model");
                for (int j = 0; j < models.Length; j++)
                {
                    rigBodyParts = GetRigFromName(models[j].Properties[1].Split('\"')[0]);
                    //Debug.Log(rigBodyParts); // DEBUG
                    if (rigBodyParts != RigBodyParts.NOT_FOUND_PART)
                    {
                        Node channel = models[j].GetNodeKey("Channel")[0];
                        trs = channel.GetNodeKey("Channel");
                        boneData = new BoneData();
                        for (int k = 0; k < trs.Length; k++) // trs
                        {
                            string currentTRS = trs[k].Properties[0].Replace("\"", string.Empty); // T or R or S
                            aChannel = trs[k].GetNodeKey("Channel");
                            for (int l = 0; l < aChannel.Length; l++) // xyz
                            {
                                string currentXYZ = aChannel[l].Properties[0].Replace("\"", string.Empty); // X or Y or Z
                                string[] keys = aChannel[l].GetNodeKey("Key")[0].Properties;
                                bool found = false;

                                if (keys.Length > 0)
                                {
                                    string[] splitted = keys[0].Split(',');
                                    keyValue = double.Parse(splitted[0]);
                                    if (keyValue == currentFrame)
                                    {
                                        boneData = SetData(boneData, currentTRS, currentXYZ, float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat));
                                        aChannel[l].GetNodeKey("Key")[0].Properties = keys.Skip(1).ToArray();
                                        found = true;
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
                            }
                        }
                        newDic.Add(rigBodyParts, boneData);
                    }
                }
                dic.Add(i, newDic);
            }

            #region SaveFile
            FileStream file = File.Create(savePath);



            bf.Serialize(file, dic);
            file.Close();
            #endregion

        }



        return dic;
    }
    static BoneData SetData(BoneData currentData, string trs, string xyz, float value)
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
    static float GetValue(SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> dic, RigBodyParts r, string trs, string xyz, int frame)
    {
        //Debug.Log($"In set data {r} frame {frame} : {trs} {xyz}");
        Dictionary<RigBodyParts, BoneData> currentDic = dic[frame];
        foreach(KeyValuePair<RigBodyParts, BoneData> pair in currentDic)
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
    
    static RigBodyParts GetRigFromName(string name)
    {
        if (Enum.IsDefined(typeof(RigBodyParts), name))
            return (RigBodyParts)Enum.Parse(typeof(RigBodyParts), name);
        return RigBodyParts.NOT_FOUND_PART;
    }
    static Node ReadFile(string path)
    {
        string[] splitted = path.Split('/');
        string fileName = splitted[splitted.Length - 1].Split('.')[0];

        Node currentNode = new Node(fileName);

        using (var fileStream = File.OpenRead(path))
        {
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
                                for (int i = 1; i < splitPts.Count; i++)
                                    if (!string.IsNullOrWhiteSpace(splitPts[i]))
                                        p.AddProp(splitPts[i].Replace("\t", " ").Trim(' '));
                            p.Parent = currentNode;
                            currentNode.AddChild(p);
                            currentNode = p;

                        }
                        else if (line.Contains("}"))
                        {
                            currentNode = currentNode.Parent;
                        }
                        else if (line.Contains(":"))
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
                        else if (!string.IsNullOrWhiteSpace(line) && !line.Contains(":") && waitNextLines)
                        {
                            currentNode.AddProp(line.Replace("\t", " ").Trim(' '));
                        }
                    }
                }
            }
            currentNode = currentNode.GetNodeKey("Takes")[0].GetNodeKey("Take")[0];
        }

        return currentNode;
	}

}
