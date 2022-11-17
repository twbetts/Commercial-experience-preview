#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace FogOfWar
{
    /// <summary>
    ///@Thomas
    /// This Component should purely generate/setup the FOW, and not manage it at runtime.
    /// Please expand region above FogOfWarManager for explanation about access to vertices/DetectablePoints
    /// </summary>
    public class GenerateDetectablePoints : MonoBehaviour
    {
        /*
         * each vertice does not make a DetectablePoint
         * There can be more than one vertice at a given location
         * This script will generate a point for the first vertice.
         * And any additional vertices will link to the same point as other overlapping vertices
        */

        [Tooltip("The generated points will belong to this component")]
        [SerializeField] FogOfWarManager _fogOfWarManager;
        [Tooltip("Generate DetectablePoints based on this mesh")]
        [SerializeField] MeshFilter _filter;

        [Tooltip("the (local) offset for generated DetectablePoints")]
        [SerializeField] Transform _pointOffset;

        [Tooltip("Stores child GO's, many DetectablePoints")]
        [SerializeField] GameObject _fogOfWarPointsContainer;

        [ContextMenu("create transforms")]
        public void CreateTransformPoints()
        {
            DestroyContainer();
            CreateContainer();

            //get vertices from mesh
            if (_filter == null)
                _filter = GetComponent<MeshFilter>();
            Vector3[] vertices = _filter.sharedMesh.vertices;

            //get colors of mesh and set transparency
            Color[] colors = new Color[vertices.Length];
            _filter.sharedMesh.colors.CopyTo(colors, 0);
            SetBaseAlpha(colors);
            _filter.sharedMesh.colors = colors;

            VerticesToDetectablePoints(vertices);

            //assign to Manager
            _fogOfWarManager.colors = colors;
            _fogOfWarManager.meshFilter = _filter;

            void VerticesToDetectablePoints(Vector3[] positions)
            {
                _fogOfWarManager.points = new List<DetectablePoint>();

                //The position of a vertice is used to store the Point,
                //so other vertices with the same value can find the same Point
                Dictionary<Vector3, DetectablePoint> storedVertices = new Dictionary<Vector3, DetectablePoint>();

                for (int i = 0; i < positions.Length; i++)
                {
                    //When a point exists, add yourself to the point.
                    if (storedVertices.TryGetValue(positions[i], out DetectablePoint pointFound))
                    {
                        pointFound.linkedVerticesIndex.Add(i);
                    }
                    //Otherwise, make the point yourself.
                    else
                    {
                        CreateDetectablePoint(positions, storedVertices, i);
                    }
                }

            }

            void CreateDetectablePoint(Vector3[] positions, Dictionary<Vector3, DetectablePoint> storedVertices, int i)
            {
                //create GO, set parent
                var gameObject = new GameObject(i.ToString());
                gameObject.transform.parent = _fogOfWarPointsContainer.transform;

                //Add component and store point in manager
                var newPoint = gameObject.AddComponent<DetectablePoint>();
                _fogOfWarManager.points.Add(newPoint);

                //Set location of new DetectablePoint
                Vector3 localPos = positions[i] + _pointOffset.localPosition;
                newPoint.transform.localPosition = localPos;

                //Set rotation of new DetectablePoint
                newPoint.transform.localRotation = Quaternion.identity;

                //Setup DetectablePoint
                newPoint.linkedVerticesIndex.Add(i);
                newPoint.manager = _fogOfWarManager;

                storedVertices.Add(positions[i], newPoint);
            }

            static void SetBaseAlpha(Color[] colors)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    Color col = colors[i];
                    col.a = 0.1f; //Mostly transparent
                    colors[i] = col;
                }
            }
        }

        void CreateContainer()
        {
            _fogOfWarPointsContainer = new GameObject("FOW points container");
            Transform trans = _fogOfWarPointsContainer.transform;

            trans.parent = transform.parent;
            trans.localPosition = new Vector3();
            trans.localRotation = Quaternion.Euler(new Vector3());
            trans.localScale = new Vector3(1f, 1f, 1f);

            _fogOfWarPointsContainer.AddComponent<Gizmo_ShowChildren>().gizmoSize = 0.3f;
        }

        void DestroyContainer()
        {
            if (!_fogOfWarPointsContainer) return;

            GameObject.DestroyImmediate(_fogOfWarPointsContainer?.gameObject);
        }
    }
}
#endif