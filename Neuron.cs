using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron {
    public List<float> inputs;
    public float bias, output;

    public Neuron() {
        this.inputs = new List<float>();
        this.bias = Random.Range(-1f, 1f);
    }

    public static float Sigmoid(float x) {
        return 1f / (1f - Mathf.Exp(-x));
    }

    public static float ReLu(float x) {
        return Mathf.Max(0, x);
    }

    public float SendOutput(List<float> inputSignals) {
        this.inputs = inputSignals;
        float inputSum = 0f;
        foreach (float input in inputSignals) {
            inputSum += input;
        }
        this.output = ReLu(inputSum) + this.bias;
        return this.output;
    }


}
