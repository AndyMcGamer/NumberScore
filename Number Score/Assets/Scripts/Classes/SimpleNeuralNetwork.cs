using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberScore.Classes
{
    [System.Serializable]
    public class SimpleNeuralNetwork
    {
        public SimpleLayer[] layers;
        public SimpleNeuralNetwork(int[] layers)
        {
            this.layers = new SimpleLayer[layers.Length - 1];
            for (int i = 0; i < this.layers.Length; ++i)
            {
                this.layers[i] = new SimpleLayer(layers[i], layers[i + 1]);
            }
        }

        public double[] CalculateOutputs(double[] inputs)
        {
            for (int i = 0; i < layers.Length; ++i)
            {
                inputs = layers[i].CalculateOutputs(inputs);
            }
            return inputs;
        }

        public int Classify(double[] inputs)
        {
            double[] output = CalculateOutputs(inputs);
            return IndexOfMax(output);
        }

        private int IndexOfMax(double[] arr)
        {
            double max = double.MinValue;
            int index = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > max)
                {
                    max = arr[i];
                    index = i;
                }
            }
            return index;
        }

        public void UpdateValues(List<SimpleLayerData> data)
        {
            for (int i = 0; i < data.Count; ++i) {
                layers[i].SetData(data[i]);
            }
        }
    }

    
}
