using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberScore.Classes
{
    public class NeuralNetwork
    {
        private Layer[] layers;
        public NeuralNetwork(int[] layers)
        {
            this.layers = new Layer[layers.Length - 1];
            for (int i = 0; i < this.layers.Length; ++i)
            {
                this.layers[i] = new Layer(layers[i], layers[i + 1]);
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
    }
}
