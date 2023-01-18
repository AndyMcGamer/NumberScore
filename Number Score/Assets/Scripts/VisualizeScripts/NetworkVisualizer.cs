using NumberScore.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace NumberScore.Graphics
{
    public enum VisualizeMode
    {
        NoHidden,
        OneHidden
    }

    struct LayerStruct
    {
        public int nodesIn;
        public int nodesOut;
    }
    
    public class NetworkVisualizer : MonoBehaviour
    {
        [SerializeField] private SimpleNeuralNetwork network;
        [SerializeField] private string layerSizes;
        [SerializeField] private VisualizeMode mode;
        [SerializeField] ComputeShader visualizationShader;
        public bool useGPU = false, render = false;
        [SerializeField] private Image image;
        [SerializeField] private Texture2D texture;
        private RenderTexture _target;
        [SerializeField] private int textureWidth;
        [SerializeField] private int textureHeight;
        [SerializeField] private Material cpuMaterial;
        [SerializeField] private Material gpuMaterial;
        [SerializeField] private Color32 goodColor;
        [SerializeField] private Color32 badColor;
        [SerializeField] private List<Slider> weightValues;
        [SerializeField] private List<Slider> biasValues;
        

        private void Awake()
        {
            ResetNetwork();
        }

        private void ResetNetwork()
        {
            List<int> sizes = new();
            string[] sizeList = layerSizes.Split(',');
            foreach (string s in sizeList)
            {
                sizes.Add(int.Parse(s));
            }
            network = new SimpleNeuralNetwork(sizes.ToArray());
            foreach (var item in weightValues)
            {
                item.value = 0;
            }
            foreach (var item in biasValues)
            {
                item.value = 0;
            }
            switch (mode)
            {
                case VisualizeMode.NoHidden:
                    for(int i = 4; i < weightValues.Count; ++i)
                    {
                        weightValues[i].gameObject.SetActive(false);
                    }
                    for(int i = 2; i < biasValues.Count; ++i)
                    {
                        biasValues[i].gameObject.SetActive(false);
                    }
                    break;
                case VisualizeMode.OneHidden:
                    foreach (var item in weightValues)
                    {
                        item.gameObject.SetActive(true);
                    }
                    foreach (var item in biasValues)
                    {
                        item.gameObject.SetActive(true);
                    }
                    break;
                default:
                    break;
            }
            Draw();
        }

        private void Update()
        {
            if (render) Draw();
        }

        private void InitRenderTexture()
        {
            if (_target == null || _target.width != textureWidth || _target.height != textureHeight)
            {
                // Release render texture if we already have one
                if (_target != null)
                    _target.Release();
                // Get a render target for Ray Tracing
                _target = new RenderTexture(textureWidth, textureHeight, 0,
                    RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                _target.enableRandomWrite = true;
                _target.Create();
            }
        }

        public void UpdateValues()
        {
            
            List<int> sizes = new();
            string[] sizeList = layerSizes.Split(',');
            foreach (string s in sizeList)
            {
                sizes.Add(int.Parse(s));
            }
            List<SimpleLayerData> data = new();
            for(int i = 1; i < sizes.Count; ++i)
            {
                double[][] weights = new double[sizes[i-1]][];
                
                int numBiases = sizes[i];
                double[] biases = new double[numBiases];
                for (int nodeIn = 0; nodeIn < sizes[i-1]; nodeIn++)
                {
                    weights[nodeIn] = new double[sizes[i]];
                    for (int nodeOut = 0; nodeOut < sizes[i]; nodeOut++)
                    {
                        weights[nodeIn][nodeOut] = weightValues[nodeIn * sizes[i-1] + nodeOut].value;
                    }
                }
                for (int bias = 0; bias < numBiases; bias++)
                {
                    biases[bias] = biasValues[bias].value;
                }
                SimpleLayerData layerData = new() { Weights = weights, Biases = biases};
                data.Add(layerData);
            }
            network.UpdateValues(data);
            render = true;
        }

        public void Draw()
        {
            if (useGPU) { RunShader(); }
            else { RunCompute(); }
            render = false;
        }

        private void RunShader()
        {
            InitRenderTexture();
            LayerStruct[] layerStructs = new LayerStruct[network.layers.Length];
            List<float> weights = new();
            List<float> biases = new();
            for(int i = 0; i < layerStructs.Length; i++)
            {
                layerStructs[i] = new LayerStruct() { nodesIn = network.layers[i].nodesIn, nodesOut = network.layers[i].nodesOut };
                for (int j = 0; j < network.layers[i].nodesOut; j++)
                {
                    for (int k = 0; k < network.layers[i].nodesIn; k++)
                    {
                        weights.Add((float)network.layers[i].weights[k][j]);
                    }
                }

                foreach (var bias in network.layers[i].biases)
                {
                    biases.Add((float)bias);
                }
                
            }
            int kernel = visualizationShader.FindKernel("CSMain");
            ComputeBuffer buffer = new ComputeBuffer(layerStructs.Length, sizeof(int) * 2);
            buffer.SetData(layerStructs);
            visualizationShader.SetBuffer(kernel, "layers", buffer);
            
            ComputeBuffer buffer1 = new ComputeBuffer(weights.Count, sizeof(float));
            buffer1.SetData(weights.ToArray());
            visualizationShader.SetBuffer(kernel, "Weights", buffer1);
            
            ComputeBuffer buffer2 = new ComputeBuffer(biases.Count, sizeof(float));
            buffer2.SetData(biases.ToArray());
            visualizationShader.SetBuffer(kernel, "Biases", buffer2);

            visualizationShader.SetTexture(kernel, "Result", _target);
            visualizationShader.SetInt("numLayers", layerStructs.Length);
            visualizationShader.Dispatch(kernel, 512/8, 512/8, 1);
            gpuMaterial.SetTexture("_MainTex", _target);
            image.material = gpuMaterial;
            buffer.Dispose();
            buffer1.Dispose();
            buffer2.Dispose();
        }

        private void RunCompute()
        {
            List<Color32> colors = new();
            for (int i = 0; i < texture.height; i++)
            {
                for (int j = 0; j < texture.width; j++)
                {
                    double[] inputs = { i/64d, j/64d };
                    if (network.Classify(inputs) == 0)
                    {
                        
                        colors.Add(badColor);
                    }
                    else
                    {
                        
                        colors.Add(goodColor);
                    }
                }
            }
            texture.SetPixels32(colors.ToArray());
            texture.Apply();
            cpuMaterial.SetTexture("_MainTex", texture);
            image.material = cpuMaterial;
        }
    }
}
