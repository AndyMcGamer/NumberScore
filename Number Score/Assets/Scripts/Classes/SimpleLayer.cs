using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberScore.Classes
{
    [System.Serializable]
    public class SimpleLayer
    {
        public double[][] weights;
        public double[] biases;
        public int nodesIn, nodesOut;
        public SimpleLayer(int numIn, int numOut)
        {
            nodesIn = numIn;
            nodesOut = numOut;
            weights = new double[nodesIn][];
            for (int i = 0; i < nodesIn; ++i)
            {
                weights[i] = new double[nodesOut];
            }
            biases = new double[nodesOut];
        }

        public double[] CalculateOutputs(double[] inputs)
        {
            double[] outputs = new double[nodesOut];
            ProcessNodes(inputs, ref outputs);
            return outputs;
        }

        private void ProcessNodes(double[] inputs, ref double[] outputs)
        {
            for (int i = 0; i < nodesOut; i++)
            {
                double output = biases[i];
                for (int j = 0; j < nodesIn; j++)
                {
                    output += inputs[j] * weights[j][i];
                }
                outputs[i] = output;
            }

            for (int i = 0; i < nodesOut; i++)
            {
                outputs[i] = 1 / (1 + System.Math.Exp(-outputs[i]));
            }
        }

        public void SetData(SimpleLayerData data)
        {
            weights = data.Weights;
            biases = data.Biases;
        }
    }

    public struct SimpleLayerData
    {
        public double[][] Weights { get; set; }
        public double[] Biases { get; set; }
    }
}
