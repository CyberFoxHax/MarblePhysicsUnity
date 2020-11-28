using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using UnityEngine;

namespace TS
{
    public class MissionImporter : MonoBehaviour
    {
        public string MissionPath;
        public List<TSObject> MissionObjects;

        public GameObject InteriorPrefab;
        public GameObject SkyboxPrefab;
        public GameObject GlobalMarble;
        
        void Start()
        {
            if (MissionPath.Length == 0)
            {
                return;
            }

            TSLexer lexer = new TSLexer(new AntlrFileStream(Path.Combine(Application.streamingAssetsPath, MissionPath)));
            TSParser parser = new TSParser(new CommonTokenStream(lexer));
            var file = parser.start();

            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.LogError("Could not parse!");
                return;
            }
            
            MissionObjects = new List<TSObject>();

            foreach (var decl in file.decl())
            {
                var objectDecl = decl.stmt()?.expression_stmt()?.stmt_expr()?.object_decl();
                if (objectDecl == null)
                {
                    continue;
                }

                if (MissionObjects.Count > 0)
                {
                    Debug.Log("Mission with two mission groups?");
                }
                
                MissionObjects.Add(ProcessObject(objectDecl));
            }

            if (MissionObjects.Count <= 0)
            {
                return;
            }
            
            var mis = MissionObjects[0];
            foreach (var obj in mis.RecursiveChildren())
            {
                if (obj.ClassName == "InteriorInstance")
                {
                    var gobj = Instantiate(InteriorPrefab, transform, false);

                    var positionParts = ParseVectorString(obj.GetField("position"));
                    var position = ConvertPoint(positionParts);

                    var rotationParts = ParseVectorString(obj.GetField("rotation"));
                    var rotation = ConvertRotation(rotationParts);

                    var scaleParts = ParseVectorString(obj.GetField("scale"));
                    var scale = ConvertScale(scaleParts);

                    gobj.transform.localPosition = position;
                    gobj.transform.localRotation = rotation;
                    gobj.transform.localScale = scale;

                    var difPath = ResolvePath(obj.GetField("interiorFile"), MissionPath);
                    gobj.GetComponent<Dif>().filePath = difPath;
                    gobj.GetComponent<Dif>().GenerateMesh();

                    GlobalMarble.GetComponent<Movement>().AddMesh(gobj.GetComponent<MeshCollider>());
                }

                if (obj.ClassName == "StaticShape" && obj.GetField("dataBlock") == "StartPad")
                {
                    var positionParts = ParseVectorString(obj.GetField("position"));
                    var position = ConvertPoint(positionParts);

                    var rotationParts = ParseVectorString(obj.GetField("rotation"));
                    var rotation = ConvertRotation(rotationParts);

                    var spawnPoint = position + rotation * new Vector3(0, 0, 3);
                    
                    GlobalMarble.transform.localPosition = spawnPoint;
                    GlobalMarble.GetComponent<Marble>().StartPoint = spawnPoint;

                    var skybox = Instantiate(SkyboxPrefab, transform, false);
                    skybox.transform.localPosition = spawnPoint;
                }
            }
        }

        private Vector3 ConvertPoint(float[] torquePoint)
        {
            return new Vector3(torquePoint[0], torquePoint[2], torquePoint[1]);
        }

        private Quaternion ConvertRotation(float[] torqueRotation)
        {
            // Torque point is an angle axis in torquespace
            float angle = torqueRotation[3];
            Vector3 axis = new Vector3(torqueRotation[0], -torqueRotation[1], torqueRotation[2]);
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            rot = Quaternion.Euler(-90.0f, 0, 0) * rot;
            return rot;
        }

        private Vector3 ConvertScale(float[] torqueScale)
        {
            return new Vector3(torqueScale[0], torqueScale[1], torqueScale[2]);
        }

        private string ResolvePath(string assetPath, string misPath)
        {
            // No funny business
            while (assetPath[0] == '/')
            {
                assetPath = assetPath.Substring(1);
            }

            switch (assetPath[0])
            {
                case '~' when misPath.Contains('/'):
                    return misPath.Substring(0, misPath.IndexOf('/')) + assetPath.Substring(1);
                case '~':
                    return assetPath.Substring(2);
                case '.':
                    return Path.GetDirectoryName(misPath) + assetPath.Substring(1);
                default:
                    return assetPath;
            }
        }

        private float[] ParseVectorString(string vs)
        {
            return vs.Split(' ').Select(float.Parse).ToArray();
        }
        
        TSObject ProcessObject(TSParser.Object_declContext objectDecl)
        {
            var retObj = ScriptableObject.CreateInstance<TSObject>();
            
            // Metadata
            retObj.ClassName = objectDecl.class_name_expr().GetText();
            retObj.Name = objectDecl.object_name().GetText();
            
            var fieldBlock = objectDecl.object_declare_block();
            if (fieldBlock == null)
            {
                return retObj;
            }
            
            // Fields
            foreach (var assignList in fieldBlock.slot_assign_list())
            {
                foreach (var slot in assignList.slot_assign())
                {
                    var key = slot.children[0].GetText();
                    var arrayValue = slot.aidx_expr()?.expr();
                    var value = slot.expr();

                    if (arrayValue != null)
                    {
                        for (int i = 0; i < arrayValue.Length; i++)
                        {
                            if (i > 0)
                            {
                                key += "_";
                            }

                            key += arrayValue[i].GetText();
                        }
                    }

                    var valueStr = value.GetText();
                    
                    var str = value.STRATOM();
                    if (str != null)
                    {
                        valueStr = str.GetText().Substring(1, valueStr.Length - 2);
                    }

                    retObj.Fields.Add(key.ToLower(), valueStr);
                }
            }

            // Sub Objects
            foreach (var subObject in fieldBlock.object_decl_list())
            {
                foreach (var subObjectDecl in subObject.object_decl())
                {
                    var child = ProcessObject(subObjectDecl);
                    child.Parent = retObj;
                    retObj.Children.Add(child);
                }
            }

            return retObj;
        }
    }
}