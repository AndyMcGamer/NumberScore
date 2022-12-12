using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberScore.Classes
{
    public class Layer
    {
        private Node[] nodes;
        private int nodesIn, nodesOut;
        public Layer(int numIn, int numOut)
        {
            nodesIn = numIn;
            nodesOut = numOut;
            nodes = new Node[numOut];
        }

        public double[] CalculateOutputs(double[] inputs)
        {
            double[] outputs = new double[nodesIn];
            for (int i = 0; i < nodesOut; ++i)
            {
                outputs[i] = nodes[i].CalculateOutput(inputs[i]);
            }
            return outputs;
        }
    }
}
