using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrainScript {
    public int inputAmount, outputAmount;
    public float[][] weights;
    public float[] biases;

    public CreatureBrainScript() {

    }

    public CreatureBrainScript(int inputAmount, int outputAmount) {
        this.inputAmount = inputAmount;
        this.outputAmount = outputAmount;

        // generate random initial weights
        this.weights = new float[inputAmount][];
        for (int i = 0; i < inputAmount; i++) {
            this.weights[i] = new float[outputAmount];
            for (int j = 0; j < outputAmount; j++) {
                this.weights[i][j] = Random.Range(-1f, 1f);
            }
        }

        // generate random initial biases
        this.biases = new float[outputAmount];
        for (int i = 0; i < outputAmount; i++) {
            this.biases[i] = Random.Range(-1f, 1f);
        }
    }

    public virtual float[] FeedForward(float[] inputSignals) {
        float[] brainOutput = new float[outputAmount];
        for (int j = 0; j < outputAmount; j++) {
            float neuronInput = 0f;
            for (int i = 0; i < inputAmount; i++) {
                neuronInput += inputSignals[i] * this.weights[i][j];
            }
            brainOutput[j] = Sigmoid(neuronInput) + this.biases[j];
        }

        return brainOutput;
    }

    public static float Sigmoid(float x) {
        return 1f / (1f - Mathf.Exp(-x));
    }

    public static float ReLu(float x) {
        return Mathf.Max(0, x);
    }
}
