using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberScore.Classes
{
    public class Node
    {
        private double weight, bias;

        public Node()
        {
            weight = 0;
            bias = 0;
        }

        public double CalculateOutput(double input)
        {
            return bias + weight * input;
        }
    }
}
