using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PolyPizza;
using UnityEngine;
using UnityEngine.Serialization;

namespace PolyPizza
{
    [RequireComponent(typeof(APIManager))]
    public class PedestalSpawner : MonoBehaviour
    {
        public int width;
        public int height;
        public float spacing;

        [FormerlySerializedAs("platform")] [Tooltip("Pedestal object for models to spawn on.")]
        public GameObject pedestal;

        private List<Transform> platforms = new List<Transform>();
        private Transform[,] plane;

        private float platformObjWidth;
        // [SerializeField] private float spawnerDelaySpacing = 1;
        // private float platformObjHeight;

        [Tooltip("Model IDs to exclude from the spawner.")]
        public string[] Exclude;

        private static readonly int Metallic = Shader.PropertyToID("metallicFactor");
        private static readonly int Roughness = Shader.PropertyToID("roughnessFactor");

        void Awake()
        {
            //Create a bunch of platforms for the models to spawn on 
            plane = new Transform[width, height];
            platformObjWidth = pedestal.GetComponent<BoxCollider>().size.x;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var platform = Instantiate(pedestal,
                        transform.position + new Vector3((((platformObjWidth + spacing) * i)), 0,
                            (((platformObjWidth + spacing) * j))),
                        Quaternion.identity);
                    platforms.Add(platform.transform);
                    plane[i, j] = platform.transform;
                    platform.transform.SetParent(transform); //Child to the spawner
                }
            }

            //See if we can hit floor
            foreach (Transform t in platforms.ToList())
            {
                if (Physics.SphereCast(t.position, platformObjWidth / 4, Vector3.down, out RaycastHit hit))
                {
                    if (!hit.collider.tag.Equals("Ground"))
                    {
                        Destroy(t.gameObject);
                        platforms.Remove(t);
                    }
                    else
                    {
                        t.position = hit.point;
                    }
                }
                else
                {
                    Destroy(t.gameObject);
                    platforms.Remove(t);
                }
            }
        }

        private void Start()
        {
            Spawn();
        }

        private async void Spawn()
        {
            var models = await APIManager.instance.GetPopular(platforms.Count + Exclude.Length, APIManager.Category.Animals);

            //exclude models
            models = Array.FindAll(models, (Model m) => !Exclude.Contains(m.Id)).ToArray();

            //Remove high poly models
            models = Array.FindAll(models, (Model m) => m.TriCount <= 8000).ToArray();

            var modelTaskDone = new bool[models.Length];
            var order = spiralOrder(plane); //Spawn them in a spiral because fuck it we fancy
            order.Reverse(); //start with most popular in center
            int _nextPlat = 0;
            for (int i = 0; i < platforms.Count; i++)
            {
                if (i >= models.Length) continue;
                GameObject model =
                    await APIManager.instance.MakeModel(models[i], platformObjWidth * 0.9f, false); //Spawn the model
                if (model == null) continue; //Model is fucked
                ToggleRenderers(model, false);

                //Find a non null platform to use lol
                //platform.transform.GetChild(0) is the platform pivot where the model is spawned
                //so we're checking if we've spawned something into a platform before
                var platform = order[_nextPlat];
                _nextPlat++;
                while (platform == null || platform.transform.GetChild(0).childCount > 0)
                {
                    platform = order[_nextPlat];
                    _nextPlat++;
                }

                //without SyncTransforms() the box collider hasn't been updated in physics will fuck up the placement
                Physics.SyncTransforms();
                var modelCollider = model.GetComponent<BoxCollider>();

                var newPiv = new GameObject(models[i].Id + " Pivot");
                newPiv.transform.SetParent(platform.transform.GetChild(0));

                //Make a pivot for the model on the bottom center face of the bounds
                newPiv.transform.position =
                    modelCollider.bounds.center + (Vector3.down * modelCollider.bounds.size.y / 2);
                model.transform.SetParent(newPiv.transform, true);

                newPiv.transform.localPosition = Vector3.zero;
                var rot = APIManager.OrbitToRotation(models[i].Orbit);
                newPiv.transform.rotation = rot;
                ToggleRenderers(model, true);
                MakeMatte(model);

                //Play animation
                if (model.TryGetComponent(out Animation anim))
                {
                    anim.Play();
                }

                modelTaskDone[i] = true;
            }
        }

        private void ToggleRenderers(GameObject obj, bool on)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = on;
            }
        }

        private void MakeMatte(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    mat.SetFloat(Metallic, 0);
                    mat.SetFloat(Roughness, 1);
                }
            }
        }

        //https://www.geeksforgeeks.org/print-a-given-matrix-in-spiral-form/
        public static List<T> spiralOrder<T>(T[,] matrix)
        {
            List<T> ans = new List<T>();

            if (matrix.Length == 0)
                return ans;

            int R = matrix.GetLength(0), C = matrix.GetLength(1);
            bool[,] seen = new bool[R, C];
            int[] dr = { 0, 1, 0, -1 };
            int[] dc = { 1, 0, -1, 0 };
            int r = 0, c = 0, di = 0;

            // Iterate from 0 to R * C - 1
            for (int i = 0; i < R * C; i++)
            {
                ans.Add(matrix[r, c]);
                seen[r, c] = true;
                int cr = r + dr[di];
                int cc = c + dc[di];

                if (0 <= cr && cr < R && 0 <= cc && cc < C
                    && !seen[cr, cc])
                {
                    r = cr;
                    c = cc;
                }
                else
                {
                    di = (di + 1) % 4;
                    r += dr[di];
                    c += dc[di];
                }
            }

            return ans;
        }
    }
}